using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using TestClient.Net.Packets;
using TestClient.Util;

namespace TestClient.Net.Messages
{
	public enum MessageDirection
	{
		Inbound,
		Outbound
	}

	public static class MessageParser
	{
		public struct Field
		{
			public string Name { get; set; }
			public MemberParserType Type { get; set; }
			public int Offset { get; set; }
			public int Length { get; set; }
			public object Value { get; set; }
		}

		private static Dictionary<uint, MemberParser> incomingParsers;
		private static Dictionary<uint, MemberParser> outgoingParsers;
		private static Dictionary<string, MemberParser> typeParsers;

		public static Message Parse(FragmentedPacket packet, MessageDirection direction)
		{
			MemberParser parser = null;
			int pos = 0;
			uint type = packet.Buffer.Memory.Read<uint>(ref pos);

			if (direction == MessageDirection.Inbound)
				incomingParsers.TryGetValue(type, out parser);
			else
				outgoingParsers.TryGetValue(type, out parser);

			Message msg = new Message(type, packet, parser);

			return msg;
		}

		public static int CountFields(MemberParser parser)
		{
			int result = 0;

			MemberParser itr = parser;
			while (itr != null)
			{
				if (itr.MemberType == MemberParserType.Case)
					result += CountFields(itr.Child);
				else
					result++;
			}

			return result;
		}

		public static int PopulateFields(MessageStruct messageStruct)
		{
			int position = 0;
			int offset = messageStruct.Offset;
			Memory<byte> buffer = messageStruct.Data;
			MemberParser parser = messageStruct.Parser;
			return PopulateFields(messageStruct, parser, buffer, offset, ref position);
		}

		public static int PopulateFields(MessageStruct messageStruct, MemberParser parser, Memory<byte> buffer, int offset, ref int position)
		{
			while (parser != null)
			{
				if (parser.PreAlignment != 0)
				{
					int align = (position + offset) % parser.PreAlignment;
					//Debug.WriteLine($"PreAlign {parser.MemberName} offset {offset} position {position} align {align} alignCheck {parser.PreAlignment}");
					if (align > 0)
						position += parser.PreAlignment - align;
				}

				bool condition = CheckCondition(messageStruct, parser);
				if (condition)
				{
					int fieldOffset = position;
					object value = GetFieldValue(messageStruct, parser, buffer, ref position);
					if (value != null)
					{
						Field field = new Field();
						field.Name = parser.MemberName;
						field.Offset = fieldOffset;
						field.Type = parser.MemberType;
						field.Value = value;
						field.Length = position - fieldOffset;

						messageStruct.Fields.Add(field);
						messageStruct.FieldIndex.Add(field.Name, messageStruct.Fields.Count - 1);
					}
				}

				if (parser.PostAlignment != 0)
				{
					int align = (offset + position) % parser.PostAlignment;
					//Debug.WriteLine($"PostAlign {parser.MemberName} offset {offset} position {position} ({offset + position}) align {align} alignCheck {parser.PostAlignment}");
					if (align > 0)
						position += parser.PostAlignment - align;
				}

				parser = parser.Next;
			}

			return position;
		}

		private static object FindFieldValue(MessageStruct messageStruct, string fieldName)
		{
			object result = null;
			MessageStruct cs = messageStruct;
			while (cs != null)
			{
				result = cs[fieldName];
				if (result != null)
					return result;
				cs = cs.Parent;
			}
			return result;
		}

		private static bool CheckCondition(MessageStruct messageStruct, MemberParser parser)
		{
			bool result = true;
			object tmp = null;

			if (parser.Condition != MemberParserCondition.None)
			{
				tmp = FindFieldValue(messageStruct, parser.ConditionField);
				if (tmp != null)
				{
					long condCheck = Convert.ToInt64(tmp);
					condCheck ^= parser.ConditionXor;
					condCheck &= parser.ConditionAnd;
					switch (parser.Condition)
					{
						case MemberParserCondition.EQ:
							result = (condCheck == parser.ConditionResult);
							break;

						case MemberParserCondition.NE:
							result = (condCheck != parser.ConditionResult);
							break;

						case MemberParserCondition.GE:
							result = (condCheck >= parser.ConditionResult);
							break;

						case MemberParserCondition.GT:
							result = (condCheck > parser.ConditionResult);
							break;

						case MemberParserCondition.LE:
							result = (condCheck <= parser.ConditionResult);
							break;

						case MemberParserCondition.LT:
							result = (condCheck < parser.ConditionResult);
							break;

					}
				}
			}
			return result;
		}

		private static object GetFieldValue(MessageStruct current, MemberParser parser, Memory<byte> buffer, ref int position)
		{
			switch (parser.MemberType)
			{
				case MemberParserType.BYTE:
					return buffer.Read<byte>(ref position);

				case MemberParserType.WORD:
					return buffer.Read<short>(ref position);

				case MemberParserType.PackedWORD:
					{
						short tmp = buffer.Read<byte>(ref position);
						if ((tmp & 0x80) != 0)
							tmp = (short)(((tmp & 0x7f) << 8) | buffer.Read<byte>(ref position));
						return tmp;
					}

				case MemberParserType.DWORD:
					return buffer.Read<int>(ref position);

				case MemberParserType.PackedDWORD:
					{
						int tmp = buffer.Read<short>(ref position);
						if ((tmp & 0x8000) != 0)
							tmp = ((tmp & 0x7fff) << 16) + buffer.Read<short>(ref position);
						return tmp;
					}

				case MemberParserType.QWORD:
					return buffer.Read<long>(ref position);

				case MemberParserType.@float:
					return buffer.Read<float>(ref position);

				case MemberParserType.@double:
					return buffer.Read<double>(ref position);

				case MemberParserType.String:
					{
						//Debug.WriteLine($"Extracting String value {parser.MemberName}");
						// strings need to be a multiple of 4 including length
						int lenlen = 2;
						int length = buffer.Read<short>(ref position);
						if (length == -1)
						{
							lenlen += 4;
							length = buffer.Read<int>(ref position);
						}

						Memory<byte> tmp = buffer.Slice(position, length);
						position += length;

						int align = (position + current.Offset) % 4;
						if (align > 0)
							position += (4 - align);

						return System.Text.Encoding.ASCII.GetString(tmp.Span);
					}

				case MemberParserType.WString:
					{
						int lenlen = 1;
						int length = buffer.Read<byte>(ref position);
						if ((length & 0x80) != 0)
						{
							lenlen++;
							length = ((length & 0x7f) << 8) | buffer.Read<byte>(ref position);
						}

						Memory<byte> tmp = buffer.Slice(position, length * 2);

						position += length * 2;

						return System.Text.Encoding.Unicode.GetString(tmp.Span);
					}

				case MemberParserType.Struct:
					{
						Memory<byte> tmp = buffer.Slice(position);
						MessageStruct ms = new MessageStruct(tmp, current.Offset + position, parser.Child, current);
						position += ms.Length;
						return ms;
					}

				case MemberParserType.Vector:
					{
						object tmp = FindFieldValue(current, parser.LengthField);
						long length = 0;
						if (tmp != null)
						{
							length = Convert.ToInt64(tmp);
						}
						long mask = parser.LengthMask;
						length &= mask;
						if (mask != 0)
						{
							while ((mask & 0x1) == 0)
							{
								length >>= 1;
								mask >>= 1;
							}
						}

						Memory<byte> tmpData = buffer.Slice(position);
						MessageStruct ms = new MessageStruct(tmpData, current.Offset + position, parser.Child, (int)(length + parser.LengthDelta), current);
						position += ms.Length;
						return ms;
					}

				case MemberParserType.Case:
					PopulateFields(current, parser.Child, buffer, current.Offset, ref position);
					break;

				default:
					System.Diagnostics.Debug.WriteLine($"Invalid MemberParserType {parser.MemberType}");
					break;
			}

			return null;
		}

		public static void Initialize(string xmlFilePath)
		{
			incomingParsers = new Dictionary<uint, MemberParser>();
			outgoingParsers = new Dictionary<uint, MemberParser>();
			typeParsers = new Dictionary<string, MemberParser>();

			XDocument doc = XDocument.Load(xmlFilePath);
			foreach (XElement element in doc.XPathSelectElements("/schema/datatypes/type"))
				LoadTypes(element);

			foreach (XElement element in doc.XPathSelectElements("/schema/messages/message"))
				LoadMessages(element);
		}

		private static void LoadTypes(XElement element)
		{
			XAttribute attr = element.Attribute(XName.Get("name"));
			if (attr == null) return;

			string name = attr.Value;

			MemberParser parser = null;
			if (typeParsers.TryGetValue(name, out parser))
				return;

			parser = new MemberParser();
			typeParsers.Add(name, parser);

			attr = element.Attribute(XName.Get("primitive"));
			if (attr != null && bool.Parse(attr.Value))
			{
				parser.MemberType = (MemberParserType)Enum.Parse(typeof(MemberParserType), name);
			}
			else
			{
				attr = element.Attribute(XName.Get("parent"));
				if (attr != null)
				{
					parser.MemberType = (MemberParserType)Enum.Parse(typeof(MemberParserType), attr.Value);
				}
				else
				{
					parser.MemberType = MemberParserType.Struct;
					parser.Child = ParseStruct(element.Elements());
				}
			}
		}

		private static void LoadMessages(XElement element)
		{
			XAttribute attr = element.Attribute(XName.Get("type"));
			if (attr == null) return;

			uint type = uint.Parse(attr.Value, NumberStyles.HexNumber);

			bool inbound = true;
			bool outbound = false;
			attr = element.Attribute(XName.Get("direction"));
			if (attr != null)
			{
				if (string.Compare(attr.Value, "outbound", true, CultureInfo.CurrentCulture) == 0)
				{
					inbound = false;
					outbound = true;
				}
				else if (string.Compare(attr.Value, "both", true, CultureInfo.CurrentCulture) == 0)
				{
					outbound = true;
				}
			}
			if ((inbound && !incomingParsers.ContainsKey(type) ||
				outbound && !outgoingParsers.ContainsKey(type)))
			{
				MemberParser parser = ParseStruct(element.Elements());

				if (inbound)
					incomingParsers.TryAdd(type, parser);
				if (outbound)
					outgoingParsers.TryAdd(type, parser);
			}
		}

		private static MemberParser ParseStruct(IEnumerable<XElement> elements)
		{
			MemberParser root = new MemberParser();
			MemberParser current = root;
			MemberParser temp = null;

			XAttribute attr;
			XAttribute value;
			long longVal;

			foreach (XElement element in elements)
			{
				longVal = 0;
				XAttribute name = element.Attribute(XName.Get("name"));
				switch (element.Name.LocalName.ToLowerInvariant())
				{
					case "field":
						attr = element.Attribute(XName.Get("type"));
						if (attr != null && typeParsers.TryGetValue(attr.Value, out temp))
						{
							current.Next = new MemberParser(temp);
							current = current.Next;
							current.MemberName = name.Value;
						}
						break;

					case "mask":
						value = element.Attribute(XName.Get("value"));
						if (value != null)
						{
							current.Next = new MemberParser();
							current = current.Next;
							current.MemberType = MemberParserType.Case;
							current.ConditionAnd = long.Parse(value.Value.Substring(2), NumberStyles.HexNumber);
							current.Child = ParseStruct(element.Elements());
						}
						break;

					case "maskmap":
						attr = element.Attribute(XName.Get("xor"));
						if (attr != null)
						{
							longVal = long.Parse(attr.Value.Substring(2), NumberStyles.HexNumber);
						}
						current.Next = ParseStruct(element.Elements());
						while (current.Next != null)
						{
							current = current.Next;
							current.Condition = MemberParserCondition.NE;
							current.ConditionResult = 0;
							current.ConditionField = name.Value;
							current.ConditionXor = longVal;
						}
						break;

					case "case":
						value = element.Attribute(XName.Get("value"));
						if (value != null)
						{
							current.Next = new MemberParser();
							current = current.Next;
							current.MemberType = MemberParserType.Case;
							current.ConditionResult = long.Parse(value.Value.Substring(2), NumberStyles.HexNumber);
							current.Child = ParseStruct(element.Elements());
						}
						break;

					case "switch":
						attr = element.Attribute(XName.Get("mask"));
						longVal = -1;
						if (attr != null)
						{
							longVal = long.Parse(attr.Value.Substring(2), NumberStyles.HexNumber);
						}
						current.Next = ParseStruct(element.Elements());
						while (current.Next != null)
						{
							current = current.Next;
							current.Condition = MemberParserCondition.EQ;
							current.ConditionField = name.Value;
							current.ConditionAnd = longVal;
						}
						break;

					case "vector":
						current.Next = new MemberParser();
						current = current.Next;
						current.MemberType = MemberParserType.Vector;
						current.MemberName = name.Value;

						attr = element.Attribute(XName.Get("length"));
						current.LengthField = attr.Value;

						attr = element.Attribute(XName.Get("mask"));
						longVal = -1;
						if (attr != null)
						{
							longVal = long.Parse(attr.Value.Substring(2), NumberStyles.HexNumber);
						}
						current.LengthMask = longVal;

						attr = element.Attribute(XName.Get("skip"));
						longVal = 0;
						if (attr != null)
						{
							longVal = long.Parse(attr.Value);
						}
						current.LengthDelta = -(int)longVal;
						current.Child = ParseStruct(element.Elements());
						break;

					case "align":
						attr = element.Attribute(XName.Get("type"));
						switch (attr.Value.ToLowerInvariant())
						{
							case "word":
								current.PostAlignment = 2;
								break;

							case "dword":
								current.PostAlignment = 4;
								break;

							case "qword":
								current.PostAlignment = 8;
								break;
						}
						break;
				}
			}

			if (root.PostAlignment != 0 && root.Next != null)
				root.Next.PreAlignment = root.PostAlignment;

			return root.Next;
		}
	}
}

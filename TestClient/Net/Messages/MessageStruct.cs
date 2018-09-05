using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;

using TestClient.Util;

namespace TestClient.Net.Messages
{
	public class MessageStruct
	{
		internal Memory<byte> Data { get; private set; }
		internal MemberParser Parser { get; private set; }
		internal List<MessageParser.Field> Fields { get; private set; }
		internal Dictionary<string, int> FieldIndex { get; private set; }

		private bool parsed;
		private bool parsing;
		private int fieldCount;

		public int Length { get; private set; }
		public int Offset { get; private set; }
		public MessageStruct Parent { get; private set; }

		#region Ctor

		protected MessageStruct()
		{
			Fields = new List<MessageParser.Field>();
			FieldIndex = new Dictionary<string, int>();
		}

		protected MessageStruct(Memory<byte> data, int offset, MemberParser parser)
			: this()
		{
			Offset = offset;
			Data = data;
			Parser = parser;
			fieldCount = -1;
		}

		public MessageStruct(Memory<byte> data, int offset, MemberParser parser, MessageStruct parent)
			: this(data, offset, parser)
		{
			Parent = parent;
			Parse();
		}

		public MessageStruct(Memory<byte> data, int offset, MemberParser parser, int count, MessageStruct parent)
			: this(data, offset, parser)
		{
			Parent = parent;
			fieldCount = count;
			Parse();
		}

		#endregion

		#region Accessors

		public object this[string name]
		{
			get
			{
				Parse();
				if (FieldIndex.TryGetValue(name, out int index))
					return Fields[index].Value;
				return null;
			}
		}

		public MessageStruct Struct(string name)
		{
			Parse();
			if (FieldIndex.TryGetValue(name, out int index))
			{
				MemberParserType fieldType = Fields[index].Type;
				if (fieldType == MemberParserType.Struct || fieldType == MemberParserType.Vector)
					return (MessageStruct)Fields[index].Value;
			}
			return null;
		}

		public MessageStruct Struct(int index)
		{
			Parse();
			MemberParserType fieldType = Fields[index].Type;
			if (fieldType == MemberParserType.Struct || fieldType == MemberParserType.Vector)
				return (MessageStruct)Fields[index].Value;
			return null;
		}

		public T Value<T>(string name) where T : IConvertible
		{
			Parse();
			T result = default(T);
			if (FieldIndex.TryGetValue(name, out int index))
			{
				IConvertible c = Fields[index].Value as IConvertible;
				result = (T)c.ToType(typeof(T), CultureInfo.CurrentCulture);
				//result = (T)Fields[index].Value;
			}
			return result;
		}

		#endregion

		#region Parse

		protected void Parse()
		{
			if (parsed || parsing) return;
			parsing = true;
			if (fieldCount < 0)
				Length = MessageParser.PopulateFields(this);
			else
				ParseVector();
			parsed = true;
		}

		private void ParseVector()
		{
			int position = 0;
			for (int i = 0; i < fieldCount; i++)
			{
				Memory<byte> buffer = Data.Slice(position);
				MessageParser.Field field = new MessageParser.Field();
				field.Offset = position;
				field.Type = MemberParserType.Struct;

				MessageStruct ms = new MessageStruct(buffer, Offset + position, Parser, this);
				field.Value = ms;
				field.Length = ms.Length;
				position += ms.Length;

				Fields.Add(field);
			}
			Length = position;
		}

		#endregion
	}
}

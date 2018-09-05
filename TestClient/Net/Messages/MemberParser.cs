using System;

namespace TestClient.Net.Messages
{
	public enum MemberParserCondition
	{
		None = 0,
		EQ = 1,
		NE = 2,
		GE = 3,
		GT = 4,
		LE = 5,
		LT = 6
	}

	public enum MemberParserType
	{
		BYTE = 0,
		WORD = 1,
		PackedWORD = 2,
		DWORD = 3,
		PackedDWORD = 4,
		QWORD = 5,
		@float = 6,
		@double = 7,
		String = 8,
		WString = 9,
		Struct = 10,
		Vector = 11,
		Case = 12
	}

	public class MemberParser
	{
		public MemberParser Next;
		public MemberParser Child;
		public MemberParserType MemberType;
		public string MemberName;
		public MemberParserCondition Condition;
		public string ConditionField;
		public long ConditionXor;
		public long ConditionAnd;
		public long ConditionResult;
		public string LengthField;
		public long LengthMask;
		public int LengthDelta;
		public int PreAlignment;
		public int PostAlignment;

		public MemberParser()
		{
		}

		public MemberParser(MemberParser Source)
		{
			this.Next = Source.Next;
			this.Child = Source.Child;
			this.MemberType = Source.MemberType;
			this.MemberName = Source.MemberName;
			this.Condition = Source.Condition;
			this.ConditionField = Source.ConditionField;
			this.ConditionXor = Source.ConditionXor;
			this.ConditionAnd = Source.ConditionAnd;
			this.ConditionResult = Source.ConditionResult;
			this.LengthField = Source.LengthField;
			this.LengthMask = Source.LengthMask;
			this.LengthDelta = Source.LengthDelta;
			this.PreAlignment = Source.PreAlignment;
			this.PostAlignment = Source.PostAlignment;
		}
	}
}

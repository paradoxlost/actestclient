using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Runtime;

namespace TestClient.Scripts
{
	[Documentation("Test Client API Module")]
	public class ScriptApi// : IMembersList
	{
		private static readonly List<string> TypeMembers = new List<string>(new string[] { "__repr__", "MessageHandler" });

		[Documentation("bite me")]
		public IScriptMessageHandler MessageHandler { get; set; }

		// public ScriptApi()
		// {
		// 	MessageHandler = new IScriptMessageHandler();
		// }

		[SpecialName]
		public IList<string> GetMemberNames()
		{
			return TypeMembers;
		}
	}
}

using System;

using Microsoft.Scripting.Runtime;

namespace TestClient.Scripts
{
	[Documentation("Interface for connecting message handlers")]
	public interface IScriptMessageHandler
	{
		Action<uint, MessageDelegate> HandleMessage { get; }
		Action<uint, uint, MessageDelegate> HandleMessageEvent { get; }
		//void HandleMessage(uint type, MessageDelegate handler);
		//void HandleMessage(uint type, uint eventCode, MessageDelegate handler);
	}
}

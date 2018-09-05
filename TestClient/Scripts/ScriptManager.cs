using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

using IronPython.Hosting;

using TestClient.Net;
using TestClient.Net.Messages;
using TestClient.Ux;

namespace TestClient.Scripts
{
	public delegate void MessageDelegate(Message msg);

	public class ScriptManager : IDisposable, IScriptMessageHandler
	{
		public static ScriptManager Instance { get; private set; }

		private bool running;
		private Thread scriptThread;

		private ScriptEngine engine;
		private ScriptRuntime engineRuntime;
		private ScriptScope engineScope;

		private string rootPath;
		private string pyStdLibPath;

		private Dictionary<uint, MessageDelegate> messageHandlers;
		private Dictionary<uint, Dictionary<uint, MessageDelegate>> eventMessageHandlers;
		private ConcurrentQueue<Message> messageQueue;

		private NetworkManager networkManager;
		private DisplayManager displayManager;

		private ScriptApi scriptApi;

		#region Ctor / Dispose

		protected ScriptManager()
		{
			Instance = this;

			messageHandlers = new Dictionary<uint, MessageDelegate>();
			eventMessageHandlers = new Dictionary<uint, Dictionary<uint, MessageDelegate>>();
			messageQueue = new ConcurrentQueue<Message>();

			scriptApi = new ScriptApi();
			scriptApi.MessageHandler = (IScriptMessageHandler)this;
			//scriptApi.MessageHandler.HandleMessage = HandleMessage;
			//scriptApi.MessageHandler.HandleMessageEvent = HandleMessageEvent;

			scriptThread = new Thread(ScriptThreadStart);

			// it is possible to do this via configuration
			// i need to find an example to implement

			//engineRuntime = ScriptRuntime.CreateFromConfiguration();
			//engine = engineRuntime.GetEngine("python");
			engine = Python.CreateEngine();
			engineRuntime = engine.Runtime;

			engineRuntime.LoadAssembly(typeof(string).Assembly);
			engineRuntime.LoadAssembly(typeof(Uri).Assembly);
			engineRuntime.LoadAssembly(this.GetType().Assembly);

			//engineScope = engine.CreateModule("Api");
			//engineScope.SetVariable("MessageHandler", scriptApi.MessageHandler);

			// Dictionary<string, object> mh = new Dictionary<string, object>();
			// mh.Add("__all__", new string[] { "HandleMessage", "HandleMessageEvent" });
			// mh.Add("__dir__", new string[] { "HandleMessage", "HandleMessageEvent" });
			// mh.Add("__name__", "MessageHandler");
			// mh.Add("HandleMessage", ((IScriptMessageHandler)this).HandleMessage);
			// mh.Add("HandleMessageEvent", ((IScriptMessageHandler)this).HandleMessageEvent);

			//ScriptScope modScope = engine.CreateScope(mh);
			// modScope.SetVariable("HandleMessage", ((IScriptMessageHandler)this).HandleMessage);
			// modScope.SetVariable("HandleMessageEvent", ((IScriptMessageHandler)this).HandleMessageEvent);

			//Scope messageMod = HostingHelpers.GetScope(modScope);

			//modScope = engine.CreateScope();
			//modScope.SetVariable("MessageHandler", messageMod);

			//Scope apiModule = HostingHelpers.GetScope(modScope);

			//engineRuntime.Globals.SetVariable("Api", apiModule);


			engineScope = engineRuntime.CreateScope();

			scriptThread.Start();
		}

		public ScriptManager(string path, string pyLibPath)
			: this()
		{
			rootPath = path;
			pyStdLibPath = pyLibPath;

			ICollection<string> searchPaths = engine.GetSearchPaths();
			// TODO: option for python stdlib
			searchPaths.Add(pyStdLibPath);
			searchPaths.Add(System.IO.Path.GetFullPath(rootPath));
			engine.SetSearchPaths(searchPaths);

			Reload();
		}

		~ScriptManager()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			running = false;
			if (disposing)
			{

			}
		}

		#endregion

		#region Message Handler

		Action<uint, MessageDelegate> IScriptMessageHandler.HandleMessage => HandleMessage;
		Action<uint, uint, MessageDelegate> IScriptMessageHandler.HandleMessageEvent => HandleMessageEvent;

		public void HandleMessage(uint type, MessageDelegate handler)
		{
			Debug.WriteLine($"Adding handler for {type:X4}");
			if (messageHandlers.TryGetValue(type, out MessageDelegate handlers))
				handlers += handler;
			else
				messageHandlers.Add(type, handler);
		}

		public void HandleMessageEvent(uint type, uint eventCode, MessageDelegate handler)
		{
			Debug.WriteLine($"Adding handler for {type:X4}/{eventCode:X4}");
			Dictionary<uint, MessageDelegate> events = null;
			if (!eventMessageHandlers.TryGetValue(type, out events))
			{
				events = new Dictionary<uint, MessageDelegate>();
				eventMessageHandlers.Add(type, events);
			}

			if (events.TryGetValue(eventCode, out MessageDelegate handlers))
				handlers += handler;
			else
				events.Add(eventCode, handler);
		}

		private void OnMessageReceived(object sender, MessageEventArgs e)
		{
			//Debug.WriteLine($"{e.Message.Type:X4} Received");
			QueueMessage(e.Message);
		}

		#endregion

		public void SetDisplayManager(DisplayManager manager)
		{
			displayManager = manager;
			engineScope.SetVariable("Display", displayManager);
		}

		public void SetNetworkManager(NetworkManager manager)
		{
			if (networkManager != null)
			{
				networkManager.MessageReceived -= OnMessageReceived;
			}
			networkManager = manager;
			networkManager.MessageReceived += OnMessageReceived;
			//engineRuntime.Globals.SetVariable("Network", networkManager);
			//engineScope.SetVariable("Network", networkManager);
		}

		public void Reload()
		{
			messageHandlers.Clear();
			eventMessageHandlers.Clear();

			engineRuntime.ImportModule("api");

			foreach (string path in System.IO.Directory.GetFiles(rootPath))
			{
				try
				{
					Debug.WriteLine($"loading {path}");
					engine.ExecuteFile(path, engineScope);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"{ex.Message}");
				}
			}
		}

		public void QueueMessage(Message msg)
		{
			messageQueue.Enqueue(msg);
		}

		#region Script Thread

		private void ScriptThreadStart()
		{
			running = true;

			try
			{
				Debug.WriteLine("Network Loop Start");
				while (running)
				{
					// anything in the queue
					if (this.messageQueue.TryDequeue(out Message msg))
					{
						try
						{
							if (messageHandlers.TryGetValue(msg.Type, out MessageDelegate handler))
								handler(msg);

							// TODO: there are only two of these, just use an if?
							if (eventMessageHandlers.TryGetValue(msg.Type, out Dictionary<uint, MessageDelegate> handlers))
							{
								uint eventCode = msg.Value<uint>("event");
								if (handlers.TryGetValue(eventCode, out MessageDelegate eventHandler))
									eventHandler(msg);
							}
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"Error handling message {msg.Type:X4} : {ex.ToString()}");
						}
						msg.Dispose();
					}

					// let other things happen
					Thread.Yield();
				}
			}
			catch (ThreadAbortException)
			{
			}
		}

		#endregion

	}
}

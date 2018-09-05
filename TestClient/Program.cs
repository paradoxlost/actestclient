using System;
using System.Diagnostics;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

// https://github.com/migueldeicaza/gui.cs/
// https://blog.terribledev.io/Parsing-cli-arguments-in-dotnet-core-Console-App/
// http://adamsitnik.com/Array-Pool/

// https://books.google.com/books?id=QGciuyBRyU0C&pg=PA244&lpg=PA244&dq=dlr+import+class+globalscope&source=bl&ots=Nd-7yhaR_a&sig=JViTYgJ7ylYziMY-R0UKppEdkfA&hl=en&sa=X&ved=0ahUKEwiHlbSos67cAhVvUd8KHbv8A6IQ6AEIMjAB#v=onepage&q=dlr%20import%20class%20globalscope&f=false

using TestClient.Net;
using TestClient.Net.Messages;
using TestClient.Scripts;
using TestClient.Ux;

namespace TestClient
{
	class Program
	{
		private static CommandManager commandManager = null;
		private static DisplayManager displayManager = null;

		private static NetworkManager networkManager = null;
		private static ScriptManager scriptManager = null;

		private static IConfiguration config = null;

		private static void CreateNetworkManager(int port)
		{
			//networkManager = new NetworkManager(port);
			networkManager.Configure(port);
			scriptManager.SetNetworkManager(networkManager);
		}

		static void Main(string[] args)
		{
			IConfigurationBuilder builder = new ConfigurationBuilder()
				.AddJsonFile("config.json");

			config = builder.Build();

			commandManager = new CommandManager();
			displayManager = new DisplayManager();
			networkManager = new NetworkManager();
			scriptManager = new ScriptManager("script", config["script:python_lib"]);
			scriptManager.SetDisplayManager(displayManager);

			commandManager.CreateNetworkManager = (port) => CreateNetworkManager(port);
			commandManager.ConnectServer = (serv, port, user, pass) =>
			{
				if (!networkManager.Configured)
					CreateNetworkManager(9000);
				networkManager.Connect(serv, port, user, pass);
			};

			commandManager.ScriptReload = () => scriptManager.Reload();

			MessageParser.Initialize("messages.xml");

			while (true)
			{
				displayManager.Clear(0);
				displayManager.Write(0, 0, "AC Test Client");

				displayManager.Write(-15, 0, networkManager == null ? "Disconnected" : networkManager.World);

				displayManager.Write(0, -1, "> ");

				commandManager.CheckInput();

				System.Threading.Thread.Yield();
				//displayManager.Wait();
			}
		}
	}
}

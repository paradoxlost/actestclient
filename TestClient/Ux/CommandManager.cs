using System;
using System.IO;
using System.Text;
using System.Threading;

using Microsoft.Extensions.CommandLineUtils;

namespace TestClient.Ux
{
	public class CommandManager
	{
		public static CommandManager Instance { get; private set; }

		private const string HelpOption = "-? | -h | --help";

		private CommandLineApplication parser;
		//private Thread inputThread;
		//private bool running;

		public Action<int> CreateNetworkManager { get; set; }
		public Action<string, int, string, string> ConnectServer { get; set; }

		public Action ScriptReload { get; set; }
		public Action<string> ScriptExecute { get; set; }

		public CommandManager()
		{
			Instance = this;

			Init();
			//inputThread = new Thread(InputThreadStart);
			//inputThread.Start();
		}

		public void CheckInput()
		{
			try
			{
				string commands = Console.ReadLine();
				parser.Execute(commands.Split(' '));
			}
			catch (Exception ex)
			{
			}

			// if (Console.KeyAvailable)
			// {

			// }
		}

		public CommandLineApplication CreateCommand(string name, Func<int> action)
		{
			return parser.CreateCommand(name, action);
		}

		public CommandLineApplication CreateCommand(string name, Action<CommandLineApplication> setup)
		{
			return parser.CreateCommand(name, setup);
		}

		public CommandLineApplication CreateCommand(string name, string desc)
		{
			return parser.CreateCommand(name, desc);
		}

		public CommandLineApplication CreateCommand(string name, string desc, Action<CommandLineApplication> setup)
		{
			return parser.CreateCommand(name, desc, setup);
		}

		public CommandLineApplication CreateCommand(string name, string desc, Func<int> action)
		{
			return parser.CreateCommand(name, desc, action);
		}

		public CommandLineApplication CreateCommand(string name, string desc, Action<CommandLineApplication> setup, Func<int> action)
		{
			return parser.CreateCommand(name, desc, setup, action);
		}

		// private void InputThreadStart()
		// {
		// 	running = true;
		// 	while (running)
		// 	{
		// 		string commands = Console.ReadLine();
		// 		try
		// 		{
		// 			parser.Execute(commands.Split(' '));
		// 		}
		// 		catch
		// 		{

		// 		}

		// 		Thread.Yield();
		// 	}
		// }

		private void Init()
		{
			parser = new CommandLineApplication();

			CreateCommand("help", () =>
				{
					parser.ShowHelp();
					return 0;
				});
			// parser.Command("help", config =>
			// 	{
			// 		config.OnExecute(() =>
			// 		{
			// 			parser.ShowHelp();
			// 			return 0;
			// 		});
			// 	});

			CommandLineApplication cmd;

			// network
			cmd = CreateCommand("network", "Network configuration");
			cmd.CreateCommand("init", "Initialize network", config =>
				{
					//CommandOption host = config.Option("-i | --ip", "Local address (defaults to all)", CommandOptionType.SingleValue);
					CommandOption port = config.Option("-p | --port", "Local port (defaults to 9000)", CommandOptionType.SingleValue);

					config.OnExecute(() =>
					{
						int listen = 9000;
						if (port.HasValue())
							listen = int.Parse(port.Value());

						if (CreateNetworkManager != null)
							CreateNetworkManager(listen);

						return 0;
					});

				});

			cmd.CreateCommand("connect", "Connect to a server", config =>
				{
					CommandOption optA = config.Option("-a | --account", "GDLE Account:Password style login", CommandOptionType.NoValue);
					CommandOption optT = config.Option("-t | --token", "ACE Account Token/Password style login", CommandOptionType.NoValue);

					CommandArgument server = config.Argument("server", "server host/ip", false);
					CommandArgument port = config.Argument("port", "server port", false);
					CommandArgument username = config.Argument("user", "username", false);
					CommandArgument password = config.Argument("password", "password", false);

					config.OnExecute(() =>
					{
						if (optA.HasValue() && optT.HasValue())
						{
							config.Error.WriteLine("Must not include both login type options");
							config.ShowHelp();
							return 1;
						}

						string user = null;
						string pass = null;

						if (optA.HasValue())
						{
							user = username.Value + ":" + password.Value;
						}
						else if (optT.HasValue())
						{
							user = username.Value;
							pass = password.Value;
						}

						if (ConnectServer != null)
							ConnectServer(server.Value, int.Parse(port.Value), user, pass);

						return 0;
					});
				});

			// script
			cmd = CreateCommand("script", "Script management");

			// TODO: switch from blind loading of scripts to explicitly requested load
			// cmd.CreateCommand("load", "Load client script", () =>
			// 	{
			// 		return 0;
			// 	});

			cmd.CreateCommand("reload", () =>
				{
					Init();
					if (ScriptReload != null)
						ScriptReload();
					return 0;
				});

			// cmd.Command("exec", config =>
			// 	{
			// 		config.ShowInHelpText = true;
			// 		config.Description = "Reload scripts";
			// 		config.HelpOption(HelpOption);

			// 		config.OnExecute(() =>
			// 		{
			// 			if (ScriptReload != null)
			// 				ScriptReload();
			// 			return 0;
			// 		});
			// 	});

			CreateCommand("quit", "Exit the application", () =>
				{
					Environment.Exit(0);
					return 0;
				});

			parser.HelpOption(HelpOption);
		}
	}
}

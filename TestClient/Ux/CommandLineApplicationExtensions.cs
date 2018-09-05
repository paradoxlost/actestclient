using System;

using Microsoft.Extensions.CommandLineUtils;

namespace TestClient.Ux
{
	public static class CommandLineApplicationExtensions
	{
		private const string HelpOption = "-? | -h | --help";

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, Func<int> action)
		{
			return CreateCommand(app, name, null, null, action);
		}

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, Action<CommandLineApplication> setup)
		{
			return CreateCommand(app, name, null, setup);
		}

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, string desc)
		{
			return CreateCommand(app, name, desc, null, null);
		}

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, string desc, Action<CommandLineApplication> setup)
		{
			return CreateCommand(app, name, desc, setup, null);
		}

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, string desc, Func<int> action)
		{
			return CreateCommand(app, name, desc, null, action);
		}

		public static CommandLineApplication CreateCommand(this CommandLineApplication app, string name, string desc, Action<CommandLineApplication> setup, Func<int> action)
		{
			CommandLineApplication cmd = app.Command(name, config =>
				{
					if (!string.IsNullOrEmpty(desc))
					{
						config.ShowInHelpText = true;
						config.Description = desc;
						config.HelpOption(HelpOption);
					}

					if (setup != null)
						setup(config);

					if (action != null)
						config.OnExecute(action);

					if (setup == null && action == null)
					{
						config.OnExecute(() =>
							{
								config.ShowHelp();
								return -1;
							});
					}

				});
			return cmd;
		}

	}
}

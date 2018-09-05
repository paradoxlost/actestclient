
from System import Func
from TestClient.Ux import CommandLineApplicationExtensions as clae

from . import _command_manager
from . import world

__all__ = [ "CreateCommand", "CreateSubCommand" ]

def CreateCommand(name, desc = None, config = None, action = None):
	return _command_manager.CreateCommand(name, desc, config, action)

def CreateSubCommand(cmd, name, desc = None, config = None, action = None):
	return clae.CreateCommand(cmd, name, desc, config, action)

def _loginCharSetup(config):
	arg = config.Argument("index", "zero-based character index (from top)", False)

	def loginCharAction():
		world.LoginCharacter(int(arg.Value))
		return 0

	# it has trouble resolving this
	config.OnExecute(Func[int](loginCharAction))

cmd = CreateCommand("character", "character actions")
CreateSubCommand(cmd, "login", "log into the selected character", _loginCharSetup)

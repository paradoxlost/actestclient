
from System import Func
from TestClient.Ux import CommandLineApplicationExtensions as clae

from . import _command_manager
from . import world
from . import character

__all__ = [ "CreateCommand", "CreateSubCommand" ]

def CreateCommand(name, desc = None, config = None, action = None):
	return _command_manager.CreateCommand(name, desc, config, action)

def CreateSubCommand(cmd, name, desc = None, config = None, action = None):
	return clae.CreateCommand(cmd, name, desc, config, action)

def _simpleCommand(func):
	def simpleSetup(config):
		def simpleAction():
			func()
			return 0

		config.OnExecute(Func[int](simpleAction))

	return simpleSetup

def _loginCharSetup(config):
	arg = config.Argument("index", "zero-based character index (from top)", False)

	def loginCharAction():
		character.Login(int(arg.Value))
		return 0

	# it has trouble resolving this
	config.OnExecute(Func[int](loginCharAction))

cmd = CreateCommand("character", "character actions")
CreateSubCommand(cmd, "login", "log into the selected character", _loginCharSetup)
CreateSubCommand(cmd, "logout", "return to character selection screen", _simpleCommand(character.Logout))

def _getItemInfoSetup(config):
	arg = config.Argument("name", "item name", False)

	def getItemInfoAction():
		items = world.findInvItems(arg.Value)
		if items:
			for item in items:
				print(item.Name, item.IsStack, item.StackSize)

		else:
			print("not found")

		return 0

	config.OnExecute(Func[int](getItemInfoAction))

cmd = CreateCommand("inventory", "inventory actions")
CreateSubCommand(cmd, "find", "find items by name", _getItemInfoSetup)

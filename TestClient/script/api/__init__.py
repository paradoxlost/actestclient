
import TestClient

_command_manager = TestClient.Ux.CommandManager.Instance
_script_manager = TestClient.Scripts.ScriptManager.Instance
_network_manager = TestClient.Net.NetworkManager.Instance

from .messages import *
from .commands import *
from .actions import *

from .world import *

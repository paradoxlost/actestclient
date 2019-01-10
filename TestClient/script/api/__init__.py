
import TestClient

_command_manager = TestClient.Ux.CommandManager.Instance
_script_manager = TestClient.Scripts.ScriptManager.Instance
_network_manager = TestClient.Net.NetworkManager.Instance
_display_manager = TestClient.Ux.DisplayManager.Instance

from .messages import *
from .commands import *
from .actions import *

from .display import *
from .world import *
from .character import *


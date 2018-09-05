
from .messages import Send, SendAction

from . import _network_manager

__all__ = [ "SendText", "CastSpell" ]

def SendText(text):
	SendAction(0x15, (text))

def CastSpell(spell, target = None):
	if target is not None:
		SendAction(0x4A, (target, spell))
	else:
		SendAction(0x48, (spell))

#movement -> f7b1
#	f753, f61c

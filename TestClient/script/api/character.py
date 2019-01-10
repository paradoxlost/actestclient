from __future__ import print_function

import TestClient.Net.Packets as Packets

from . import _script_manager, _network_manager

#from .commands import *
from .messages import *
import display

__all__ = [
	"Character", "Characters", "Current", "Login", "Logout"
	]

Characters = None
Current = None

Location = None

class Character(object):
	Id = 0
	Index = -1
	Name = None

	def __init__(self, id, name, idx):
		self.Id = id
		self.Name = name
		self.Index = idx

def Logout():
	Send(0xf653, 5)

def Login(idx):
	global Characters, Current
	#print("LoginCharacter", idx, len(Characters))
	if idx >= 0 and idx < len(Characters):
		Current = Characters[idx]
		#print("Attempting to login", Current.Name)
		Send(0xf7c8, 4)

# to login
# > f7c8
# < f7df
# > f657

def _characterList(msg):
	global AccountName, Characters

	display.setTitle("Select Character")

	count = msg.characterCount
	if count > 0:
		Characters = []
		cl = msg.child("characters")
		for i in xrange(0, count):
			cs = cl[i]
			char = Character(cs.character, cs.name, i)
			Characters.append(char)
			display.write(3, 2 + i, "{}.  {}".format(i, cs.name))

	AccountName = msg.zonename

def _start3d(msg):
	global Current, AccountName
	#print("Start 3D Mode", Current.Name, AccountName)
	Send(0xf657, 4, (Current.Id, AccountName))
	SendAction(0x00a1)

def _characterInWorld(msg):
	display.setTitle(Current.Name)

HandleMessage(0xf658, _characterList)
HandleMessage(0xf746, _characterInWorld)
HandleMessage(0xf7df, _start3d)

def _loginCharacter(msg):
	global Current
	pass

#	cnt = msg.inventoryCount
#	inv = msg.child("inventory")

#	for i in xrange(0, cnt):
#		item = inv[i]
#		_inventoryItems.append(item.item)



#HandleMessageEvent(0xf7b0, 0x0013, _loginCharacter)

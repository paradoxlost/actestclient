from __future__ import print_function

import TestClient.Net.Packets as Packets

from . import _script_manager, _network_manager

#from .commands import *
from .messages import *

__all__ = [
	"AccountName", "ServerName", "Character", "Characters", "CurrentCharacter", "LoginCharacter"
	]

AccountName = None
ServerName = None
Characters = None
CurrentCharacter = None

# TODO: own module?
class Character(object):
	Id = 0
	Index = -1
	Name = None

	def __init__(self, id, name, idx):
		self.Id = id
		self.Name = name
		self.Index = idx

def LoginCharacter(idx):
	global Characters, CurrentCharacter
	print("LoginCharacter", idx, len(Characters))
	if idx >= 0 and idx < len(Characters):
		CurrentCharacter = Characters[idx]
		print("Attempting to login", CurrentCharacter.Name)
		Send(0xf7c8, 5)

# to login
# > f7c8
# < f7df
# > f657

def _serverName(msg):
	global ServerName
	ServerName = msg.server
	_network_manager.World = ServerName
	#Display.WriteClear(-15, 0, 15, Network.World)
	#Display.MoveToInput()
	#Display.Refresh()

def _databaseInfoRequest(msg):
	# in the real world, this message would be read for the requested dat files
	# we will just send a dummy response
	_network_manager.SendMessage(Packets.DatabaseSyncFragment())

def _characterList(msg):
	global AccountName, Characters
	count = msg.characterCount
	if count > 0:
		Characters = []
		cl = msg.child("characters")
		for i in xrange(0, count):
			cs = cl[i]
			char = Character(cs.character, cs.name, i)
			Characters.append(char)

	AccountName = msg.zonename

def _start3d(msg):
	global CurrentCharacter, AccountName
	print("Start 3D Mode", CurrentCharacter.Name, AccountName)
	Send(0xf657, 5, (CurrentCharacter.Id, AccountName))

HandleMessage(0xf658, _characterList)

HandleMessage(0xf7df, _start3d)

HandleMessage(0xf7e1, _serverName)
HandleMessage(0xf7e5, _databaseInfoRequest)

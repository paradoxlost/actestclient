from __future__ import print_function

import TestClient.Net.Packets as Packets

from . import _script_manager, _network_manager

from types import Vector, Quaternion, Position
#from .commands import *
from .messages import *
import display

import character

__all__ = [
	"AccountName", "ServerName", "findInvItems"
	]

AccountName = None
ServerName = None

class WorldItem(object):
	Id = 0
	Flags = 0
	Name = None
	Type = 0
	Category = 0
	Behavior = 0

	IsContainer = False
	Items = {}

	IsStack = False
	StackSize = None

	Location = None

	def __init__(self, msg):
		self.Id = msg.object
		game = msg.game #msg.child("game")
		self.Flags = game.flags1
		self.Name = game.name
		self.Type = game.type
		self.Category = game.category
		self.Behavior = game.behavior

		self.IsContainer = (self.Category & 0x0200) != 0
		if (self.Flags & 0x1000) != 0:
			self.IsStack = True
			self.StackSize = game.stack

		self.Location = Position()


#/WorldItem

#
# object management
#

_items = { }
_inventoryItems = []

_characterContainer = 0

_location = Position()

def findInvItems(name):
	global _items, _inventoryItems
	for itemId in _inventoryItems:
		item = _items[itemId]
		if item.Name == name:
			yield item


def _createObject(msg):
	global _items
	item = WorldItem(msg)
	_items[item.Id] = item

def _removeObject(msg):
	global _items
	_items.pop(msg.object, None)

def _loginCharacter(msg):
	global _characterContainer, _inventoryItems, _location
	#print("world::_loginCharacter")
	_characterContainer = msg.character

	props = msg.properties
	pf = props.flags
	if pf & 0x0020:
		pc = props.positionCount
		ps = props.positions
		if pc > 0:
			_location.unpack(props.positions[0].value)
			display.setPosition(_location)

		#for pi in xrange(0, pc):
		#	p = ps[pi]
		#	print("{} : {:x}".format(p.key, p.value.landcell))

	cnt = msg.inventoryCount
	inv = msg.inventory

	for i in xrange(0, cnt):
		item = inv[i]
		_inventoryItems.append(item.item)

	#print("world::_loginCharacter // exit")


HandleMessage(0xf745, _createObject)
HandleMessage(0xf747, _removeObject)
HandleMessageEvent(0xf7b0, 0x0013, _loginCharacter)
#
# initial connection
#

def _serverName(msg):
	global ServerName
	ServerName = msg.server
	_network_manager.World = ServerName
	display.setWorld(ServerName)

def _databaseInfoRequest(msg):
	# in the real world, this message would be read for the requested dat files
	# we will just send a dummy response
	_network_manager.SendMessage(Packets.DatabaseSyncFragment())

HandleMessage(0xf7e1, _serverName)
HandleMessage(0xf7e5, _databaseInfoRequest)

#
# positioning
#
def _updatePosition(msg):
	global _location
	obj = msg.object
	pos = msg.position

	if obj == character.Current.Id:
		_location.unpack(pos)
		display.setPosition(_location)
	else:
		try:
			item = _items[obj]
			item.Location.unpack(pos)
		except:
			pass

HandleMessage(0xf748, _updatePosition)

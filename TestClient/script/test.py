from __future__ import print_function
#from builtins import range

import api
from api.commands import *
from api.messages import *

def createObject(msg):
	#print("Create Object", msg.Value[int]("object"))
	phys = msg.child("physics")
	flags = phys.flags
	if flags & 0x00030000:
		print(msg.child("game").name)

def characterInWorld(msg):
	print("In World?")

def characterInfo(msg):
	print("Character Info")

HandleMessage(0xf745, createObject)
HandleMessage(0xf746, characterInWorld)

HandleMessageEvent(0xf7b0, 0x0013, characterInfo)


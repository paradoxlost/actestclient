from __future__ import print_function

__all__ = [
	"Vector", "Quaternion", "Position"
	]

class Vector(object):
	def __init__(self):
		self.X = 0
		self.Y = 0
		self.Z = 0

	def unpack(self, msg):
		self.X = msg.x
		self.Y = msg.y
		self.Z = msg.z

class Quaternion(object):
	def __init__(self):
		self.W = 0
		self.X = 0
		self.Y = 0
		self.Z = 0

	def unpack(self, msg):
		# todo: later
		#flags =
		self.W = msg.wQuat
		self.X = msg.xQuat
		self.Y = msg.yQuat
		self.Z = msg.zQuat

class Position(object):
	def __init__(self):
		self.Cell = 0
		self.Offset = Vector()
		self.Rotation = Quaternion()
		self.Velocity = Vector()

	def unpack(self, msg):
		self.Cell = msg.landcell
		#self.Offset.unpack(msg.displacement)

	def toCoords(self):
		#float lat = (((int)(blockId & 0xff) - 0x7f) * 192 - 84) / 240.0f;
		#float lng = (((int)(blockId >> 8) - 0x7f) * 192 - 84) / 240.0f;
		block = (self.Cell >> 16)
		lat = (((block & 0xff) - 0x7f) * 192 - 84) / 240.0
		lng = (((block >> 8) - 0x7f) * 192 - 84) / 240.0

		return "{0:2.1f}{2}, {1:2.1f}{3}".format(abs(lat), abs(lng), "S" if lat < 0 else "N", "W" if lng < 0 else "E" )

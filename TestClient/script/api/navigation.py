
__all__ = [ "Coords", "LocalCoords", "NavNode" ]

class Coords(object):
	X = 0.0
	Y = 0.0
	Z = 0.0

	def __init__(self, x = 0, y = 0, z = 0):
		self.X = x
		self.Y = y
		self.Z = z

	def Read(self, file):
		self.X = float(file.readline())
		self.Y = float(file.readline())
		self.Z = float(file.readline())

class LocalCoords(Coords):
	Local = False

	def Read(self, file):
		self.Local = bool(file.readline())
		super(LocalCoords, self).Read(file)

class NavNode(object):
	Type = 0
	Location = None

	def __init__(self, nodeType):
		self.Type = nodeType
		self.Location = Coords()

	def _read(self, file):
		self.Location.Read(file)
		file.readline() # 0

class InstancePortalNavNode(NavNode):
	PortalId = 0

	def _read(self, file):
		super(InstancePortalNavNode, self)._read(file)
		self.PortalId = int(file.readline())

class RecallNavNode(NavNode):
	SpellId = 0

	def _read(self, file):
		super(RecallNavNode, self)._read(file)
		self.SpellId = int(file.readline())

class DelayNavNode(NavNode):
	Delay = 0

	def _read(self, file):
		super(DelayNavNode, self)._read(file)
		self.Delay = int(file.readline())

class ChatCommandNavNode(NavNode):
	Command = None

	def _read(self, file):
		super(ChatCommandNavNode, self)._read(file)
		self.Command = file.readline()

class VendorNavNode(NavNode):
	VendorId = 0
	VendorName = None

	def _read(self, file):
		super(VendorNavNode, self)._read(file)
		self.VendorId = int(file.readline())
		self.VendorName = file.readline()

class NamedObjectNavNode(NavNode):
	Name = None
	ObjectClass = 0
	LocalCoords = None

	def _read(self, file):
		super(NamedObjectNavNode, self)._read(file)
		self.Name = file.readline()
		self.ObjectClass = int(file.readline())
		self.LocalCoords = LocalCoords()
		self.LocalCoords.Read(file)

class NamedPortalNavNode(NamedObjectNavNode):
	pass

class NpcNavNode(NamedObjectNavNode):
	pass

class CheckPointNavNode(NavNode):
	pass

class JumpNavNode(DelayNavNode):
	Angle = 0
	Walk = False
	Delay = 0

	def _read(self, file):
		super(JumpNavNode, self)._read(file)
		self.Angle = int(file.readline())
		self.Walk = bool(file.readline())
		self.Delay = int(file.readline())

class NavPlan(object):
	PlanType = 0
	NodeCount = 0
	Nodes = []

	def __init__(self, path):
		planFile = open(path, "r")
		self._readHeader(planFile)
		self._readPlan(planFile)
		planFile.close()

	def _readHeader(self, file):
		file.readline() # should check, don't care right now
		self.PlanType = int(file.readline())

	def _readPlan(self, file):
		if self.PlanType == 3:
			self.TargetId = int(file.readline())
			return

		# read nodes
		self.NodeCount = int(file.readline())
		cnt = 0
		while cnt < self.NodeCount:
			self._readNode(file)

	def _readNode(self, file):
		nt = int(file.readline())
		node = None

		if nt == 0: # standard point node
			node = NavNode(nt)
		elif nt == 1: # portal
			node = InstancePortalNavNode(nt)
		elif nt == 2: # recall
			node = RecallNavNode(nt)
		elif nt == 3: # wait
			node = DelayNavNode(nt)
		elif nt == 4: # chat command
			node = ChatCommandNavNode(nt)
		elif nt == 5: # vendor
			node = VendorNavNode(nt)
		elif nt == 6: # portal 2
			node = NamedPortalNavNode(nt)
		elif nt == 7: # npc
			node = NpcNavNode(nt)
		elif nt == 8: # checkpoint
			node = CheckPointNavNode(nt)
		elif nt == 9: # jump
			node = JumpNavNode(nt)

		# error case?
		self.Nodes.append(node)

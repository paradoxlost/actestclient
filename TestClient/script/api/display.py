from __future__ import print_function

from . import _script_manager, _network_manager, _display_manager

from .character import Current

__all__ = [
	"setWorld", "setPosition", "setTitle", "write",
	"enablePane",
	"Pane", "ChatPane"
	]

Pos_World = (-15, 0)
Pos_Title = (0, 0)

_current_title = "AC Test Client"
_current_pos = ""

_panes = []

def setWorld(name):
	write(Pos_World[0], Pos_World[1], name, abs(Pos_World[0]))

def setPosition(pos):
	global _current_pos
	_current_pos = pos.toCoords()
	write(Pos_World[0], Pos_World[1] + 1, _current_pos, abs(Pos_World[0]))

def setTitle(text):
	global _current_title
	_current_title = text
	write(Pos_Title[0], Pos_Title[1], text, 20)

def write(cols, rows, text, clear = 0):
	pt = _display_manager.GetCursor()
	if clear > 0:
		_display_manager.WriteClear(cols, rows, clear, text)
	else:
		_display_manager.Write(cols, rows, text)
	_display_manager.Move(pt.X, pt.Y)

def _writeNoMove(cols, rows, text):
	_display_manager.Write(cols, rows, text)

def _refreshed(sender, e):
	_writeNoMove(Pos_Title[0], Pos_Title[1], _current_title)
	_writeNoMove(Pos_World[0], Pos_World[1], _network_manager.World)
	_writeNoMove(Pos_World[0], Pos_World[1] + 1, _current_pos)

	for p in _panes:
		try:
			p.draw()
		except:
			pass

	_writeNoMove(0, -1,  "> ")

_display_manager.Refreshed += _refreshed

def enablePane(pane):
	global _panes
	_panes.append(pane)

class Pane(object):

	def __init__(self, left = 0, top = 0, right = -1, bottom = -1):
		self.Left = left
		self.Top = top
		self.Right = right
		self.Bottom = bottom

	def getHeight(self):
		realTop = self.Top
		realBottom = self.Bottom

		if realTop < 0:
			realTop = _display_manager.Height + realTop

		if realBottom < 0:
			realBottom = _display_manager.Height + realBottom

		return realTop - realBottom

	def draw(self):
		pass

class ChatPane(Pane):

	def __init__(self, left = 0, top = 1, right = -1, bottom = -2):
		super(ChatPane, self).__init__(left, top, right, bottom)
		self.lines = []

	def draw(self):
		# clear the area
		height = self.getHeight()
		for i in xrange(self.Top, height):
			_display_manager.Clear(i)

		# draw the headings
		_writeNoMove(self.Left, self.Top, "Chat")

		# draw the chat buffer
		bufflen = max(len(self.lines), height)
		for i in xrange(-1, -bufflen, -1):
			_writeNoMove(self.Left, i - 1, self.lines[i])

	def addChat(self, text):
		self.lines.append(text)
		self.draw()

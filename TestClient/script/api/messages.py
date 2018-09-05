from TestClient.Net.Packets import Fragment, EventFragment, PacketWriter

from . import _script_manager, _network_manager

import struct

__all__ = [
	"HandleMessage", "HandleMessageEvent",
	"Message", "EventMessage",
	"Send", "SendAction"
	]

def _wrap_handler(func):
	def _internal_wrap(msg):
		func(MessageWrapper(msg))

	return _internal_wrap

def HandleMessage(msg, func):
	_script_manager.HandleMessage(msg, _wrap_handler(func))

def HandleMessageEvent(msg, evt, func):
	_script_manager.HandleMessageEvent(msg, evt, _wrap_handler(func))

def Send(evt, group, data = None):
	_network_manager.SendMessage(Message(evt, group, data))

def SendAction(action, data = None):
	_network_manager.SendEventMessage(EventMessage(action, data))

class MessageWrapper(object):
	_parser = None

	def __init__(self, parser):
		self._parser = parser

	def __getattr__(self, attr):
		return self._parser[attr]

	def __len__(self):
		return self._parser.Length

	def __getitem__(self, idx):
		return MessageWrapper(self._parser.Struct(idx))

	def child(self, name):
		return MessageWrapper(self._parser.Struct(name))

class Message(Fragment):
	Data = None

	def __new__(cls, msg, group, data = None):
		return Fragment.__new__(cls, msg, group)

	def __init__(self, msg, group, data = None):
		self.Data = data

	def OnSerialize(self, writer):
		# python only has int, long, double numerics
		# so we will have to get creative to handle
		# float and short values
		if self.Data is not None:
			for var in self.Data:
				if type(var) is str:
					PacketWriter.WriteString16(writer, var)
					continue

				if type(var) is int:
					writer.Write(var)
					continue

class EventMessage(EventFragment):
	Data = None

	def __new__(cls, action, data = None):
		return EventFragment.__new__(cls, action)

	def __init__(self, action, data = None):
		self.Data = data

	def OnSerialize(self, writer):
		EventFragment.OnSerialize(self, writer)
		# python only has int, long, double numerics
		# so we will have to get creative to handle
		# float and short values
		if self.Data is not None:
			for var in self.Data:
				if type(var) is str:
					PacketWriter.WriteString16(writer, var)
					continue

				if type(var) is int:
					writer.Write(var)
					continue

class PackedEventMessage(EventFragment):
	Format = None
	Data = None
	_b = None

	def __new__(cls, action, fmt, *args):
		return EventFragment.__new__(cls, action)

	def __init__(self, action, fmt, *args):
		self.Format = fmt
		self.Data = args
		self._b = struct.pack(fmt, *args)

	def OnSerialize(self, writer):
		EventFragment.OnSerialize(self, writer)
		writer.Write(bytes(self._b))
		# instead of using the writer here, we'll serialize using `struct`
		# and pass on the blob from that

#https://docs.python.org/2/library/struct.html

using System.Net.Sockets;

namespace MsgPack.Rpc.Core.Protocols {
	internal static class SocketErrorCodeExtension {
		public static bool? IsError(this SocketError source) {
			switch (source) {
				case SocketError.AlreadyInProgress:
				case SocketError.Disconnecting:
				case SocketError.IsConnected:
				case SocketError.Shutdown: {
					return null;
				}
				case SocketError.InProgress:
				case SocketError.Interrupted:
				case SocketError.IOPending:
				case SocketError.OperationAborted:
				case SocketError.Success:
				case SocketError.WouldBlock: {
					return false;
				}
				default: {
					return true;
				}
			}
		}

		public static RpcError ToRpcError(this SocketError source) {
			if (!source.IsError().GetValueOrDefault()) {
				return null;
			}

			switch (source) {
				case SocketError.ConnectionRefused: {
					// Caller bug
					return RpcError.ConnectionRefusedError;
				}
				case SocketError.HostNotFound:
				case SocketError.HostUnreachable:
				case SocketError.NetworkUnreachable: {
					return RpcError.NetworkUnreacheableError;
				}
				case SocketError.MessageSize: {
					return RpcError.MessageTooLargeError;
				}
				case SocketError.TimedOut: {
					return RpcError.ConnectionTimeoutError;
				}
				default: {
					// Caller bug
					return RpcError.TransportError;
				}
			}
		}
	}
}

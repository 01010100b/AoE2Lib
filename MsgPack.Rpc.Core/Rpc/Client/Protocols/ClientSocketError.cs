using MsgPack.Rpc.Core.Protocols;
using System.Net.Sockets;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Defines utility methods to handle <see cref="SocketError"/>.
	/// </summary>
	public static class ClientSocketError {
		/// <summary>
		///		Creates a <see cref="RpcErrorMessage"/> based on the specified <see cref="SocketError"/>.
		/// </summary>
		/// <param name="socketError">The underlying <see cref="SocketError"/>.</param>
		/// <returns>
		///		A <see cref="RpcErrorMessage"/> based on the specified <see cref="SocketError"/>.
		/// </returns>
		public static RpcErrorMessage ToClientRpcError(this SocketError socketError) {
			if (socketError.IsError().GetValueOrDefault()) {
				return new RpcErrorMessage(socketError.ToRpcError(), new SocketException((int)socketError).Message, string.Empty);
			}
			else {
				return RpcErrorMessage.Success;
			}
		}
	}
}

using System.Diagnostics.Contracts;
using System.Net.Sockets;

namespace MsgPack.Rpc.Core.Protocols {
	internal static class SocketAsyncEventArgsExtensions {
		public static MessageContext GetContext(this SocketAsyncEventArgs source) {
			Contract.Requires(source != null);
			return source.UserToken as MessageContext;
		}
	}
}

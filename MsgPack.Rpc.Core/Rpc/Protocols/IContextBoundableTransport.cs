using System.Diagnostics.Contracts;
using System.Net.Sockets;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Defines common interface for the transports which is bindable to the message context and async socket.
	/// </summary>
	[ContractClass(typeof(IContextBoundableTransportContract))]
	internal interface IContextBoundableTransport {
		/// <summary>
		///		Gets the bound socket.
		/// </summary>
		Socket BoundSocket { get; }

		/// <summary>
		///		Called when the async socket operation is completed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
		void OnSocketOperationCompleted(object sender, SocketAsyncEventArgs e);
	}

	[ContractClassFor(typeof(IContextBoundableTransport))]
	internal abstract class IContextBoundableTransportContract : IContextBoundableTransport {
		public Socket BoundSocket => null;

		public void OnSocketOperationCompleted(object sender, SocketAsyncEventArgs e) {
			Contract.Requires(sender != null);
			Contract.Requires(e != null);
		}
	}

}

using System;
using System.Net.Sockets;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Implements <see cref="ClientTransport"/> for TCP/IP protocol.
	/// </summary>
	public sealed class TcpClientTransport : ClientTransport {
		/// <summary>
		/// Gets a value indicating whether the protocol used by this class can resume receiving.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can resume receiving; otherwise, <c>false</c>.
		/// </value>
		protected override bool CanResumeReceiving => true;

		/// <summary>
		///		Initializes a new instance of the <see cref="TcpClientTransport"/> class.
		/// </summary>
		/// <param name="manager">The manager which will manage this instance.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="manager"/> is <c>null</c>.
		/// </exception>
		public TcpClientTransport(TcpClientTransportManager manager)
			: base(manager) { }

		/// <summary>
		///		Shutdowns the sending.
		/// </summary>
		protected sealed override void ShutdownSending() {
			try {
				BoundSocket.Shutdown(SocketShutdown.Send);
			}
			catch (SocketException ex) {
				if (ex.SocketErrorCode != SocketError.NotConnected) {
					throw;
				}
			}

			base.ShutdownSending();
		}

		/// <summary>
		///		Shutdowns the receiving.
		/// </summary>
		protected sealed override void ShutdownReceiving() {
			try {
				BoundSocket.Shutdown(SocketShutdown.Receive);
			}
			catch (SocketException ex) {
				if (ex.SocketErrorCode != SocketError.NotConnected) {
					throw;
				}
			}

			base.ShutdownReceiving();
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Send' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected sealed override void SendCore(ClientRequestContext context) {
			if (!BoundSocket.SendAsync(context.SocketContext)) {
				context.SetCompletedSynchronously();
				OnSent(context);
			}
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Receive' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected sealed override void ReceiveCore(ClientResponseContext context) {
			if (!BoundSocket.ReceiveAsync(context.SocketContext)) {
				context.SetCompletedSynchronously();
				OnReceived(context);
			}
		}
	}
}

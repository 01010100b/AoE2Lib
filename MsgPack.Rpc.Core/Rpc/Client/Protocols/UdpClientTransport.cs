using System;
using System.Net;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Implements <see cref="ClientTransport"/> for UDP/IP protocol.
	/// </summary>
	public sealed class UdpClientTransport : ClientTransport {
		/// <summary>
		///		Gets the remote end point.
		/// </summary>
		/// <value>
		///		The remote end point.
		/// </value>
		public EndPoint RemoteEndPoint { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the protocol used by this class can resume receiving.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can resume receiving; otherwise, <c>false</c>.
		/// </value>
		protected override bool CanResumeReceiving => false;

		/// <summary>
		///		Initializes a new instance of the <see cref="UdpClientTransport"/> class.
		/// </summary>
		/// <param name="manager">The manager which will manage this instance.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="manager"/> is <c>null</c>.
		/// </exception>
		public UdpClientTransport(UdpClientTransportManager manager) : base(manager) { }

		/// <summary>
		///		Gets the <see cref="ClientRequestContext"/> to store context information for request or notification.
		/// </summary>
		/// <returns>
		///		The <see cref="ClientRequestContext"/> to store context information for request or notification.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		This object is not ready to invoke this method.
		/// </exception>
		public sealed override ClientRequestContext GetClientRequestContext() {
			if (RemoteEndPoint == null) {
				throw new InvalidOperationException("RemoteEndPoint must be set. UdpClientTransport must be retrieved from UdpTClientransportManager.GetTransport.");
			}

			var result = base.GetClientRequestContext();
			result.RemoteEndPoint = RemoteEndPoint;
			return result;
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Send' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected sealed override void SendCore(ClientRequestContext context) {
			if (!BoundSocket.SendToAsync(context.SocketContext)) {
				context.SetCompletedSynchronously();
				OnSent(context);
			}
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Receive' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected sealed override void ReceiveCore(ClientResponseContext context) {
			if (!BoundSocket.ReceiveFromAsync(context.SocketContext)) {
				context.SetCompletedSynchronously();
				OnReceived(context);
			}
		}
	}
}

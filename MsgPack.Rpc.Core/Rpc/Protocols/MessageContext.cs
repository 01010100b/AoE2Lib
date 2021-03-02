using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Represents context information of asynchronous MesagePack-RPC operation.
	/// </summary>
	public abstract class MessageContext : IDisposable {
		#region -- Async Socket Context --

		/// <summary>
		///		Gets the socket context for asynchronous socket.
		/// </summary>
		/// <value>
		///		The <see cref="SocketAsyncEventArgs"/> for asynchronous socket.
		/// </value>
		public SocketAsyncEventArgs SocketContext { get; }

		#endregion

		#region -- Session Management --

		static long lastSessionId;

		long sessionId;

		/// <summary>
		///		Gets the ID of the session.
		/// </summary>
		/// <value>
		///		The ID of the session.
		/// </value>
		/// <remarks>
		///		DO NOT use this information for the security related feature.
		///		This information is intented for the tracking of session processing in debugging.
		/// </remarks>
		public long SessionId {
			get { return sessionId; }
			internal set { sessionId = value; }
		}

		/// <summary>
		///		Gets the session start time.
		/// </summary>
		/// <value>
		///		The session start time.
		/// </value>
		public DateTimeOffset SessionStartedAt { get; private set; }

		/// <summary>
		///		Gets or sets the message id.
		/// </summary>
		/// <value>
		///		The message id. 
		///		This value will be undefined for the notification message.
		/// </value>
		public int? MessageId {
			get;
			internal set;
		}

		#endregion

		#region -- CompletedSynchronously --


		/// <summary>
		///		Gets a value indicating whether the operation has been completed synchronously.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the operation has been completed synchronously; otherwise, <c>false</c>.
		/// </value>
		public bool CompletedSynchronously { get; private set; }

		/// <summary>
		///		Sets the operation has been completed synchronously.
		/// </summary>
		public void SetCompletedSynchronously() {
			Contract.Ensures(CompletedSynchronously == true);

			CompletedSynchronously = true;
		}

		#endregion

		#region -- Transport --

		IContextBoundableTransport boundTransport;

		/// <summary>
		///		Gets the bound <see cref="IContextBoundableTransport"/>.
		/// </summary>
		internal IContextBoundableTransport BoundTransport => boundTransport;

		/// <summary>
		///		Sets the bound <see cref="IContextBoundableTransport"/>.
		/// </summary>
		/// <param name="transport">The <see cref="IContextBoundableTransport"/>.</param>
		internal virtual void SetTransport(IContextBoundableTransport transport) {
			Contract.Requires(transport != null);
			Contract.Requires(BoundTransport == null);
			Contract.Ensures(BoundTransport != null);

			var oldBoundTransport = Interlocked.CompareExchange(ref boundTransport, transport, null);
			if (oldBoundTransport != null) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "This context is already bounded to '{0}'(Socket: 0x{1:X}).", transport.GetType(), transport.BoundSocket == null ? IntPtr.Zero : transport.BoundSocket.Handle));
			}

			SocketContext.Completed += transport.OnSocketOperationCompleted;
		}

		readonly TimeoutWatcher timeoutWatcher;
		int isTimeout;

		/// <summary>
		///		Gets a value indicating whether the watched operation is timed out.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the watched operation is timed out; otherwise, <c>false</c>.
		/// </value>
		internal bool IsTimeout => Interlocked.CompareExchange(ref isTimeout, 0, 0) != 0;

		/// <summary>
		///		Occurs when the watched operation is timed out.
		/// </summary>
		internal event EventHandler Timeout;

		void OnTimeout() {
			Interlocked.Exchange(ref isTimeout, 1);

			Timeout?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region -- Communication --

		int? bytesTransferred;

		/// <summary>
		///		Gets the bytes count of transferred data.
		/// </summary>
		/// <returns>The bytes count of transferred data..</returns>
		public int BytesTransferred => bytesTransferred ?? SocketContext.BytesTransferred;

		/// <summary>
		///		Gets or sets the remote end point.
		/// </summary>
		/// <value>
		///		The remote end point.
		/// </value>
		public EndPoint RemoteEndPoint {
			get { return SocketContext.RemoteEndPoint; }
			set { SocketContext.RemoteEndPoint = value; }
		}

		/// <summary>
		///		Gets the last asynchronous operation.
		/// </summary>
		/// <value>
		///		The <see cref="SocketAsyncOperation"/> which represents the last asynchronous operation.
		/// </value>
		public SocketAsyncOperation LastOperation => SocketContext.LastOperation;

		/// <summary>
		///		Gets or sets the asynchronous socket operation result.
		/// </summary>
		/// <value>
		///		The <see cref="SocketError"/> which represents the asynchronous socket operation result.
		/// </value>
		public SocketError SocketError {
			get { return SocketContext.SocketError; }
			set { SocketContext.SocketError = value; }
		}

		#endregion

		#region -- In-Proc support fakes --

		/// <summary>
		///		Sets <see cref="BytesTransferred"/> property value with specified value for testing purposes.
		/// </summary>
		/// <param name="value">The value.</param>
		internal void SetBytesTransferred(int value) {
			bytesTransferred = value;
		}

		internal byte[] Buffer => SocketContext.Buffer;

		internal IList<ArraySegment<byte>> BufferList => SocketContext.BufferList;

		internal int Offset => SocketContext.Offset;

		#endregion


		/// <summary>
		///		Initializes a new instance of the <see cref="MessageContext"/> class.
		/// </summary>
		protected MessageContext() {
			SocketContext = new SocketAsyncEventArgs {
				UserToken = this
			};
			timeoutWatcher = new TimeoutWatcher();
			timeoutWatcher.Timeout += (sender, e) => OnTimeout();
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing">
		///		<c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
		///	</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				SocketContext.Dispose();
				timeoutWatcher.Dispose();
			}
		}

		/// <summary>
		///		Starts timeout watch.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		internal virtual void StartWatchTimeout(TimeSpan timeout) {
			Interlocked.Exchange(ref isTimeout, 0);
			timeoutWatcher.Start(timeout);
		}

		/// <summary>
		///		Stops timeout watch.
		/// </summary>
		internal virtual void StopWatchTimeout() {
			timeoutWatcher.Stop();
			timeoutWatcher.Reset();
		}

		/// <summary>
		///		Renews the session id and start time.
		/// </summary>
		public void RenewSessionId() {
			Contract.Ensures(SessionId > 0);
			Contract.Ensures(SessionStartedAt >= DateTimeOffset.Now);

			sessionId = Interlocked.Increment(ref lastSessionId);
			SessionStartedAt = DateTimeOffset.Now;
		}

		/// <summary>
		///		Clears the session id.
		/// </summary>
		internal void ClearSessionId() {
			Interlocked.Exchange(ref sessionId, 0);
		}

		/// <summary>
		///		Clears this instance internal buffers for reuse.
		/// </summary>
		internal virtual void Clear() {
			Contract.Ensures(CompletedSynchronously == false);
			Contract.Ensures(MessageId == null);
			Contract.Ensures(SessionId == 0);
			Contract.Ensures(SessionStartedAt == default);

			CompletedSynchronously = false;
			MessageId = null;
			sessionId = 0;
			SessionStartedAt = default;
			bytesTransferred = null;
			timeoutWatcher.Reset();
			Interlocked.Exchange(ref isTimeout, 0);
		}

		internal void UnboundTransport() {
			Contract.Ensures(BoundTransport == null);

			var boundTransport = Interlocked.Exchange(ref this.boundTransport, null);
			if (boundTransport != null) {
				SocketContext.Completed -= boundTransport.OnSocketOperationCompleted;
			}
		}
	}
}
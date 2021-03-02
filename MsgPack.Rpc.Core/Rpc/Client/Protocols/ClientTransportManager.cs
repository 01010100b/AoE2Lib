using MsgPack.Rpc.Core.Protocols;
using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Defines non-generic interface of <see cref="ClientTransportManager{T}"/> and provides related features.
	/// </summary>
	public abstract class ClientTransportManager : IDisposable {
		readonly ObjectPool<ClientRequestContext> requestContextPool;

		/// <summary>
		///		Gets the <see cref="ObjectPool{T}"/> of <see cref="ClientRequestContext"/>.
		/// </summary>
		/// <value>
		///		The <see cref="ObjectPool{T}"/> of <see cref="ClientRequestContext"/>.
		///		This value will not be <c>null</c>.
		/// </value>
		protected ObjectPool<ClientRequestContext> RequestContextPool {
			get {
				Contract.Ensures(Contract.Result<ObjectPool<ClientRequestContext>>() != null);

				return requestContextPool;
			}
		}

		readonly ObjectPool<ClientResponseContext> responseContextPool;

		/// <summary>
		///		Gets the <see cref="ObjectPool{T}"/> of <see cref="ClientResponseContext"/>.
		/// </summary>
		/// <value>
		///		The <see cref="ObjectPool{T}"/> of <see cref="ClientResponseContext"/>.
		///		This value will not be <c>null</c>.
		/// </value>
		protected ObjectPool<ClientResponseContext> ResponseContextPool {
			get {
				Contract.Ensures(Contract.Result<ObjectPool<ClientResponseContext>>() != null);

				return responseContextPool;
			}
		}

		readonly RpcClientConfiguration configuration;

		/// <summary>
		///		Gets the <see cref="RpcClientConfiguration"/> which describes transport configuration.
		/// </summary>
		/// <value>
		///		The <see cref="RpcClientConfiguration"/> which describes transport configuration.
		///		This value will not be <c>null</c>.
		/// </value>
		protected internal RpcClientConfiguration Configuration {
			get {
				Contract.Ensures(Contract.Result<RpcClientConfiguration>() != null);

				return configuration;
			}
		}

		int isDisposed;

		/// <summary>
		///		Gets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposed => Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0;

		int isInShutdown;

		/// <summary>
		///		Gets a value indicating whether this instance is in shutdown.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is in shutdown; otherwise, <c>false</c>.
		/// </value>
		public bool IsInShutdown => Interlocked.CompareExchange(ref isInShutdown, 0, 0) != 0;

		EventHandler<ShutdownCompletedEventArgs> shutdownCompleted;

		/// <summary>
		///		Occurs when client shutdown is completed.
		/// </summary>
		public event EventHandler<ShutdownCompletedEventArgs> ShutdownCompleted {
			add {
				EventHandler<ShutdownCompletedEventArgs> oldHandler;
				var currentHandler = shutdownCompleted;
				do {
					oldHandler = currentHandler;
					var newHandler = Delegate.Combine(oldHandler, value) as EventHandler<ShutdownCompletedEventArgs>;
					currentHandler = Interlocked.CompareExchange(ref shutdownCompleted, newHandler, oldHandler);
				} while (oldHandler != currentHandler);
			}
			remove {
				EventHandler<ShutdownCompletedEventArgs> oldHandler;
				var currentHandler = shutdownCompleted;
				do {
					oldHandler = currentHandler;
					var newHandler = Delegate.Remove(oldHandler, value) as EventHandler<ShutdownCompletedEventArgs>;
					currentHandler = Interlocked.CompareExchange(ref shutdownCompleted, newHandler, oldHandler);
				} while (oldHandler != currentHandler);
			}
		}

		/// <summary>
		///		Raises <see cref="ShutdownCompleted"/> event.
		/// </summary>
		/// <param name="e">The <see cref="ShutdownCompletedEventArgs"/> instance containing the event data.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="e"/> is <c>null</c>.
		/// </exception>		
		protected virtual void OnShutdownCompleted(ShutdownCompletedEventArgs e) {
			if (e == null) {
				throw new ArgumentNullException(nameof(e));
			}

			Contract.EndContractBlock();

			Interlocked.CompareExchange(ref shutdownCompleted, null, null)?.Invoke(this, e);
			Interlocked.Exchange(ref isInShutdown, 0);
		}

		/// <summary>
		///		Occurs when unknown response received.
		/// </summary>
		/// <remarks>
		///		When the client restart between the server accepts request and sends response,
		///		the orphan message might be occurred.
		/// </remarks>
		public event EventHandler<UnknownResponseReceivedEventArgs> UnknownResponseReceived;

		/// <summary>
		///		Raises the <see cref="E:UnknownResponseReceived"/> event.
		/// </summary>
		/// <param name="e">The <see cref="UnknownResponseReceivedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnUnknownResponseReceived(UnknownResponseReceivedEventArgs e) {
			if (e == null) {
				throw new ArgumentNullException(nameof(e));
			}

			Contract.EndContractBlock();

			Interlocked.CompareExchange(ref UnknownResponseReceived, null, null)?.Invoke(this, e);
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientTransportManager"/> class.
		/// </summary>
		/// <param name="configuration">
		///		The <see cref="RpcClientConfiguration"/> which contains various configuration information.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="configuration"/> is <c>null</c>.
		/// </exception>
		protected ClientTransportManager(RpcClientConfiguration configuration) {
			Contract.EndContractBlock();

			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			requestContextPool = configuration.RequestContextPoolProvider(() => new ClientRequestContext(configuration), configuration.CreateRequestContextPoolConfiguration());
			responseContextPool = configuration.ResponseContextPoolProvider(() => new ClientResponseContext(configuration), configuration.CreateResponseContextPoolConfiguration());
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		When overridden in derived class, releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing">
		///		<c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
		///	</param>
		/// <remarks>
		///		This method is guaranteed that this is invoked exactly once and after <see cref="IsDisposed"/> changed <c>true</c>.
		/// </remarks>
		protected virtual void Dispose(bool disposing) {
			if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0) {
			}
		}

		/// <summary>
		///		Initiates client shutdown.
		/// </summary>
		/// <returns>
		///		If shutdown process is initiated, then <c>true</c>.
		///		If shutdown is already initiated or completed, then <c>false</c>.
		/// </returns>
		public bool BeginShutdown() {
			if (Interlocked.Exchange(ref isInShutdown, 1) == 0) {
				BeginShutdownCore();
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		///		When overridden in derived class, initiates protocol specific shutdown process.
		/// </summary>
		protected virtual void BeginShutdownCore() {
			// nop
		}

		/// <summary>
		///		Establishes logical connection, which specified to the managed transport protocol, for the server.
		/// </summary>
		/// <param name="targetEndPoint">The end point of target server.</param>
		/// <returns>
		///		<see cref="Task{T}"/> of <see cref="ClientTransport"/> which represents asynchronous establishment process
		///		specific to the managed transport.
		///		This value will not be <c>null</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public Task<ClientTransport> ConnectAsync(EndPoint targetEndPoint) {
			if (targetEndPoint == null) {
				throw new ArgumentNullException(nameof(targetEndPoint));
			}

			Contract.Ensures(Contract.Result<Task<ClientTransport>>() != null);

			return ConnectAsyncCore(targetEndPoint);
		}

		/// <summary>
		///		Establishes logical connection, which specified to the managed transport protocol, for the server.
		/// </summary>
		/// <param name="targetEndPoint">The end point of target server.</param>
		/// <returns>
		///		<see cref="Task{T}"/> of <see cref="ClientTransport"/> which represents asynchronous establishment process
		///		specific to the managed transport.
		///		This value will not be <c>null</c>.
		/// </returns>
		protected abstract Task<ClientTransport> ConnectAsyncCore(EndPoint targetEndPoint);

		/// <summary>
		///		Handles socket error.
		/// </summary>
		/// <param name="socket">The <see cref="Socket"/> which might cause socket error.</param>
		/// <param name="context">The <see cref="SocketAsyncEventArgs"/> which holds actual error information.</param>
		/// <returns>
		///		<see cref="RpcErrorMessage"/> corresponds for the socket error.
		///		<c>null</c> if the operation result is not socket error.
		/// </returns>
		protected internal RpcErrorMessage? HandleSocketError(Socket socket, SocketAsyncEventArgs context) {
			if (context.SocketError.IsError() == false) {
				return null;
			}

			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.SocketError,
				"Socket error. {{ \"Socket\" : 0x{0:X}, \"RemoteEndpoint\" : \"{1}\", \"LocalEndpoint\" : \"{2}\", \"LastOperation\" : \"{3}\", \"SocketError\" : \"{4}\", \"ErrorCode\" : 0x{5:X} }}",
				ClientTransport.GetHandle(socket),
				ClientTransport.GetRemoteEndPoint(socket, context),
				ClientTransport.GetLocalEndPoint(socket),
				context.LastOperation,
				context.SocketError,
				(int)context.SocketError
			);

			return context.SocketError.ToClientRpcError();
		}

		/// <summary>
		///		Returns specified <see cref="ClientTransport"/> to the internal pool.
		/// </summary>
		/// <param name="transport">The <see cref="ClientTransport"/> to be returned.</param>
		internal abstract void ReturnTransport(ClientTransport transport);

		internal void HandleOrphan(int? messageId, RpcErrorMessage rpcError, MessagePackObject? returnValue) {
			OnUnknownResponseReceived(new UnknownResponseReceivedEventArgs(messageId, rpcError, returnValue));
		}

		internal ClientRequestContext GetRequestContext(ClientTransport transport) {
			Contract.Requires(transport != null);
			Contract.Ensures(Contract.Result<ClientRequestContext>() != null);

			var result = RequestContextPool.Borrow();
			result.SetTransport(transport);
			result.RenewSessionId();
			return result;
		}

		internal ClientResponseContext GetResponseContext(ClientTransport transport, EndPoint remoteEndPoint) {
			Contract.Requires(transport != null);
			Contract.Requires(remoteEndPoint != null);
			Contract.Ensures(Contract.Result<ClientResponseContext>() != null);

			var result = ResponseContextPool.Borrow();
			result.RenewSessionId();
			result.SetTransport(transport);
			result.RemoteEndPoint = remoteEndPoint;
			return result;
		}

		/// <summary>
		///		Returns the request context to the pool.
		/// </summary>
		/// <param name="context">The context to the pool.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is  <c>null</c>.
		/// </exception>
		protected internal void ReturnRequestContext(ClientRequestContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			Contract.EndContractBlock();

			context.Clear();
			context.UnboundTransport();
			RequestContextPool.Return(context);
		}

		/// <summary>
		///		Returns the response context to the pool.
		/// </summary>
		/// <param name="context">The response to the pool.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is  <c>null</c>.
		/// </exception>
		protected internal void ReturnResponseContext(ClientResponseContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			Contract.EndContractBlock();

			context.Clear();
			context.UnboundTransport();
			ResponseContextPool.Return(context);
		}

		/// <summary>
		///		Starts the connect timeout watching.
		/// </summary>
		/// <param name="onTimeout">A callback to be invoked when the timeout occurrs.</param>
		/// <returns>A <see cref="ConnectTimeoutWatcher"/> for connect timeout watching.</returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="onTimeout"/> is <c>null</c>.
		/// </exception>
		protected ConnectTimeoutWatcher BeginConnectTimeoutWatch(Action onTimeout) {
			if (onTimeout == null) {
				throw new ArgumentNullException(nameof(onTimeout));
			}

			Contract.Ensures(Contract.Result<ConnectTimeoutWatcher>() != null);

			if (configuration.ConnectTimeout == null) {
				return NullConnectTimeoutWatcher.Instance;
			}
			else {
				return new DefaultConnectTimeoutWatcher(configuration.ConnectTimeout.Value, onTimeout);
			}
		}

		/// <summary>
		///		Ends the connect timeout watching.
		/// </summary>
		/// <param name="watcher">The <see cref="ConnectTimeoutWatcher"/>.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="watcher"/> is <c>null</c>.
		/// </exception>
		protected void EndConnectTimeoutWatch(ConnectTimeoutWatcher watcher) {
			if (watcher == null) {
				throw new ArgumentNullException(nameof(watcher));
			}

			Contract.EndContractBlock();

			watcher.Dispose();
		}

		/// <summary>
		///		Helps connection timeout watching.
		/// </summary>
		protected abstract class ConnectTimeoutWatcher : IDisposable {
			internal ConnectTimeoutWatcher() { }

			/// <summary>
			///		Stops watching and release internal resources.
			/// </summary>
			public void Dispose() {
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			protected virtual void Dispose(bool disposing) {
				// nop
			}
		}

		sealed class NullConnectTimeoutWatcher : ConnectTimeoutWatcher {
			public static readonly NullConnectTimeoutWatcher Instance = new NullConnectTimeoutWatcher();

			NullConnectTimeoutWatcher() { }
		}

		sealed class DefaultConnectTimeoutWatcher : ConnectTimeoutWatcher {
			readonly TimeoutWatcher watcher;

			public DefaultConnectTimeoutWatcher(TimeSpan timeout, Action onTimeout) {
				var watcher = new TimeoutWatcher();
				watcher.Timeout += (sender, e) => onTimeout();
				Interlocked.Exchange(ref this.watcher, watcher);
				watcher.Start(timeout);
			}

			protected override void Dispose(bool disposing) {
				if (disposing) {
					watcher.Stop();
					watcher.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}

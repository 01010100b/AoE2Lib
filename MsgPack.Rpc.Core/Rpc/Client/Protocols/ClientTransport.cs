using MsgPack.Rpc.Core.Protocols;
using MsgPack.Rpc.Core.Protocols.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Defines interface of client protocol binding.
	/// </summary>
	public abstract partial class ClientTransport : IDisposable, IContextBoundableTransport {
		Socket boundSocket;

		/// <summary>
		///		Gets the bound <see cref="Socket"/>.
		/// </summary>
		/// <value>
		///		The bound <see cref="Socket"/>.
		///		This value might be <c>null</c> when any sockets have not been bound, or underlying protocol does not rely socket.
		/// </value>
		public Socket BoundSocket {
			get { return Interlocked.CompareExchange(ref boundSocket, null, null); }
			internal set { Interlocked.Exchange(ref boundSocket, value); }
		}

		Socket IContextBoundableTransport.BoundSocket => boundSocket;

		int isDisposed;

		/// <summary>
		///		Gets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposed => Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0;

		readonly ConcurrentDictionary<int, Action<ClientResponseContext, Exception, bool>> pendingRequestTable;
		readonly ConcurrentDictionary<long, Action<Exception, bool>> pendingNotificationTable;

		readonly ClientTransportManager manager;

		/// <summary>
		///		Gets the <see cref="ClientTransportManager"/> which manages this instance.
		/// </summary>
		/// <value>
		///		The <see cref="ClientTransportManager"/> which manages this instance.
		///		This value will not be <c>null</c>.
		/// </value>
		protected internal ClientTransportManager Manager {
			get {
				Contract.Ensures(Contract.Result<ClientTransportManager>() != null);

				return manager;
			}
		}

		/// <summary>
		///		Gets a value indicating whether the protocol used by this class can resume receiving.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can resume receiving; otherwise, <c>false</c>.
		/// </value>
		protected abstract bool CanResumeReceiving {
			get;
		}

		int shutdownSource;

		/// <summary>
		///		Gets a value indicating whether this instance is in shutdown.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is in shutdown; otherwise, <c>false</c>.
		/// </value>
		public bool IsClientShutdown => Interlocked.CompareExchange(ref shutdownSource, 0, 0) == (int)ShutdownSource.Client;

		/// <summary>
		///		Gets a value indicating whether this instance detectes server shutdown.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance detects server shutdown; otherwise, <c>false</c>.
		/// </value>
		public bool IsServerShutdown => Interlocked.CompareExchange(ref shutdownSource, 0, 0) == (int)ShutdownSource.Server;

		bool IsInAnyShutdown => Interlocked.CompareExchange(ref shutdownSource, 0, 0) != 0;

		internal IList<MessageFilter<ClientRequestContext>> AfterSerializationFilters { get; }

		internal IList<MessageFilter<ClientResponseContext>> BeforeDeserializationFilters { get; }

		EventHandler<ShutdownCompletedEventArgs> shutdownCompleted;

		/// <summary>
		///		Occurs when the initiated shutdown process is completed.
		/// </summary>
		internal event EventHandler<ShutdownCompletedEventArgs> ShutdownCompleted {
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
		///		Raises internal shutdown completion routine.
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

			var socket = Interlocked.Exchange(ref boundSocket, null);
			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.TransportShutdownCompleted,
				"Transport shutdown is completed. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
				GetHandle(socket),
				GetRemoteEndPoint(socket, default(MessageContext)),
				GetLocalEndPoint(socket)
			);

			if (socket != null) {
				socket.Close();
			}

			// Notify shutdown to waiting clients.
			// Note that it is OK from concurrency point of view because additional modifications are guarded via shutdown flag.
			var errorMessage = new RpcErrorMessage(RpcError.TransportError, string.Format(CultureInfo.CurrentCulture, "Transport is shutdown. Shutdown source is: {0}", e.Source), null);
			foreach (var entry in pendingRequestTable) {
				entry.Value(null, errorMessage.ToException(), false);
			}

			foreach (var entry in pendingNotificationTable) {
				entry.Value(errorMessage.ToException(), false);
			}

			pendingRequestTable.Clear();
			pendingNotificationTable.Clear();

			Interlocked.CompareExchange(ref shutdownCompleted, null, null)?.Invoke(this, e);
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientTransport"/> class.
		/// </summary>
		/// <param name="manager">The manager which will manage this instance.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="manager"/> is <c>null</c>.
		/// </exception>
		protected ClientTransport(ClientTransportManager manager) {
			Contract.EndContractBlock();

			this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
			pendingRequestTable = new ConcurrentDictionary<int, Action<ClientResponseContext, Exception, bool>>();
			pendingNotificationTable = new ConcurrentDictionary<long, Action<Exception, bool>>();

			AfterSerializationFilters =
				new ReadOnlyCollection<MessageFilter<ClientRequestContext>>(
					manager.Configuration.FilterProviders
					.OfType<MessageFilterProvider<ClientRequestContext>>()
					.Select(provider => provider.GetFilter(MessageFilteringLocation.AfterSerialization))
					.Where(filter => filter != null)
					.ToArray()
				);
			BeforeDeserializationFilters =
				new ReadOnlyCollection<MessageFilter<ClientResponseContext>>(
					manager.Configuration.FilterProviders
					.OfType<MessageFilterProvider<ClientResponseContext>>()
					.Select(provider => provider.GetFilter(MessageFilteringLocation.BeforeDeserialization))
					.Where(filter => filter != null)
					.Reverse()
					.ToArray()
				);
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (Interlocked.Exchange(ref isDisposed, 1) == 0) {
					try {
						var socket = BoundSocket;

						MsgPackRpcClientProtocolsTrace.TraceEvent(
							MsgPackRpcClientProtocolsTrace.DisposeTransport,
							"Dispose transport. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
							GetHandle(socket),
							GetRemoteEndPoint(socket, default(MessageContext)),
							GetLocalEndPoint(socket)
						);

						if (Interlocked.CompareExchange(ref shutdownSource, (int)ShutdownSource.Disposing, 0) == 0) {
							var closingSocket = Interlocked.Exchange(ref boundSocket, null);
							if (closingSocket != null) {
								closingSocket.Close();
							}
						}
					}
					catch (ObjectDisposedException) {
						MsgPackRpcClientProtocolsTrace.TraceEvent(
							MsgPackRpcClientProtocolsTrace.DisposeTransport,
							"Dispose transport. {{ \"Socket\" : \"Disposed\", \"RemoteEndPoint\" : \"Disposed\", \"LocalEndPoint\" : \"Disposed\" }}"
						);
					}
				}
			}
		}

		void VerifyIsNotDisposed() {
			if (IsDisposed) {
				throw new ObjectDisposedException(ToString());
			}
		}

		/// <summary>
		///		Initiates shutdown process.
		/// </summary>
		/// <returns>
		///		If shutdown process is initiated, then <c>true</c>.
		///		If shutdown is already initiated or completed, then <c>false</c>.
		/// </returns>
		public bool BeginShutdown() {
			if (Interlocked.CompareExchange(ref shutdownSource, (int)ShutdownSource.Client, 0) == 0) {
				ShutdownSending();

				if (pendingNotificationTable.Count == 0 && pendingRequestTable.Count == 0) {
					ShutdownReceiving();
				}

				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		///		When overridden in the derived class, shutdowns the sending.
		/// </summary>
		protected virtual void ShutdownSending() {
			var socket = BoundSocket;
			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.ShutdownSending,
				"Shutdown sending. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
				GetHandle(socket),
				GetRemoteEndPoint(socket, default(MessageContext)),
				GetLocalEndPoint(socket)
			);
		}

		/// <summary>
		///		When overridden in the derived class, shutdowns the receiving.
		/// </summary>
		protected virtual void ShutdownReceiving() {
			var socket = BoundSocket;
			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.ShutdownReceiving,
				"Shutdown receiving. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
				GetHandle(socket),
				GetRemoteEndPoint(socket, default(MessageContext)),
				GetLocalEndPoint(socket)
			);

			OnShutdownCompleted(new ShutdownCompletedEventArgs((ShutdownSource)shutdownSource));
		}

		/// <summary>
		///		Resets the connection.
		/// </summary>
		protected virtual void ResetConnection() {
			var socket = BoundSocket;
			if (socket != null) {
				// Reset immediately.
				socket.Close(0);
			}
		}

		void OnProcessFinished() {
			if (IsInAnyShutdown) {
				if (pendingNotificationTable.Count == 0 && pendingRequestTable.Count == 0) {
					ShutdownReceiving();
				}
			}
		}

		void OnSocketOperationCompleted(object sender, SocketAsyncEventArgs e) {
			var socket = sender as Socket;
			var context = e.GetContext();

			if (!HandleSocketError(socket, context)) {
				return;
			}

			switch (context.LastOperation) {
				case SocketAsyncOperation.Send:
				case SocketAsyncOperation.SendTo:
				case SocketAsyncOperation.SendPackets:
				{
					var requestContext = context as ClientRequestContext;
					Contract.Assert(requestContext != null);
					OnSent(requestContext);
					break;
				}
				case SocketAsyncOperation.Receive:
				case SocketAsyncOperation.ReceiveFrom:
				case SocketAsyncOperation.ReceiveMessageFrom:
				{
					var responseContext = context as ClientResponseContext;
					Contract.Assert(responseContext != null);
					OnReceived(responseContext);
					break;
				}
				default: {
					MsgPackRpcClientProtocolsTrace.TraceEvent(
						MsgPackRpcClientProtocolsTrace.UnexpectedLastOperation,
						"Unexpected operation. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\", \"LastOperation\" : \"{3}\" }}",
						GetHandle(socket),
						GetRemoteEndPoint(socket, e),
						GetLocalEndPoint(socket),
						context.LastOperation
					);
					break;
				}
			}
		}

		void IContextBoundableTransport.OnSocketOperationCompleted(object sender, SocketAsyncEventArgs e) {
			OnSocketOperationCompleted(sender, e);
		}


		void OnSendTimeout(object sender, EventArgs e) {
			OnWaitTimeout(sender as ClientRequestContext);
		}

		void OnReceiveTimeout(object sender, EventArgs e) {
			OnWaitTimeout(sender as ClientResponseContext);
		}

		void OnWaitTimeout(MessageContext context) {
			Contract.Assert(context != null);

			var asClientRequestContext = context as ClientRequestContext;

			var socket = BoundSocket;
			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.WaitTimeout,
					"Wait timeout. {{  \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\", \"Operation\" : \"{3}\", \"MessageType\" : \"{4}\"  \"MessageId\" : {5}, \"BytesTransferred\" : {6}, \"Timeout\" : \"{7}\" }}",
					GetHandle(socket),
					GetRemoteEndPoint(socket, context),
					GetLocalEndPoint(socket),
					asClientRequestContext != null ? "Send" : "Receive",
					asClientRequestContext == null ? MessageType.Response : asClientRequestContext.MessageType,
					context.MessageId,
					context.BytesTransferred,
					manager.Configuration.WaitTimeout
			);

			var rpcError =
				new RpcErrorMessage(
					RpcError.TimeoutError,
					new MessagePackObject(
						new MessagePackObjectDictionary(3)
						{
							{ RpcException.messageKeyUtf8, asClientRequestContext != null ? "Wait timeout on sending." : "Wait timeout on receiving." },
							{ RpcException.debugInformationKeyUtf8, string.Empty },
							{ RpcTimeoutException.ClientTimeoutKeyUtf8, Manager.Configuration.WaitTimeout.Value.Ticks }
						},
						true
					)
				);

			RaiseError(context.MessageId, context.SessionId, GetRemoteEndPoint(socket, context), rpcError, false);
			ResetConnection();
		}

		internal bool HandleSocketError(Socket socket, MessageContext context) {
			if (context.IsTimeout && context.SocketError == SocketError.OperationAborted) {
				return false;
			}

			var rpcError = Manager.HandleSocketError(socket, context.SocketContext);
			if (rpcError != null) {
				RaiseError(context.MessageId, context.SessionId, GetRemoteEndPoint(socket, context), rpcError.Value, context.CompletedSynchronously);
			}

			return rpcError == null;
		}

		void HandleDeserializationError(ClientResponseContext context, string message, Func<byte[]> invalidRequestHeaderProvider) {
			HandleDeserializationError(context, context.MessageId, new RpcErrorMessage(RpcError.RemoteRuntimeError, "Invalid stream.", message), message, invalidRequestHeaderProvider);
		}

		void HandleDeserializationError(ClientResponseContext context, int? messageId, RpcErrorMessage rpcError, string message, Func<byte[]> invalidRequestHeaderProvider) {
			MsgPackRpcClientProtocolsTrace.TraceRpcError(
				rpcError.Error,
				"Deserialization error. {0} {{ \"Message ID\" : {1}, \"Error\" : {2} }}",
				message,
				messageId == null ? "(null)" : messageId.ToString(),
				rpcError
			);

			if (invalidRequestHeaderProvider != null && MsgPackRpcClientProtocolsTrace.ShouldTrace(MsgPackRpcClientProtocolsTrace.DumpInvalidResponseHeader)) {
				var array = invalidRequestHeaderProvider();
				MsgPackRpcClientProtocolsTrace.TraceData(MsgPackRpcClientProtocolsTrace.DumpInvalidResponseHeader, BitConverter.ToString(array), array);
			}

			RaiseError(messageId, context.SessionId, GetRemoteEndPoint(BoundSocket, context), rpcError, context.CompletedSynchronously);

			context.nextProcess = DumpCorrupttedData;
		}

		void RaiseError(int? messageId, long sessionId, EndPoint remoteEndPoint, RpcErrorMessage rpcError, bool completedSynchronously) {
			if (messageId != null) {
				Action<ClientResponseContext, Exception, bool> handler = null;
				try {
					pendingRequestTable.TryRemove(messageId.Value, out handler);
				}
				finally {
					if (handler == null) {
						HandleOrphan(messageId, sessionId, remoteEndPoint, rpcError, null);
					}
					else {
						handler(null, rpcError.ToException(), completedSynchronously);
					}
				}
			}
			else {
				Action<Exception, bool> handler = null;
				try {
					pendingNotificationTable.TryRemove(sessionId, out handler);
				}
				finally {
					if (handler == null) {
						HandleOrphan(messageId, sessionId, remoteEndPoint, rpcError, null);
					}
					else {
						handler(rpcError.ToException(), completedSynchronously);
					}
				}
			}
		}

		void HandleOrphan(ClientResponseContext context) {
			var error = ErrorInterpreter.UnpackError(context);

			MessagePackObject? returnValue = null;
			if (error.IsSuccess) {
				returnValue = Unpacking.UnpackObject(context.resultBuffer);
			}

			HandleOrphan(context.MessageId, context.SessionId, GetRemoteEndPoint(BoundSocket, context), error, returnValue);
		}

		void HandleOrphan(int? messageId, long sessionId, EndPoint remoteEndPoint, RpcErrorMessage rpcError, MessagePackObject? returnValue) {
			var socket = BoundSocket;
			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.OrphanError,
				"There are no handlers to handle message which has MessageID:{0}, SessionID:{1}. This may indicate runtime problem or due to client recycling. {{ \"Socket\" : 0x{2:X}, \"RemoteEndPoint\" : \"{3}\", \"LocalEndPoint\" : \"{4}\", \"SessionID\" :{1}, \"MessageID\" : {0}, \"Error\" : {5}, \"ReturnValue\" : {6}, \"CallStack\" : \"{7}\" }}",
				messageId == null ? "(null)" : messageId.Value.ToString(CultureInfo.InvariantCulture),
				sessionId,
				GetHandle(socket),
				remoteEndPoint,
				GetLocalEndPoint(socket),
				rpcError,
				returnValue,
				new StackTrace(0, true)
 );

			manager.HandleOrphan(messageId, rpcError, returnValue);
		}

		Stream OpenDumpStream(DateTimeOffset sessionStartedAt, EndPoint destination, long sessionId, MessageType type, int? messageId) {
			var directoryPath =
				string.IsNullOrWhiteSpace(Manager.Configuration.CorruptResponseDumpOutputDirectory)
				? Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"MsgPack",
					"v" + typeof(ClientTransport).Assembly.GetName().Version,
					"Client",
					"Dump"
				)
				: Environment.ExpandEnvironmentVariables(Manager.Configuration.CorruptResponseDumpOutputDirectory);

			var filePath =
				Path.Combine(
					directoryPath,
					string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd_HH-mm-ss}-{1}-{2}-{3}{4}.dat", sessionStartedAt, destination == null ? string.Empty : FileSystem.EscapeInvalidPathChars(destination.ToString(), "_"), sessionId, type, messageId == null ? string.Empty : "-" + messageId)
				);

			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}

			return
				new FileStream(
					filePath,
					FileMode.Append,
					FileAccess.Write,
					FileShare.Read,
					64 * 1024,
					FileOptions.None
				);
		}

		static void ApplyFilters<T>(IList<MessageFilter<T>> filters, T context)
			where T : MessageContext {
			foreach (var filter in filters) {
				filter.ProcessMessage(context);
			}
		}

		/// <summary>
		///		Gets the <see cref="ClientRequestContext"/> to store context information for request or notification.
		/// </summary>
		/// <returns>
		///		The <see cref="ClientRequestContext"/> to store context information for request or notification.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		This object is not ready to invoke this method.
		/// </exception>
		public virtual ClientRequestContext GetClientRequestContext() {
			var context = Manager.GetRequestContext(this);
			return context;
		}

		/// <summary>
		///		Sends a request or notification message with the specified context.
		/// </summary>
		/// <param name="context">The context information.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="context"/> is not bound to this transport.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		///		This instance has been disposed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		This instance is in shutdown.
		///		Or the message ID or session ID is duplicated.
		/// </exception>
		/// <exception cref="RpcException">
		///		Failed to send request or notification to the server.
		/// </exception>
		public void Send(ClientRequestContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			if (!ReferenceEquals(context.BoundTransport, this)) {
				throw new ArgumentException("Context is not bound to this object.", nameof(context));
			}

			VerifyIsNotDisposed();

			if (IsClientShutdown) {
				throw new InvalidOperationException("This transport is in shutdown.");
			}

			Contract.EndContractBlock();

			if (IsServerShutdown) {
				throw new RpcErrorMessage(RpcError.TransportError, "Server did shutdown socket.", null).ToException();
			}

			context.Prepare();

			if (context.MessageType == MessageType.Request) {
				if (!pendingRequestTable.TryAdd(context.MessageId.Value, context.RequestCompletionCallback)) {
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Message ID '{0}' is already used.", context.MessageId));
				}
			}
			else {
				if (!pendingNotificationTable.TryAdd(context.SessionId, context.NotificationCompletionCallback)) {
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Session ID '{0}' is already used.", context.MessageId));
				}
			}

			if (MsgPackRpcClientProtocolsTrace.ShouldTrace(MsgPackRpcClientProtocolsTrace.SendOutboundData)) {
				var socket = BoundSocket;
				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.SendOutboundData,
					"Send request/notification. {{ \"SessionID\" : {0}, \"Socket\" : 0x{1:X}, \"RemoteEndPoint\" : \"{2}\", \"LocalEndPoint\" : \"{3}\", \"Type\" : \"{4}\", \"MessageID\" : {5}, \"Method\" : \"{6}\", \"BytesTransferring\" : {7} }}",
					context.SessionId,
					GetHandle(socket),
					GetRemoteEndPoint(socket, context),
					GetLocalEndPoint(socket),
					context.MessageType,
					context.MessageId,
					context.MethodName,
					context.sendingBuffer.Sum(segment => (long)segment.Count)
				);
			}

			// Because exceptions here means client error, it should be handled like other client error.
			// Therefore, no catch clauses here.
			ApplyFilters(AfterSerializationFilters, context);

			if (Manager.Configuration.WaitTimeout != null) {
				context.Timeout += OnSendTimeout;
				context.StartWatchTimeout(Manager.Configuration.WaitTimeout.Value);
			}

			SendCore(context);
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Send' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected abstract void SendCore(ClientRequestContext context);

		/// <summary>
		///		Called when asynchronous 'Send' operation is completed.
		/// </summary>
		/// <param name="context">Context information.</param>
		/// <returns>
		///		<c>true</c>, if the subsequent request is already received;
		///		<c>false</c>, otherwise.
		/// </returns>
		///	<exception cref="InvalidOperationException">
		///		This instance is not in 'Sending' state.
		///	</exception>
		///	<exception cref="ObjectDisposedException">
		///		This instance is disposed.
		///	</exception>
		protected virtual void OnSent(ClientRequestContext context) {
			if (MsgPackRpcClientProtocolsTrace.ShouldTrace(MsgPackRpcClientProtocolsTrace.SentOutboundData)) {
				var socket = BoundSocket;
				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.SentOutboundData,
						"Sent request/notification. {{ \"SessionID\" : {0}, \"Socket\" : 0x{1:X}, \"RemoteEndPoint\" : \"{2}\", \"LocalEndPoint\" : \"{3}\", \"Type\" : \"{4}\", \"MessageID\" : {5}, \"Method\" : \"{6}\", \"BytesTransferred\" : {7} }}",
						context.SessionId,
						GetHandle(socket),
						GetRemoteEndPoint(socket, context),
						GetLocalEndPoint(socket),
						context.MessageType,
						context.MessageId,
						context.MethodName,
						context.BytesTransferred
					);
			}

			context.StopWatchTimeout();
			context.Timeout -= OnSendTimeout;

			context.ClearBuffers();

			if (context.MessageType == MessageType.Notification) {
				try {
					Action<Exception, bool> handler = null;
					try {
						pendingNotificationTable.TryRemove(context.SessionId, out handler);
					}
					finally {
						var rpcError = context.SocketError.ToClientRpcError();
						handler?.Invoke(rpcError.IsSuccess ? null : rpcError.ToException(), context.CompletedSynchronously);
					}
				}
				finally {
					Manager.ReturnRequestContext(context);
					OnProcessFinished();
					Manager.ReturnTransport(this);
				}
			}
			else {
				if (Manager.Configuration.WaitTimeout != null
					&& (Manager.Configuration.WaitTimeout.Value - context.ElapsedTime).TotalMilliseconds < 1.0) {
					OnWaitTimeout(context);
					Manager.ReturnRequestContext(context);
					Manager.ReturnTransport(this);
					return;
				}

				var responseContext = Manager.GetResponseContext(this, context.RemoteEndPoint);

				if (Manager.Configuration.WaitTimeout != null) {
					responseContext.Timeout += OnReceiveTimeout;
					responseContext.StartWatchTimeout(Manager.Configuration.WaitTimeout.Value - context.ElapsedTime);
				}

				Manager.ReturnRequestContext(context);
				Receive(responseContext);
			}
		}


		/// <summary>
		///		Receives byte stream from remote end point.
		/// </summary>
		/// <param name="context">Context information.</param>
		///	<exception cref="InvalidOperationException">
		///		This instance is not in 'Idle' state.
		///	</exception>
		///	<exception cref="ObjectDisposedException">
		///		This instance is disposed.
		///	</exception>
		void Receive(ClientResponseContext context) {
			Contract.Assert(context != null);
			Contract.Assert(context.BoundTransport == this, "Context is not bound to this object.");

			// First, drain last received request.
			if (context.ReceivedData.Any(segment => 0 < segment.Count)) {
				DrainRemainingReceivedData(context);
			}
			else {
				// There might be dirty data due to client shutdown.
				context.ReceivedData.Clear();
				Array.Clear(context.CurrentReceivingBuffer, 0, context.CurrentReceivingBuffer.Length);

				context.PrepareReceivingBuffer();

				var socket = BoundSocket;
				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.BeginReceive,
					"Receive inbound data. {{  \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
					GetHandle(socket),
					GetRemoteEndPoint(socket, context),
					GetLocalEndPoint(socket)
				);
				ReceiveCore(context);
			}
		}

		void DrainRemainingReceivedData(ClientResponseContext context) {
			// Process remaining binaries. This pipeline recursively call this method on other thread.
			if (!context.nextProcess(context)) {
				// Draining was not ended. Try to take next bytes.
				Receive(context);
			}

			// This method must be called on other thread on the above pipeline, so exit this thread.
		}

		/// <summary>
		///		Performs protocol specific asynchronous 'Receive' operation.
		/// </summary>
		/// <param name="context">Context information.</param>
		protected abstract void ReceiveCore(ClientResponseContext context);

		/// <summary>
		///		Called when asynchronous 'Receive' operation is completed.
		/// </summary>
		/// <param name="context">Context information.</param>
		///	<exception cref="InvalidOperationException">
		///		This instance is not in 'Idle' nor 'Receiving' state.
		///	</exception>
		///	<exception cref="ObjectDisposedException">
		///		This instance is disposed.
		///	</exception>
		protected virtual void OnReceived(ClientResponseContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			Contract.EndContractBlock();

			if (MsgPackRpcClientProtocolsTrace.ShouldTrace(MsgPackRpcClientProtocolsTrace.ReceiveInboundData)) {
				var socket = BoundSocket;
				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.ReceiveInboundData,
					"Receive response. {{ \"SessionID\" : {0}, \"Socket\" : 0x{1:X}, \"RemoteEndPoint\" : \"{2}\", \"LocalEndPoint\" : \"{3}\", \"BytesTransfered\" : {4} }}",
					context.SessionId,
					GetHandle(socket),
					GetRemoteEndPoint(socket, context),
					GetLocalEndPoint(socket),
					context.BytesTransferred
				);
			}

			if (context.BytesTransferred == 0) {
				if (Interlocked.CompareExchange(ref shutdownSource, (int)ShutdownSource.Server, 0) == 0) {
					// Server sent shutdown response.
					ShutdownReceiving();

					// recv() returns 0 when the server socket shutdown gracefully.
					var socket = BoundSocket;
					MsgPackRpcClientProtocolsTrace.TraceEvent(
						MsgPackRpcClientProtocolsTrace.DetectServerShutdown,
						"Server shutdown current socket. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
						GetHandle(socket),
						GetRemoteEndPoint(socket, context),
						GetLocalEndPoint(socket)
					);
				}
				else if (IsClientShutdown) {
					// Client was started shutdown.
					ShutdownReceiving();
				}

				if (!context.ReceivedData.Any(segment => 0 < segment.Count)) {
					// There are no data to handle.
					FinishReceiving(context);
					return;
				}
			}
			else {
				context.ShiftCurrentReceivingBuffer();
			}

			if (MsgPackRpcClientProtocolsTrace.ShouldTrace(MsgPackRpcClientProtocolsTrace.DeserializeResponse)) {
				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.DeserializeResponse,
					"Deserialize response. {{ \"SessionID\" : {0}, \"Length\" : {1} }}",
					context.SessionId,
					context.ReceivedData.Sum(item => (long)item.Count)
				);
			}

			// Go deserialization pipeline.

			// Exceptions here means message error.
			try {
				ApplyFilters(BeforeDeserializationFilters, context);
			}
			catch (RpcException ex) {
				HandleDeserializationError(context, TryDetectMessageId(context), new RpcErrorMessage(ex.RpcError, ex.Message, ex.DebugInformation), "Filter rejects message.", () => context.ReceivedData.SelectMany(s => s.AsEnumerable()).ToArray());
				FinishReceiving(context);
				return;
			}

			if (!context.nextProcess(context)) {
				if (IsServerShutdown) {
					ShutdownReceiving();
				}
				else if (CanResumeReceiving) {
					// Wait to arrive more data from server.
					ReceiveCore(context);
					return;
				}

				//this.FinishReceiving( context );
				//return;
			}

			FinishReceiving(context);
		}

		void FinishReceiving(ClientResponseContext context) {
			context.StopWatchTimeout();
			context.Timeout -= OnReceiveTimeout;
			Manager.ReturnResponseContext(context);
			//this.Manager.ReturnTransport( this );
		}

		static int? TryDetectMessageId(ClientResponseContext context) {
			if (context.MessageId != null) {
				return context.MessageId;
			}

			using var stream = new ByteArraySegmentStream(context.ReceivedData);
			using var unpacker = Unpacker.Create(stream);

			if (!unpacker.Read() || !unpacker.IsArrayHeader || unpacker.LastReadData != 4) {
				// Not a response message
				return null;
			}

			if (!unpacker.Read() || !unpacker.LastReadData.IsTypeOf<int>().GetValueOrDefault() || unpacker.LastReadData != (int)MessageType.Response) {
				// Not a response message or invalid message type
				return null;
			}

			if (!unpacker.Read() || !unpacker.LastReadData.IsTypeOf<int>().GetValueOrDefault()) {
				// Invalid message ID.
				return null;
			}

			return unpacker.LastReadData.AsInt32();
		}

		/// <summary>
		///		Returns the specified context to the <see cref="Manager"/>.
		/// </summary>
		/// <param name="context">The <see cref="ClientRequestContext"/> to be returned.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="context"/> is not bound to this transport.
		/// </exception>
		public void ReturnContext(ClientRequestContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			if (!ReferenceEquals(context.BoundTransport, this)) {
				throw new ArgumentException("Context is not bound to this transport.", nameof(context));
			}

			Contract.EndContractBlock();

			Manager.ReturnRequestContext(context);
		}

		/// <summary>
		///		Returns the specified context to the <see cref="Manager"/>.
		/// </summary>
		/// <param name="context">The <see cref="ClientResponseContext"/> to be returned.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="context"/> is not bound to this transport.
		/// </exception>
		public void ReturnContext(ClientResponseContext context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			if (!ReferenceEquals(context.BoundTransport, this)) {
				throw new ArgumentException("Context is not bound to this transport.", nameof(context));
			}

			Contract.EndContractBlock();

			Manager.ReturnResponseContext(context);
		}


		#region -- Tracing --

		internal static IntPtr GetHandle(Socket socket) {
			if (socket != null) {
				try {
					return socket.Handle;
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			return IntPtr.Zero;
		}

		internal static EndPoint GetRemoteEndPoint(Socket socket, MessageContext context) {
			if (context != null) {
				try {
					var result = context.RemoteEndPoint;
					if (result != null) {
						return result;
					}
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			if (socket != null) {
				try {
					return socket.RemoteEndPoint;
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			return null;
		}

		internal static EndPoint GetRemoteEndPoint(Socket socket, SocketAsyncEventArgs context) {
			if (context != null) {
				try {
					var result = context.RemoteEndPoint;
					if (result != null) {
						return result;
					}
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			if (socket != null) {
				try {
					return socket.RemoteEndPoint;
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			return null;
		}

		internal static EndPoint GetLocalEndPoint(Socket socket) {
			if (socket != null) {
				try {
					return socket.LocalEndPoint;
				}
				catch (SocketException) { }
				catch (ObjectDisposedException) { }
			}

			return null;
		}

		#endregion
	}
}

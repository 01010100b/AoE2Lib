using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MsgPack.Rpc.Core.Client.Protocols;
using MsgPack.Serialization;

namespace MsgPack.Rpc.Core.Client {
	/// <summary>
	///		Entry point of MessagePack-RPC client.
	/// </summary>
	public sealed class RpcClient : IDisposable {
		static int messageIdGenerator;
		static int NextId() {
			return Interlocked.Increment(ref messageIdGenerator);
		}

		internal SerializationContext SerializationContext { get; }

		internal ClientTransportManager TransportManager { get; }

		ClientTransport transport;

		internal ClientTransport Transport => transport;

		TaskCompletionSource<object> transportShutdownCompletionSource;

		/// <summary>
		///		Gets a value indicating whether this instance is connected to the server.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is connected to the server; otherwise, <c>false</c>.
		/// </value>
		public bool IsConnected => Interlocked.CompareExchange(ref transport, null, null) != null;

		Task<ClientTransport> connectTask;

		internal void EnsureConnected() {
			var task = Interlocked.CompareExchange(ref connectTask, null, null);
			if (task != null) {
				Interlocked.Exchange(ref transport, task.Result);
				Interlocked.Exchange(ref connectTask, null);
				task.Dispose();
			}
		}

		bool isDisposed;

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcClient"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public RpcClient(EndPoint targetEndPoint) : this(targetEndPoint, null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcClient"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="configuration">
		///		A <see cref="RpcClientConfiguration"/> which contains protocol information etc.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public RpcClient(EndPoint targetEndPoint, RpcClientConfiguration configuration) : this(targetEndPoint, configuration, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcClient"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="serializationContext">
		///		A <see cref="SerializationContext"/> to hold serializers.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public RpcClient(EndPoint targetEndPoint, SerializationContext serializationContext) : this(targetEndPoint, null, serializationContext) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcClient"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="configuration">
		///		A <see cref="RpcClientConfiguration"/> which contains protocol information etc.
		/// </param>
		/// <param name="serializationContext">
		///		A <see cref="SerializationContext"/> to hold serializers.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public RpcClient(EndPoint targetEndPoint, RpcClientConfiguration configuration, SerializationContext serializationContext) {
			if (targetEndPoint == null) {
				throw new ArgumentNullException(nameof(targetEndPoint));
			}

			Contract.EndContractBlock();

			var safeConfiguration = configuration ?? RpcClientConfiguration.Default;

			TransportManager = safeConfiguration.TransportManagerProvider(safeConfiguration);
			SerializationContext = serializationContext ?? new SerializationContext();
			Interlocked.Exchange(ref connectTask, TransportManager.ConnectAsync(targetEndPoint));
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			isDisposed = true;
			var transport = Interlocked.CompareExchange(ref this.transport, null, null);
			if (transport != null) {
				transport.Dispose();
			}

			TransportManager.Dispose();
		}

		void VerifyIsNotDisposed() {
			if (isDisposed) {
				throw new ObjectDisposedException(ToString());
			}
		}

		/// <summary>
		///		Initiates shutdown of current connection and wait to complete it.
		/// </summary>
		public void Shutdown() {
			using var task = ShutdownAsync();

			if (task != null) {
				task.Wait();
			}
		}

		/// <summary>
		///		Initiates shutdown of current connection.
		/// </summary>
		/// <returns>
		///		The <see cref="Task"/> to wait to complete shutdown process.
		///		This value will be <c>null</c> when there is not the active connection.
		/// </returns>
		public Task ShutdownAsync() {
			VerifyIsNotDisposed();

			if (transportShutdownCompletionSource != null) {
				return null;
			}

			var taskCompletionSource = new TaskCompletionSource<object>();
			if (Interlocked.CompareExchange(ref transportShutdownCompletionSource, taskCompletionSource, null) != null) {
				return null;
			}

			transport.ShutdownCompleted += OnTranportShutdownComplete;
			transport.BeginShutdown();
			return taskCompletionSource.Task;
		}

		void OnTranportShutdownComplete(object sender, EventArgs e) {
			var taskCompletionSource = Interlocked.CompareExchange(ref transportShutdownCompletionSource, null, transportShutdownCompletionSource);
			if (taskCompletionSource != null) {
				var transport = sender as ClientTransport;
				transport.ShutdownCompleted -= OnTranportShutdownComplete;
				taskCompletionSource.SetResult(null);
			}
		}

		/// <summary>
		///		Calls specified remote method with specified argument and returns its result synchronously. 
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <returns>
		///		The <see cref="MessagePackObject"/> which represents the return value.
		///		Note that <c>nil</c> object will be returned when the remote method does not return any values (a.k.a. void).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		/// <exception cref="RpcException">
		///		Failed to execute specified remote method.
		/// </exception>
		public MessagePackObject Call(string methodName, params object[] arguments) {
			return EndCall(BeginCall(methodName, arguments, null, null));
		}

		/// <summary>
		///		Calls specified remote method with specified argument asynchronously. 
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <param name="asyncState">
		///		User supplied state object to be set as <see cref="P:Task.AsyncState"/>.
		/// </param>
		/// <returns>
		///		A <see cref="Task{T}"/> of <see cref="MessagePackObject"/> which represents asynchronous invocation.
		///		The resulting <see cref="MessagePackObject"/> which represents the return value.
		///		Note that <c>nil</c> object will be returned when the remote method does not return any values (a.k.a. void).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		/// <remarks>
		///		In .NET Framework 4.0, Silverlight 5, or Windows Phone 7.5, 
		///		the exception will be thrown as <see cref="AggregateException"/> in continuation <see cref="Task"/>.
		///		But this is because of runtime limitation, so this behavior will change in the future.
		///		You can <see cref="BeginCall"/> and <see cref="EndCall"/> to get appropriate exception directly.
		/// </remarks>
		public Task<MessagePackObject> CallAsync(string methodName, object[] arguments, object asyncState) {
			return Task.Factory.FromAsync(BeginCall, EndCall, methodName, arguments, asyncState, TaskCreationOptions.None);
		}

		/// <summary>
		///		Calls specified remote method with specified argument asynchronously. 
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <param name="asyncCallback">
		///		The callback method invoked when the notification is sent or the reponse is received.
		///		This value can be <c>null</c>.
		///		Usually this callback get the result of invocation via <see cref="EndCall"/>.
		/// </param>
		/// <param name="asyncState">
		///		User supplied state object which can be gotten via <see cref="IAsyncResult.AsyncState"/> in the <paramref name="asyncCallback"/> callback.
		///		This value can be <c>null</c>.
		/// </param>
		/// <returns>
		///		An <see cref="IAsyncResult" /> which can be passed to <see cref="EndCall"/> method.
		///		Usually, this value will be ignored because same instance will be passed to the <paramref name="asyncCallback"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		public IAsyncResult BeginCall(string methodName, object[] arguments, AsyncCallback asyncCallback, object asyncState) {
			VerifyIsNotDisposed();
			EnsureConnected();

			var messageId = NextId();
			var asyncResult = new RequestMessageAsyncResult(this, messageId, asyncCallback, asyncState);

			var isSucceeded = false;
			var context = transport.GetClientRequestContext();
			try {
				context.SetRequest(messageId, methodName, asyncResult.OnCompleted);
				if (arguments == null) {
					context.ArgumentsPacker.Pack(Array.Empty<MessagePackObject>());
				}
				else {
					context.ArgumentsPacker.PackArrayHeader(arguments.Length);
					foreach (var arg in arguments) {
						if (arg == null) {
							context.ArgumentsPacker.PackNull();
						}
						else {
							SerializationContext.GetSerializer(arg.GetType()).PackTo(context.ArgumentsPacker, arg);
						}
					}
				}

				transport.Send(context);
				isSucceeded = true;
			}
			finally {
				if (!isSucceeded) {
					transport.ReturnContext(context);
				}
			}

			return asyncResult;
		}

		/// <summary>
		///		Finishes asynchronous method invocation and returns its result.
		/// </summary>
		/// <param name="asyncResult">
		///		<see cref="IAsyncResult"/> returned from <see cref="BeginCall"/>.
		/// </param>
		/// <returns>
		///		The <see cref="MessagePackObject"/> which represents the return value.
		///		Note that <c>nil</c> object will be returned when the remote method does not return any values (a.k.a. void).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="asyncResult"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="asyncResult"/> is not valid type.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		<paramref name="asyncResult"/> is returned from other instance,
		///		or its state is not valid.
		/// </exception>
		/// <exception cref="RpcException">
		///		Failed to execute specified remote method.
		/// </exception>
		/// <remarks>
		///		You must call this method to clean up internal bookkeeping information and
		///		handles communication error
		///		even if the remote method returns <c>void</c>.
		/// </remarks>
		public MessagePackObject EndCall(IAsyncResult asyncResult) {
			VerifyIsNotDisposed();

			var requestAsyncResult = AsyncResult.Verify<RequestMessageAsyncResult>(asyncResult, this);
			requestAsyncResult.WaitForCompletion();
			requestAsyncResult.Finish();
			var result = requestAsyncResult.Result;
			Contract.Assert(result != null);
			return result.Value;
		}

		/// <summary>
		///		Sends specified remote method with specified argument as notification message synchronously.
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		/// <exception cref="RpcException">
		///		Failed to send notification message.
		/// </exception>
		public void Notify(string methodName, params object[] arguments) {
			EndNotify(BeginNotify(methodName, arguments, null, null));
		}

		/// <summary>
		///		Sends specified remote method with specified argument as notification message asynchronously.
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <param name="asyncState">
		///		User supplied state object to be set as <see cref="P:Task.AsyncState"/>.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> which represents asynchronous invocation.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		/// <remarks>
		///		In .NET Framework 4.0, Silverlight 5, or Windows Phone 7.5, 
		///		the exception will be thrown as <see cref="AggregateException"/> in continuation <see cref="Task"/>.
		///		But this is because of runtime limitation, so this behavior will change in the future.
		///		You can <see cref="BeginNotify"/> and <see cref="EndNotify"/> to get appropriate exception directly.
		/// </remarks>
		public Task NotifyAsync(string methodName, object[] arguments, object asyncState) {
			return Task.Factory.FromAsync(BeginNotify, EndNotify, methodName, arguments, asyncState, TaskCreationOptions.None);
		}

		/// <summary>
		///		Sends specified remote method with specified argument as notification message asynchronously.
		/// </summary>
		/// <param name="methodName">
		///		The name of target method.
		/// </param>
		/// <param name="arguments">
		///		Argument to be passed to the server.
		///		All values must be able to be serialized with MessagePack serializer.
		/// </param>
		/// <param name="asyncCallback">
		///		The callback method invoked when the notification is sent or the reponse is received.
		///		This value can be <c>null</c>.
		///		Usually this callback get the result of invocation via <see cref="EndNotify"/>.
		/// </param>
		/// <param name="asyncState">
		///		User supplied state object which can be gotten via <see cref="IAsyncResult.AsyncState"/> in the <paramref name="asyncCallback"/> callback.
		///		This value can be <c>null</c>.
		/// </param>
		/// <returns>
		///		An <see cref="IAsyncResult" /> which can be passed to <see cref="EndNotify"/> method.
		///		Usually, this value will be ignored because same instance will be passed to the <paramref name="asyncCallback"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is not valid.
		/// </exception>
		public IAsyncResult BeginNotify(string methodName, object[] arguments, AsyncCallback asyncCallback, object asyncState) {
			VerifyIsNotDisposed();
			EnsureConnected();

			var asyncResult = new NotificationMessageAsyncResult(this, asyncCallback, asyncState);

			var isSucceeded = false;
			var context = transport.GetClientRequestContext();
			try {
				context.SetNotification(methodName, asyncResult.OnCompleted);
				if (arguments == null) {
					context.ArgumentsPacker.Pack(Array.Empty<MessagePackObject>());
				}
				else {
					context.ArgumentsPacker.PackArrayHeader(arguments.Length);
					foreach (var arg in arguments) {
						if (arg == null) {
							context.ArgumentsPacker.PackNull();
						}
						else {
							SerializationContext.GetSerializer(arg.GetType()).PackTo(context.ArgumentsPacker, arg);
						}
					}
				}

				transport.Send(context);
				isSucceeded = true;
			}
			finally {
				if (!isSucceeded) {
					transport.ReturnContext(context);
				}
			}

			return asyncResult;
		}

		/// <summary>
		///		Finishes asynchronous method invocation.
		/// </summary>
		/// <param name="asyncResult">
		///		<see cref="IAsyncResult"/> returned from <see cref="BeginNotify"/>.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="asyncResult"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="asyncResult"/> is not valid type.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		<paramref name="asyncResult"/> is returned from other instance,
		///		or its state is not valid.
		/// </exception>
		/// <exception cref="RpcException">
		///		Failed to execute specified remote method.
		/// </exception>
		/// <remarks>
		///		You must call this method to clean up internal bookkeeping information and
		///		handles communication error.
		/// </remarks>
		public void EndNotify(IAsyncResult asyncResult) {
			VerifyIsNotDisposed();

			var notificationAsyncResult = AsyncResult.Verify<MessageAsyncResult>(asyncResult, this);
			notificationAsyncResult.WaitForCompletion();
			notificationAsyncResult.Finish();
		}
	}
}
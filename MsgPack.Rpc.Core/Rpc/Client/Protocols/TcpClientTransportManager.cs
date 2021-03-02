using MsgPack.Rpc.Core.Protocols;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Implements <see cref="ClientTransportManager{T}"/> for <see cref="TcpClientTransport"/>.
	/// </summary>
	public sealed class TcpClientTransportManager : ClientTransportManager<TcpClientTransport> {
		/// <summary>
		///		Initializes a new instance of the <see cref="TcpClientTransportManager"/> class.
		/// </summary>
		/// <param name="configuration">
		///		The <see cref="RpcClientConfiguration"/> which describes transport configuration.
		/// </param>
		public TcpClientTransportManager(RpcClientConfiguration configuration)
			: base(configuration) {
			SetTransportPool(configuration.TcpTransportPoolProvider(() => new TcpClientTransport(this), configuration.CreateTransportPoolConfiguration()));
		}

		/// <summary>
		///		Establishes logical connection, which specified to the managed transport protocol, for the server.
		/// </summary>
		/// <param name="targetEndPoint">The end point of target server.</param>
		/// <returns>
		///		<see cref="Task{T}"/> of <see cref="ClientTransport"/> which represents asynchronous establishment process specific to the managed transport.
		///		This value will not be <c>null</c>.
		/// </returns>
		protected sealed override Task<ClientTransport> ConnectAsyncCore(EndPoint targetEndPoint) {
			var source = new TaskCompletionSource<ClientTransport>();
			var context = new SocketAsyncEventArgs {
				RemoteEndPoint = targetEndPoint
			};
			context.Completed += OnCompleted;

			MsgPackRpcClientProtocolsTrace.TraceEvent(
				MsgPackRpcClientProtocolsTrace.BeginConnect,
				"Connecting. {{ \"EndPoint\" : \"{0}\", \"AddressFamily\" : {1}, \"PreferIPv4\" : {2}, \"OSSupportsIPv6\" : {3} }}",
				targetEndPoint,
				targetEndPoint.AddressFamily,
				Configuration.PreferIPv4,
				Socket.OSSupportsIPv6
			);

			context.UserToken =
				Tuple.Create(
					source,
					BeginConnectTimeoutWatch(
						() => {
							MsgPackRpcClientProtocolsTrace.TraceEvent(
								MsgPackRpcClientProtocolsTrace.ConnectTimeout,
								"Connect timeout. {{ \"EndPoint\" : \"{0}\", \"AddressFamily\" : {1}, \"PreferIPv4\" : {2}, \"OSSupportsIPv6\" : {3}, \"ConnectTimeout\" : {4} }}",
								targetEndPoint,
								targetEndPoint.AddressFamily,
								Configuration.PreferIPv4,
								Socket.OSSupportsIPv6,
								Configuration.ConnectTimeout
							);
							Socket.CancelConnectAsync(context);
						}
					)
				);

			if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, context)) {
				OnCompleted(null, context);
			}

			return source.Task;
		}

		void OnCompleted(object sender, SocketAsyncEventArgs e) {
			var socket = sender as Socket;
			var userToken = e.UserToken as Tuple<TaskCompletionSource<ClientTransport>, ConnectTimeoutWatcher>;
			var taskCompletionSource = userToken.Item1;
			var watcher = userToken.Item2;
			if (watcher != null) {
				EndConnectTimeoutWatch(watcher);
			}

			var error = HandleSocketError(e.ConnectSocket ?? socket, e);
			if (error != null) {
				taskCompletionSource.SetException(error.Value.ToException());
				return;
			}

			switch (e.LastOperation) {
				case SocketAsyncOperation.Connect: {
					OnConnected(e.ConnectSocket, e, taskCompletionSource);
					break;
				}
				default: {
					MsgPackRpcClientProtocolsTrace.TraceEvent(
						MsgPackRpcClientProtocolsTrace.UnexpectedLastOperation,
						"Unexpected operation. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\", \"LastOperation\" : \"{3}\" }}",
						ClientTransport.GetHandle(socket),
						ClientTransport.GetRemoteEndPoint(socket, e),
						ClientTransport.GetLocalEndPoint(socket),
						e.LastOperation
					);
					taskCompletionSource.SetException(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown socket operation : {0}", e.LastOperation)));
					break;
				}
			}
		}

		void OnConnected(Socket connectSocket, SocketAsyncEventArgs context, TaskCompletionSource<ClientTransport> taskCompletionSource) {
			try {
				if (connectSocket == null || !connectSocket.Connected) {
					// canceled.
					taskCompletionSource.SetException(
						new RpcTransportException(
							RpcError.ConnectionTimeoutError,
							"Connect timeout.",
							string.Format(CultureInfo.CurrentCulture, "Timeout: {0}", Configuration.ConnectTimeout)
						)
					);
					return;
				}

				MsgPackRpcClientProtocolsTrace.TraceEvent(
					MsgPackRpcClientProtocolsTrace.EndConnect,
					"Connected. {{ \"Socket\" : 0x{0:X}, \"RemoteEndPoint\" : \"{1}\", \"LocalEndPoint\" : \"{2}\" }}",
					ClientTransport.GetHandle(connectSocket),
					ClientTransport.GetRemoteEndPoint(connectSocket, context),
					ClientTransport.GetLocalEndPoint(connectSocket)
				);

				taskCompletionSource.SetResult(GetTransport(connectSocket));
			}
			finally {
				context.Dispose();
			}
		}

		/// <summary>
		///		Gets the transport managed by this instance.
		/// </summary>
		/// <param name="bindingSocket">The <see cref="Socket"/> to be bind the returning transport.</param>
		/// <returns>
		///		The transport managed by this instance.
		///		This implementation binds a valid <see cref="Socket"/> to the returning transport.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		<see cref="P:IsTransportPoolSet"/> is <c>false</c>.
		///		Or <paramref name="bindingSocket"/> is <c>null</c>.
		/// </exception>
		protected sealed override TcpClientTransport GetTransportCore(Socket bindingSocket) {
			if (bindingSocket == null) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "'bindingSocket' is required in {0}.", GetType()));
			}

			var transport = base.GetTransportCore(bindingSocket);
			BindSocket(transport, bindingSocket);
			return transport;
		}
	}
}

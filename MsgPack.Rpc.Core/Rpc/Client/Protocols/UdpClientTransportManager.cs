using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Implements <see cref="ClientTransportManager{T}"/> for <see cref="TcpClientTransport"/>.
	/// </summary>
	public sealed class UdpClientTransportManager : ClientTransportManager<UdpClientTransport> {
		/// <summary>
		///		Initializes a new instance of the <see cref="UdpClientTransportManager"/> class.
		/// </summary>
		/// <param name="configuration">
		///		The <see cref="RpcClientConfiguration"/> which describes transport configuration.
		/// </param>
		public UdpClientTransportManager(RpcClientConfiguration configuration)
			: base(configuration) {
			SetTransportPool(configuration.UdpTransportPoolProvider(() => new UdpClientTransport(this), configuration.CreateTransportPoolConfiguration()));
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
			var task = new Task<ClientTransport>(CreateTransport, targetEndPoint);
			task.RunSynchronously(TaskScheduler.Default);
			return task;
		}

		UdpClientTransport CreateTransport(object state) {
			var socket =
				new Socket(
					(Configuration.PreferIPv4 || !Socket.OSSupportsIPv6) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6,
					SocketType.Dgram,
					ProtocolType.Udp
				);

			var transport = GetTransport(socket);
			transport.RemoteEndPoint = state as EndPoint;
			return transport;
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
		protected sealed override UdpClientTransport GetTransportCore(Socket bindingSocket) {
			if (bindingSocket == null) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "'bindingSocket' is required in {0}.", GetType()));
			}

			var transport = base.GetTransportCore(bindingSocket);
			BindSocket(transport, bindingSocket);
			return transport;
		}
	}
}

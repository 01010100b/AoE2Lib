using MsgPack.Rpc.Core.Protocols;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Threading;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Manages <see cref="ClientTransport"/>s.
	/// </summary>
	/// <typeparam name="TTransport"></typeparam>
	public abstract class ClientTransportManager<TTransport> : ClientTransportManager
		where TTransport : ClientTransport {
		readonly ConcurrentDictionary<TTransport, object> activeTransports;

		ObjectPool<TTransport> transportPool;

		/// <summary>
		///		Gets a value indicating whether the transport pool is set.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the transport pool is set; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		///		To set transport pool, invoke <see cref="SetTransportPool"/> method.
		/// </remarks>
		public bool IsTransportPoolSet => transportPool != null;

		int tranportIsInShutdown;

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientTransportManager{T}"/> class.
		/// </summary>
		/// <param name="configuration">
		///		The <see cref="RpcClientConfiguration"/> which contains various configuration information.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="configuration"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		///		The derived class must call <see cref="SetTransportPool"/> in the end of constructor
		///		unless it implements special process which does not use transport pool at all.
		/// </remarks>
		protected ClientTransportManager(RpcClientConfiguration configuration)
			: base(configuration) {
			activeTransports = new ConcurrentDictionary<TTransport, object>();
		}

		/// <summary>
		///		Sets the <see cref="ObjectPool{T}"/> of <typeparamref name="TTransport"/> to this instance.
		/// </summary>
		/// <param name="transportPool">
		///		The <see cref="ObjectPool{T}"/> of <typeparamref name="TTransport"/> to be set.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="transportPool"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		This method is called more than once for this instance.
		/// </exception>
		protected void SetTransportPool(ObjectPool<TTransport> transportPool) {
			Contract.EndContractBlock();

			if (this.transportPool != null) {
				throw new InvalidOperationException("Already set.");
			}

			this.transportPool = transportPool ?? throw new ArgumentNullException(nameof(transportPool));
		}

		/// <summary>
		///		Initiates protocol specific shutdown process.
		/// </summary>
		protected override void BeginShutdownCore() {
			var registered = 0;

			foreach (var transport in activeTransports) {
				try { }
				finally {
					transport.Key.ShutdownCompleted += OnTransportShutdownCompleted;
					Interlocked.Increment(ref tranportIsInShutdown);
					registered++;
				}

				if (!transport.Key.BeginShutdown()) {
					try { }
					finally {
						transport.Key.ShutdownCompleted -= OnTransportShutdownCompleted;
						Interlocked.Increment(ref tranportIsInShutdown);
						registered--;
					}
				}
			}

			base.BeginShutdownCore();

			if (registered == 0) {
				OnShutdownCompleted(new ShutdownCompletedEventArgs(ShutdownSource.Client));
			}
		}

		void OnTransportShutdownCompleted(object sender, ShutdownCompletedEventArgs e) {
			var transport = sender as TTransport;
			Contract.Assert(transport != null);
			try { }
			finally {
				transport.ShutdownCompleted -= OnTransportShutdownCompleted;
				if (Interlocked.Decrement(ref tranportIsInShutdown) == 0) {
					OnShutdownCompleted(e);
				}
			}
		}

		/// <summary>
		///		Gets the transport managed by this instance.
		/// </summary>
		/// <param name="bindingSocket">The <see cref="Socket"/> to be bind the returning transport.</param>
		/// <returns>
		///		The transport managed by this instance.
		///		Note that <see cref="ClientTransport.BoundSocket"/> might be <c>null</c> depends on <see cref="GetTransportCore"/> implementation.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsTransportPoolSet"/> is <c>false</c>.
		///		Or <paramref name="bindingSocket"/> cannot be <c>null</c> for the current transport.
		/// </exception>
		protected TTransport GetTransport(Socket bindingSocket) {
			Contract.Ensures(Contract.Result<TTransport>() != null);
			Contract.Ensures(Contract.Result<TTransport>().BoundSocket == null || Contract.Result<TTransport>().BoundSocket == bindingSocket);


			TTransport transport;
			try { }
			finally {
				transport = GetTransportCore(bindingSocket);
				transport.BoundSocket = bindingSocket;
				activeTransports.TryAdd(transport, null);
			}

			return transport;
		}

		/// <summary>
		///		Gets the transport managed by this instance.
		/// </summary>
		/// <param name="bindingSocket">The <see cref="Socket"/> to be bind the returning transport.</param>
		/// <returns>
		///		The transport managed by this instance.
		///		Note that <see cref="ClientTransport.BoundSocket"/> might be <c>null</c> depends on <see cref="GetTransportCore"/> implementation.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsTransportPoolSet"/> is <c>false</c>.
		///		Or <paramref name="bindingSocket"/> cannot be <c>null</c> for the current transport.
		/// </exception>
		/// <remarks>
		///		This implementation does not bind <paramref name="bindingSocket"/> to the returning transport.
		///		The derived class which uses <see cref="Socket"/> for its communication, must set the transport via <see cref="BindSocket"/> method.
		/// </remarks>
		protected virtual TTransport GetTransportCore(Socket bindingSocket) {
			Contract.Ensures(Contract.Result<TTransport>() != null);
			Contract.Ensures(Contract.Result<TTransport>().BoundSocket == null || Contract.Result<TTransport>().BoundSocket == bindingSocket);

			if (!IsTransportPoolSet) {
				throw new InvalidOperationException("Transport pool must be set via SetTransportPool().");
			}

			var transport = transportPool.Borrow();
			return transport;
		}

		/// <summary>
		///		Binds the specified socket to the specified transport.
		/// </summary>
		/// <param name="transport">The transport to be bound.</param>
		/// <param name="bindingSocket">The socket to be bound.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="transport"/> is <c>null</c>.
		///		Or <paramref name="bindingSocket"/> is <c>null</c>.
		/// </exception>
		protected void BindSocket(TTransport transport, Socket bindingSocket) {
			if (transport == null) {
				throw new ArgumentNullException(nameof(transport));
			}

			Contract.EndContractBlock();

			transport.BoundSocket = bindingSocket ?? throw new ArgumentNullException(nameof(bindingSocket));
		}

		/// <summary>
		///		Returns specified <see cref="ClientTransport"/> to the internal pool.
		/// </summary>
		/// <param name="transport">The <see cref="ClientTransport"/> to be returned.</param>
		internal sealed override void ReturnTransport(ClientTransport transport) {
			ReturnTransport((TTransport)transport);
		}

		/// <summary>
		///		Invoked from the <see cref="ClientTransport"/> which was created by this manager,
		///		returns the transport to this manager.
		/// </summary>
		/// <param name="transport">The <see cref="ClientTransport"/> which was created by this manager.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="transport"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="transport"/> is not managed by this manager.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsTransportPoolSet"/> is <c>false</c>.
		/// </exception>
		protected void ReturnTransport(TTransport transport) {
			if (transport == null) {
				throw new ArgumentNullException(nameof(transport));
			}

			if (!ReferenceEquals(this, transport.Manager)) {
				throw new ArgumentException("The specified transport is not owned by this manager.", nameof(transport));
			}

			if (!IsTransportPoolSet) {
				throw new InvalidOperationException("Transport pool must be set via SetTransportPool().");
			}

			Contract.EndContractBlock();

			try { }
			finally {
				activeTransports.TryRemove(transport, out _);
				ReturnTransportCore(transport);
			}
		}

		/// <summary>
		///		Invoked from <see cref="ReturnTransport(TTransport)"/>, returns the transport to this manager.
		/// </summary>
		/// <param name="transport">The <see cref="ClientTransport"/> which was created by this manager.</param>
		protected virtual void ReturnTransportCore(TTransport transport) {
			Contract.Requires(transport != null);
			Contract.Requires(ReferenceEquals(this, transport.Manager));
			Contract.Requires(IsTransportPoolSet);

			if (!transport.IsDisposed && !transport.IsClientShutdown && !transport.IsServerShutdown) {
				transportPool.Return(transport);
			}
		}
	}
}

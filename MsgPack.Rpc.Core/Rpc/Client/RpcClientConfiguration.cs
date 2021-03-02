using MsgPack.Rpc.Core.Protocols.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.Client {
	/// <summary>
	///		Represents client side configuration settings.
	/// </summary>
	public sealed partial class RpcClientConfiguration : FreezableObject {
		static readonly RpcClientConfiguration _default = new RpcClientConfiguration().Freeze();

		/// <summary>
		///		Gets the default frozen instance.
		/// </summary>
		/// <value>
		///		The default frozen instance.
		///		This value will not be <c>null</c>.
		/// </value>
		public static RpcClientConfiguration Default {
			get {
				Contract.Ensures(Contract.Result<RpcClientConfiguration>() != null);

				return _default;
			}
		}

		/// <summary>
		///		Gets the filter providers collection.
		/// </summary>
		/// <value>
		///		The filter providers collection. Default is empty.
		/// </value>
		public IList<MessageFilterProvider> FilterProviders { get; private set; } = new List<MessageFilterProvider>();

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcClientConfiguration"/> class.
		/// </summary>
		public RpcClientConfiguration() { }

		/// <summary>
		///		Creates the <see cref="ObjectPoolConfiguration"/> for the transport pool corresponds to values of this instance.
		/// </summary>
		/// <returns>
		///		The <see cref="ObjectPoolConfiguration"/> for the transport pool corresponds to values of this instance.
		///		This value will not be <c>null</c>.
		/// </returns>
		public ObjectPoolConfiguration CreateTransportPoolConfiguration() {
			Contract.Ensures(Contract.Result<ObjectPoolConfiguration>() != null);

			return new ObjectPoolConfiguration() { ExhausionPolicy = ExhausionPolicy.BlockUntilAvailable, MaximumPooled = MaximumConcurrentRequest, MinimumReserved = MinimumConcurrentRequest };
		}

		/// <summary>
		///		Creates the <see cref="ObjectPoolConfiguration"/> for the <see cref="Protocols.ClientRequestContext"/> pool corresponds to values of this instance.
		/// </summary>
		/// <returns>
		///		The <see cref="ObjectPoolConfiguration"/> for the <see cref="Protocols.ClientRequestContext"/> pool corresponds to values of this instance.
		///		This value will not be <c>null</c>.
		/// </returns>
		public ObjectPoolConfiguration CreateRequestContextPoolConfiguration() {
			Contract.Ensures(Contract.Result<ObjectPoolConfiguration>() != null);

			return new ObjectPoolConfiguration() { ExhausionPolicy = ExhausionPolicy.BlockUntilAvailable, MaximumPooled = MaximumConcurrentRequest, MinimumReserved = MinimumConcurrentRequest };
		}

		/// <summary>
		///		Creates the <see cref="ObjectPoolConfiguration"/> for the <see cref="Protocols.ClientResponseContext"/> pool corresponds to values of this instance.
		/// </summary>
		/// <returns>
		///		The <see cref="ObjectPoolConfiguration"/> for the <see cref="Protocols.ClientResponseContext"/> pool corresponds to values of this instance.
		///		This value will not be <c>null</c>.
		/// </returns>
		public ObjectPoolConfiguration CreateResponseContextPoolConfiguration() {
			Contract.Ensures(Contract.Result<ObjectPoolConfiguration>() != null);

			return new ObjectPoolConfiguration() { ExhausionPolicy = ExhausionPolicy.BlockUntilAvailable, MaximumPooled = MaximumConcurrentRequest, MinimumReserved = MinimumConcurrentRequest };
		}

		/// <summary>
		///		Clones all of the fields of this instance.
		/// </summary>
		/// <returns>
		///		The shallow copy of this instance.
		/// </returns>
		public RpcClientConfiguration Clone() {
			Contract.Ensures(Contract.Result<RpcClientConfiguration>() != null);
			Contract.Ensures(!ReferenceEquals(Contract.Result<RpcClientConfiguration>(), this));
			Contract.Ensures(Contract.Result<RpcClientConfiguration>().IsFrozen == IsFrozen);

			return CloneCore() as RpcClientConfiguration;
		}

		/// <summary>
		///		Freezes this instance.
		/// </summary>
		/// <returns>
		///		This instance.
		/// </returns>
		public RpcClientConfiguration Freeze() {
			Contract.Ensures(ReferenceEquals(Contract.Result<RpcClientConfiguration>(), this));
			Contract.Ensures(IsFrozen);

			return FreezeCore() as RpcClientConfiguration;
		}

		/// <summary>
		/// Gets the frozen copy of this instance.
		/// </summary>
		/// <returns>
		/// This instance if it is already frozen.
		/// Otherwise, frozen copy of this instance.
		/// </returns>
		public RpcClientConfiguration AsFrozen() {
			Contract.Ensures(Contract.Result<RpcClientConfiguration>() != null);
			Contract.Ensures(!ReferenceEquals(Contract.Result<RpcClientConfiguration>(), this));
			Contract.Ensures(Contract.Result<RpcClientConfiguration>().IsFrozen);
			Contract.Ensures(IsFrozen == Contract.OldValue(IsFrozen));

			return AsFrozenCore() as RpcClientConfiguration;
		}

		/// <summary>
		///		Clones all of the fields of this instance.
		/// </summary>
		/// <returns>
		///		The shallow copy of this instance. Returned instance always is not frozen.
		/// </returns>
		protected override FreezableObject CloneCore() {
			var result = base.CloneCore() as RpcClientConfiguration;
			result.FilterProviders = new List<MessageFilterProvider>(result.FilterProviders);
			return result;
		}

		/// <summary>
		///		Freezes this instance.
		/// </summary>
		/// <returns>
		///		This instance.
		/// </returns>
		protected override FreezableObject FreezeCore() {
			var result = base.FreezeCore() as RpcClientConfiguration;
			result.FilterProviders = new ReadOnlyCollection<MessageFilterProvider>(result.FilterProviders);
			return result;
		}
	}
}

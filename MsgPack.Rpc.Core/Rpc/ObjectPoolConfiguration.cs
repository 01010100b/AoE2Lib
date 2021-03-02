namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Represents configuratin of the <see cref="ObjectPool{T}"/>.
	/// </summary>
	public sealed partial class ObjectPoolConfiguration : FreezableObject {
		/// <summary>
		///		Gets the default frozen instance.
		/// </summary>
		/// <value>
		///		The default frozen instance.
		///		This value will not be <c>null</c>.
		/// </value>
		public static ObjectPoolConfiguration Default { get; } = new ObjectPoolConfiguration().AsFrozen();

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectPoolConfiguration"/> class.
		/// </summary>
		public ObjectPoolConfiguration() { }

		/// <summary>
		///		Clones all of the fields of this instance.
		/// </summary>
		/// <returns>
		///		The shallow copy of this instance.
		/// </returns>
		public ObjectPoolConfiguration Clone() {
			return CloneCore() as ObjectPoolConfiguration;
		}

		/// <summary>
		///		Freezes this instance.
		/// </summary>
		/// <returns>
		///		This instance.
		/// </returns>
		public ObjectPoolConfiguration Freeze() {
			return FreezeCore() as ObjectPoolConfiguration;
		}

		/// <summary>
		/// Gets the frozen copy of this instance.
		/// </summary>
		/// <returns>
		/// This instance if it is already frozen.
		/// Otherwise, frozen copy of this instance.
		/// </returns>
		public ObjectPoolConfiguration AsFrozen() {
			return AsFrozenCore() as ObjectPoolConfiguration;
		}
	}
}

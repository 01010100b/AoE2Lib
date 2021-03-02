namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Defines exhausion policy of the object pool.
	/// </summary>
	public enum ExhausionPolicy {
		/// <summary>
		///		Blocks the caller threads until any objects will be available.
		/// </summary>
		BlockUntilAvailable,

		/// <summary>
		///		Throws the <see cref="ObjectPoolEmptyException"/> immediately.
		/// </summary>
		ThrowException
	}
}

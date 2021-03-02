namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Represents the source which caused transport shutdown.
	/// </summary>
	public enum ShutdownSource {
		/// <summary>
		///		Unknown. This might indicate internal runtime error.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///		Client initiated the shutdown, it might be done in normal shutdown sequence, or client failure.
		/// </summary>
		Client = 1,

		/// <summary>
		///		Server initiated the shutdown, it might indicates server maintenance or failure.
		/// </summary>
		Server = 2,

		/// <summary>
		///		Disposing current transport causes rudely shutdown.
		/// </summary>
		Disposing = 3
	}
}
namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Define type of MsgPack-RPC message packet.
	/// </summary>
	public enum MessageType {
		/// <summary>
		///		Request message which will be replied as response.
		/// </summary>
		Request = 0,

		/// <summary>
		///		Response message to the request message.
		/// </summary>
		Response = 1,

		/// <summary>
		///		One way notification message.
		/// </summary>
		Notification = 2
	}
}

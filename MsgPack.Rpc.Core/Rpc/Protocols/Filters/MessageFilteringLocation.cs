namespace MsgPack.Rpc.Core.Protocols.Filters {
	/// <summary>
	///		Represents location of the message filter is applied.
	/// </summary>
	public enum MessageFilteringLocation {
		/// <summary>
		///		After outbound message serialization.
		/// </summary>
		AfterSerialization,

		/// <summary>
		///		Before inbound message deserialization.
		/// </summary>
		BeforeDeserialization,
	}
}
using System;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Marks the method can be invoked as MessagePack-RPC method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class MessagePackRpcMethodAttribute : Attribute {
		/// <summary>
		///		Initializes a new instance of the <see cref="MessagePackRpcMethodAttribute"/> class.
		/// </summary>
		public MessagePackRpcMethodAttribute() { }
	}
}

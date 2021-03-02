using System;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Thrown if incoming MsgPack-RPC message exceeds the quota.
	/// </summary>
	[Serializable]
	public sealed class RpcMessageTooLongException : RpcProtocolException {
		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMessageTooLongException"/> class with the default error message.
		/// </summary>
		public RpcMessageTooLongException() : this(null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMessageTooLongException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">
		///		Error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		Debug information of error.
		///		This value can be null for security reason, and its contents are for developers, not end users.
		/// </param>
		public RpcMessageTooLongException(string message, string debugInformation) : this(message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMessageTooLongException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="message">
		///		Error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		Debug information of error.
		///		This value can be null for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <param name="inner">
		///		Exception which caused this error.
		/// </param>		
		public RpcMessageTooLongException(string message, string debugInformation, Exception inner) : base(RpcError.MessageTooLargeError, message, debugInformation, inner) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcException"/> class with the unpacked data.
		/// </summary>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		internal RpcMessageTooLongException(MessagePackObject unpackedException) : base(RpcError.MessageTooLargeError, unpackedException) { }
	}
}

using System;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Exception thrown when some error ocurred above transport layer in remote server.
	/// </summary>
	/// <remarks>
	///		In MessagePack-RPC, it is possible that remote server is not even CLI environment.
	///		For example, it might be JVM environment, native C++, Ruby runtime, native D language, or so.
	///		Therefore, it is impossible to represent application-specific error as <see cref="Exception"/> since an exception is environment-specific representation of a error.
	///		The solution is to pack error information to Message-Pack map representation.
	///		So, this class wraps the map as CLI <see cref="Exception"/> to interoperate MessagePack-RPC and CLI environment.
	/// </remarks>
	[Serializable]
	public class RpcFaultException : RpcException {
		/// <summary>
		///		Initializes a new instance of the <see cref="RpcFaultException"/> class with a specified error message.
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.RemoteRuntimeError"/> is used.
		///	</param>
		/// <param name="message">
		///		Error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		Debug information of error.
		///		This value can be null for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <remarks>
		///		<para>
		///			For example, if some exception is occurred in server application,
		///			the value of <see cref="Exception.ToString()"/> should specify for <paramref name="debugInformation"/>.
		///			And then, user-friendly, safe message should be specified to <paramref name="message"/> like 'Internal Error."
		///		</para>
		///		<para>
		///			MessagePack-RPC for CLI runtime does not propagate <see cref="RpcException.DebugInformation"/> for remote endpoint.
		///			So you should specify some error handler to instrument it (e.g. logging handler).
		///		</para>
		/// </remarks>
		public RpcFaultException(RpcError rpcError, string message, string debugInformation) : base(rpcError, message, debugInformation) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcFaultException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.RemoteRuntimeError"/> is used.
		///	</param>
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
		/// <remarks>
		///		<para>
		///			For example, if some exception is occurred in server application,
		///			the value of <see cref="Exception.ToString()"/> should specify for <paramref name="debugInformation"/>.
		///			And then, user-friendly, safe message should be specified to <paramref name="message"/> like 'Internal Error."
		///		</para>
		///		<para>
		///			MessagePack-RPC for CLI runtime does not propagate <see cref="RpcException.DebugInformation"/> for remote endpoint.
		///			So you should specify some error handler to instrument it (e.g. logging handler).
		///		</para>
		/// </remarks>
		public RpcFaultException(RpcError rpcError, string message, string debugInformation, Exception inner) : base(rpcError, message, debugInformation, inner) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcFaultException"/> class with the unpacked data.
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.RemoteRuntimeError"/> is used.
		///	</param>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		protected internal RpcFaultException(RpcError rpcError, MessagePackObject unpackedException)
			: base(rpcError, unpackedException) { }
	}
}

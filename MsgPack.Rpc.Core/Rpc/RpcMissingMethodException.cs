using MsgPack.Rpc.Core.Protocols;
using System;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Thrown when specified method is not exist on remote server.
	/// </summary>
	[Serializable]
	public sealed class RpcMissingMethodException : RpcMethodInvocationException {
		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMissingMethodException"/> class with the default error message.
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is empty or blank.
		/// </exception>
		public RpcMissingMethodException(string methodName)
			: this(methodName, null, null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMissingMethodException"/> class with a specified error message.
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		/// <param name="message">
		///		Error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		Debug information of error.
		///		This value can be null for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is empty or blank.
		/// </exception>
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
		public RpcMissingMethodException(string methodName, string message, string debugInformation)
			: this(methodName, message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMissingMethodException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
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
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is empty or blank.
		/// </exception>
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
		public RpcMissingMethodException(string methodName, string message, string debugInformation, Exception inner)
			: base(RpcError.NoMethodError, methodName, message ?? RpcError.NoMethodError.DefaultMessage, debugInformation, inner) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMissingMethodException"/> class with the unpacked data.
		/// </summary>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		internal RpcMissingMethodException(MessagePackObject unpackedException)
			: base(RpcError.NoMethodError, unpackedException) { }
	}
}

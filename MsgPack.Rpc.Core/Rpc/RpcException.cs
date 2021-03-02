using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Represents MessagePack-RPC related exception.
	/// </summary>
	/// <remarks>
	///		<para>
	///		</para>
	///		<para>
	///			There is no specification to represent error in MessagePack-RPC,
	///			but de-facto is map which has following structure:
	///			<list type="table">
	///				<listheader>
	///					<term>Key</term>
	///					<description>Value</description>
	///				</listheader>
	///				<item>
	///					<term>ErrorCode</term>
	///					<description>
	///						<para><strong>Type:</strong><see cref="int"/></para>
	///						<para><strong>Value:</strong>
	///						Error code to identify error type.
	///						</para>
	///					</description>
	///					<term>Description</term>
	///					<description>
	///						<para><strong>Type:</strong><see cref="string"/></para>
	///						<para><strong>Value:</strong>
	///						Description of message.
	///						<note>
	///							Note that this value should not contain any sensitive information.
	///							Since detailed error information might be exploit for clackers,
	///							this value should not contain such information.
	///						</note>
	///						</para>
	///					</description>
	///					<term>DebugInformation</term>
	///					<description>
	///						<para><strong>Type:</strong><see cref="string"/></para>
	///						<para><strong>Value:</strong>
	///						Detailed information to debug.
	///						This value is optional.
	///						Server should send this information only when target end point (client) is certainly localhost 
	///						or server is explicitly configured as testing environment.
	///						</para>
	///					</description>
	///				</item>
	///			</list>
	///		</para>
	/// </remarks>
	[Serializable]
	public partial class RpcException : Exception {
		// NOT readonly for safe-deserialization
		RpcError rpcError;

		/// <summary>
		///		Gets the metadata of the error.
		/// </summary>
		/// <value>
		///		The metadata of the error. This value will not be <c>null</c>.
		/// </value>
		public RpcError RpcError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				Contract.Assert(rpcError != null);
				return rpcError;
			}
		}

		// NOT readonly for safe-deserialization
		string debugInformation;

		/// <summary>
		///		Gets the debug information of the error.
		/// </summary>
		/// <value>
		///		The debug information of the error.
		///		This value will not be <c>null</c> but may be empty for security reason, and its contents are for developers, not end users.
		/// </value>
		public string DebugInformation => debugInformation ?? string.Empty;

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcException"/> class with a specified error message.
		/// </summary>
		/// <param name="rpcError">
		///		The metadata of the error. If <c>null</c> is specified, the <see cref="Core.RpcError.RemoteRuntimeError"/> is used.
		///	</param>
		/// <param name="message">
		///		The error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		The debug information of the error.
		///		This value can be <c>null</c> for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <remarks>
		///		<para>
		///			For example, if some exception is occurred in server application,
		///			the value of <see cref="Exception.ToString()"/> should specify for <paramref name="debugInformation"/>.
		///			And then, user-friendly, safe message should be specified to <paramref name="message"/> like 'Internal Error."
		///		</para>
		///		<para>
		///			MessagePack-RPC for CLI runtime does not propagate <see cref="DebugInformation"/> for remote endpoint.
		///			So you should specify some error handler to instrument it (e.g. logging handler).
		///		</para>
		/// </remarks>
		public RpcException(RpcError rpcError, string message, string debugInformation) : this(rpcError, message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="rpcError">
		///		The metadata of the error. If <c>null</c> is specified, the <see cref="Core.RpcError.RemoteRuntimeError"/> is used.
		///	</param>
		/// <param name="message">
		///		The error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		The debug information of the error.
		///		This value can be <c>null</c> for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <param name="inner">
		///		The exception which caused this error.
		/// </param>
		/// <remarks>
		///		<para>
		///			For example, if some exception is occurred in server application,
		///			the value of <see cref="Exception.ToString()"/> should specify for <paramref name="debugInformation"/>.
		///			And then, user-friendly, safe message should be specified to <paramref name="message"/> like 'Internal Error."
		///		</para>
		///		<para>
		///			MessagePack-RPC for CLI runtime does not propagate <see cref="DebugInformation"/> for remote endpoint.
		///			So you should specify some error handler to instrument it (e.g. logging handler).
		///		</para>
		/// </remarks>
		public RpcException(RpcError rpcError, string message, string debugInformation, Exception inner)
			: base(message ?? (rpcError ?? RpcError.RemoteRuntimeError).DefaultMessage, inner) {
			this.rpcError = rpcError ?? RpcError.RemoteRuntimeError;
			this.debugInformation = debugInformation;
			RegisterSerializeObjectStateEventHandler();
		}

		internal static T Get<T>(SerializationEntry entry, string name, Func<SerializationEntry, T> getter) {
			Contract.Assert(name != null);
			Contract.Assert(getter != null);

			try {
				return getter(entry);
			}
			catch (InvalidCastException ex) {
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, "Invalid '{0}' value.", name), ex);
			}
		}

		/// <summary>
		///		When overridden on the derived class, handles <see cref="E:Exception.SerializeObjectState"/> event to add type-specified serialization state.
		/// </summary>
		/// <param name="sender">The <see cref="Exception"/> instance itself.</param>
		/// <param name="e">
		///		The <see cref="SafeSerializationEventArgs"/> instance containing the event data.
		///		The overriding method adds its internal state to this object via <see cref="M:SafeSerializationEventArgs.AddSerializedState"/>.
		///	</param>
		///	<remarks>
		///		The overriding method MUST invoke base implementation, or the serialization should fail.
		///	</remarks>
		/// <seealso cref="ISafeSerializationData"/>
		protected virtual void OnSerializeObjectState(object sender, SafeSerializationEventArgs e) {
			e.AddSerializedState(
				new SerializedState() {
					DebugInformation = debugInformation,
					RemoteExceptions = remoteExceptions,
					RpcErrorIdentifier = rpcError.Identifier,
					RpcErrorCode = rpcError.ErrorCode,
					PreservedStackTrace = _preservedStackTrace
				}
			);
		}

		void RegisterSerializeObjectStateEventHandler() {
			SerializeObjectState += OnSerializeObjectState;
		}

		[Serializable]
		sealed class SerializedState : ISafeSerializationData {
			public string DebugInformation;
			public RemoteExceptionInformation[] RemoteExceptions;
			public string RpcErrorIdentifier;
			public int? RpcErrorCode;
			public List<string> PreservedStackTrace;

			public void CompleteDeserialization(object deserialized) {
				var enclosing = deserialized as RpcException;
				enclosing.debugInformation = DebugInformation;
				enclosing.remoteExceptions = RemoteExceptions;
				enclosing.rpcError = RpcError.FromIdentifier(RpcErrorIdentifier, RpcErrorCode);
				enclosing._preservedStackTrace = PreservedStackTrace;
				enclosing.RegisterSerializeObjectStateEventHandler();
			}
		}
	}
}

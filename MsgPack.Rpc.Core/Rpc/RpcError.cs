using MsgPack.Rpc.Core.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Represents pre-defined MsgPack-RPC error metadata.
	/// </summary>
	/// <remarks>
	///		See https://gist.github.com/470667/d33136f74584381bdb58b6444abfcb4a8bbe8abc for details.
	/// </remarks>
	public sealed class RpcError {
		#region -- Built-in Errors --

		static readonly RpcError timeoutError =
			new RpcError(
				"RPCError.TimeoutError",
				-60,
				"Request has been timeout.",
				(error, data) => new RpcTimeoutException(data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the client cannot get response from server.
		///		Details are unknown at all, for instance, message might reach server.
		///		It might be success when the client retry.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for timeout error.
		/// </value>
		public static RpcError TimeoutError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return timeoutError;
			}
		}

		static readonly RpcError transportError =
			new RpcError(
				"RPCError.ClientError.TransportError",
				-50,
				"Cannot initiate transferring message.",
				(error, data) => new RpcTransportException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the client cannot initiate transferring message.
		///		It might be network failure, be configuration issue, or handshake failure.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for general tranport error.
		/// </value>
		public static RpcError TransportError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return transportError;
			}
		}


		static readonly RpcError networkUnreacheableError =
			new RpcError(
				"RPCError.ClientError.TranportError.NetworkUnreacheableError",
				-51,
				"Cannot reach specified remote end point.",
				(error, data) => new RpcTransportException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the client cannot reach specified remote end point.
		///		This error is transport protocol specific.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for network unreacheable error.
		/// </value>
		public static RpcError NetworkUnreacheableError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return networkUnreacheableError;
			}
		}


		static readonly RpcError connectionRefusedError =
			new RpcError(
				"RPCError.ClientError.TranportError.ConnectionRefusedError",
				-52,
				"Connection was refused explicitly by remote end point.",
				(error, data) => new RpcTransportException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the client connection was explicitly refused by the remote end point.
		///		It should fail when you retry.
		///		This error is connection oriented transport protocol specific.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for connection refused error.
		/// </value>
		public static RpcError ConnectionRefusedError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return connectionRefusedError;
			}
		}


		static readonly RpcError connectionTimeoutError =
			new RpcError(
				"RPCError.ClientError.TranportError.ConnectionTimeoutError",
				-53,
				"Connection timout was occurred.",
				(error, data) => new RpcTransportException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when a connection timout was occurred.
		///		It might be success when you retry.
		///		This error is connection oriented transport protocol specific.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for connection timeout error.
		/// </value>
		public static RpcError ConnectionTimeoutError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return connectionTimeoutError;
			}
		}


		static readonly RpcError messageRefusedError =
			new RpcError(
				"RPCError.ClientError.MessageRefusedError",
				-40,
				"Message was refused explicitly by remote end point.",
				(error, data) => new RpcProtocolException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the message was explicitly refused by remote end point.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for message refused error.
		/// </value>
		/// <remarks>
		///		<para>
		///			End point issues this error when:
		///			<list type="bullet">
		///				<item>Couild not deserialize the message.</item>
		///				<item>Message structure of deserialized message was wrong as MessagePack-RPC protocol.</item>
		///				<item>Any value of message was wrong as the protocol.</item>
		///			</list>
		///		</para>
		///		<para>
		///			It may be caused when:
		///			<list type="bullet">
		///				<item>Some deserializing issues were occurred.</item>
		///				<item>Unexpected item type was found as the protocol (e.g. arguments field was not array).</item>
		///				<item>Unexpected item value was found as the protocol (e.g. undefined message type field).</item>
		///			</list>
		///		</para>
		///		<para>
		///			The root cause of this issue might be:
		///			<list type="bullet">
		///				<item>There are some bugs on used library in client or server.</item>
		///				<item>Versions of MessagePack library in client and server were not compatible.</item>
		///				<item>Versions of MessagePack-RPC library in client and server were not compatible.</item>
		///				<item>Packet was changed unexpectedly.</item>
		///			</list>
		///		</para>
		/// </remarks>		
		public static RpcError MessageRefusedError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return messageRefusedError;
			}
		}


		static readonly RpcError messageTooLargeError =
			new RpcError(
				"RPCError.ClientError.MessageRefusedError.MessageTooLargeError",
				-41, "Message is too large.",
				(error, data) => new RpcMessageTooLongException(data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the message was refused explicitly by remote end point due to it was too large.
		///		The structure may be right, but message was simply too large or some portions might be corruptted.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for message too large error.
		/// </value>
		/// <remarks>
		///		<para>
		///			It may be caused when:
		///			<list type="bullet">
		///				<item>Message is too large to be expected by remote end point.</item>
		///			</list>
		///		</para>
		///		<para>
		///			The root cause of this issue might be:
		///			<list type="bullet">
		///				<item>Versions of MessagePack library in client and server were not compatible.</item>
		///				<item>Versions of MessagePack-RPC library in client and server were not compatible.</item>
		///				<item>Packet was changed unexpectedly.</item>
		///				<item>Malicious issuer tried to send invalid message.</item>
		///				<item>Expected value by remote end point was simply too small.</item>
		///			</list>
		///		</para>
		/// </remarks>
		public static RpcError MessageTooLargeError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return messageTooLargeError;
			}
		}


		static readonly RpcError callError =
			new RpcError(
				"RPCError.ClientError.CallError",
				-20,
				"Failed to call specified method.",
				(error, data) => new RpcMethodInvocationException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the RPC runtime failed to call specified method.
		///		The message was certainly reached and the structure was right, but failed to call method.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for call error.
		/// </value>
		public static RpcError CallError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return callError;
			}
		}


		static readonly RpcError noMethodError =
			new RpcError(
				"RPCError.ClientError.CallError.NoMethodError",
				-21,
				"Specified method was not found.",
				(error, data) => new RpcMissingMethodException(data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the specified method was not found.
		///		The message was certainly reached and the structure was right, but failed to call method.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for no method error.
		/// </value>
		public static RpcError NoMethodError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return noMethodError;
			}
		}


		static readonly RpcError argumentError =
			new RpcError(
				"RPCError.ClientError.CallError.ArgumentError",
				-22,
				"Some argument(s) were wrong.",
				(error, data) => new RpcArgumentException(data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the some argument(s) are wrong.
		///		The serialized value might be ill formed or the value is not valid semantically.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for argument error.
		/// </value>
		public static RpcError ArgumentError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return argumentError;
			}
		}


		static readonly RpcError serverError =
			new RpcError(
				"RPCError.ServerError",
				-30,
				"Server cannot process received message.",
				(error, data) => new RpcServerUnavailableException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the server cannot process received message.
		///		Other server might process your request.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for server error.
		/// </value>
		public static RpcError ServerError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return serverError;
			}
		}


		static readonly RpcError serverBusyError =
			new RpcError(
				"RPCError.ServerError.ServerBusyError",
				-31,
				"Server is busy.",
				(error, data) => new RpcServerUnavailableException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when the server is busy.
		///		Other server may process your request.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for server busy error.
		/// </value>
		public static RpcError ServerBusyError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return serverBusyError;
			}
		}


		static readonly RpcError remoteRuntimeError =
			new RpcError(
				"RPCError.RemoteRuntimeError",
				-10,
				"Remote end point failed to process request.",
				(error, data) => new RpcException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for when an internal runtime error is occurred in the remote end point.
		/// </summary>
		/// <value>
		///		The <see cref="RpcError"/> for remote runtime error.
		/// </value>
		public static RpcError RemoteRuntimeError {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return remoteRuntimeError;
			}
		}


		#endregion -- Built-in Errors --

		const string unexpectedErrorIdentifier = "RPCError.RemoteError.UnexpectedError";
		const int unexpectedErrorCode = int.MaxValue;

		static readonly RpcError unexpected =
			new RpcError(
				unexpectedErrorIdentifier,
				unexpectedErrorCode,
				"Unexpected RPC error is occurred.",
				(error, data) => new RpcException(error, data)
			);

		/// <summary>
		///		Gets the <see cref="RpcError"/> for unexpected error.
		/// </summary>
		///	<value>
		///		The <see cref="RpcError"/> for unexpected error.
		///	</value>
		/// <remarks>
		///		The <see cref="RemoteRuntimeError"/> should be used for caught 'unexpected' exception.
		///		This value is for unexpected situation on exception marshaling.
		/// </remarks>
		public static RpcError Unexpected {
			get {
				Contract.Ensures(Contract.Result<RpcError>() != null);
				return unexpected;
			}
		}


		static readonly Dictionary<string, RpcError> identifierDictionary = new Dictionary<string, RpcError>();
		static readonly Dictionary<int, RpcError> errorCodeDictionary = new Dictionary<int, RpcError>();

		static RpcError() {
			foreach (FieldInfo field in
				typeof(RpcError).FindMembers(
					MemberTypes.Field,
					BindingFlags.Static | BindingFlags.NonPublic,
					(member, criteria) => (member as FieldInfo).FieldType.Equals(criteria),
					typeof(RpcError)
				)
			) {
				var builtInError = field.GetValue(null) as RpcError;
				identifierDictionary.Add(builtInError.Identifier, builtInError);
				errorCodeDictionary.Add(builtInError.ErrorCode, builtInError);
			}
		}

		/// <summary>
		///		Get iedntifier of this error.
		/// </summary>
		/// <value>
		///		Iedntifier of this error.
		/// </value>
		public string Identifier { get; }

		/// <summary>
		///		Get error code of this error.
		/// </summary>
		/// <value>
		///		Error code of this error.
		/// </value>
		public int ErrorCode { get; }

		/// <summary>
		///		Get default message in invariant culture.
		/// </summary>
		/// <value>
		///		Default message in invariant culture.
		/// </value>
		/// <remarks>
		///		You can use this property to build custom exception.
		/// </remarks>
		public string DefaultMessageInvariant { get; }

		/// <summary>
		///		Get default message in current UI culture.
		/// </summary>
		/// <value>
		///		Default message in current UI culture.
		/// </value>
		/// <remarks>
		///		You can use this property to build custom exception.
		/// </remarks>
		public string DefaultMessage =>
				// TODO: localization key: Idnentifier ".DefaultMessage"
				DefaultMessageInvariant;

		readonly Func<RpcError, MessagePackObject, RpcException> exceptionUnmarshaler;

		RpcError(string identifier, int errorCode, string defaultMessageInvariant, Func<RpcError, MessagePackObject, RpcException> exceptionUnmarshaler) {
			Identifier = identifier;
			ErrorCode = errorCode;
			DefaultMessageInvariant = defaultMessageInvariant;
			this.exceptionUnmarshaler = exceptionUnmarshaler;
		}

		/// <summary>
		///		Create <see cref="RpcException"/> which corresponds to this error with specified detailed information.
		/// </summary>
		/// <param name="detail">
		///		Detailed error information.
		/// </param>
		/// <returns>
		///		<see cref="RpcException"/> which corresponds to this error with specified detailed information.
		/// </returns>
		internal RpcException ToException(MessagePackObject detail) {
			Contract.Assume(exceptionUnmarshaler != null);

			return exceptionUnmarshaler(this, detail);
		}

		/// <summary>
		///		Determines whether the specified <see cref="object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		///		<c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public sealed override bool Equals(object obj) {
			if (ReferenceEquals(this, obj)) {
				return true;
			}

			if (!(obj is RpcError other)) {
				return false;
			}

			return ErrorCode == other.ErrorCode && Identifier == other.Identifier;
		}

		/// <summary>
		///		Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///		A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public sealed override int GetHashCode() {
			return ErrorCode.GetHashCode();
		}

		/// <summary>
		///		Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		///		A <see cref="string"/> that represents this instance.
		/// </returns>
		public sealed override string ToString() {
			return string.Format(CultureInfo.CurrentCulture, "{0}({1}): {2}", Identifier, ErrorCode, DefaultMessage);
		}

		/// <summary>
		///		Create custom error with specified identifier and error code.
		/// </summary>
		/// <param name="identifier">
		///		Identifier of custom error. This should be "RPCError.&lt;ApplicationName&gt;.&lt;ErrorType&gt;[.&lt;ErrorSubType&gt;]."
		/// </param>
		/// <param name="errorCode">
		///		Error code of custom error. This must be positive or zero.
		/// </param>
		/// <returns>
		///		Custom <see cref="RpcError"/> with specified <paramref name="identifier"/> and <paramref name="errorCode"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="identifier"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="identifier"/> is empty or blank.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<paramref name="errorCode"/> is negative.
		/// </exception>
		public static RpcError CustomError(string identifier, int errorCode) {
			if (identifier == null) {
				throw new ArgumentNullException(nameof(identifier));
			}

			if (string.IsNullOrWhiteSpace(identifier)) {
				throw new ArgumentException("'identifier' cannot be empty.", nameof(identifier));
			}

			if (errorCode < 0) {
				throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Application error code must be grator than or equal to 0.");
			}

			Contract.EndContractBlock();

			return
				new RpcError(
					identifier,
					errorCode,
					"Application throws exception.",
					(error, data) => new RpcFaultException(error, data)
				);
		}

		/// <summary>
		///		Get built-in error with specified identifier and error code, or create custom error when specified identifier and error code is not built-in error.
		/// </summary>
		/// <param name="identifier">
		///		Identifier of error.
		/// </param>
		/// <param name="errorCode">
		///		Error code of error.
		/// </param>
		/// <returns>
		///		Built-in or custom <see cref="RpcError"/> corresponds to <paramref name="identifier"/> or <paramref name="errorCode"/>.
		/// </returns>
		public static RpcError FromIdentifier(string identifier, int? errorCode) {
			if (errorCode != null && errorCodeDictionary.TryGetValue(errorCode.Value, out var result)) {
				return result;
			}

			if (identifier != null && identifierDictionary.TryGetValue(identifier, out result)) {
				return result;
			}

			return CustomError(string.IsNullOrWhiteSpace(identifier) ? unexpectedErrorIdentifier : identifier, errorCode ?? unexpectedErrorCode);
		}

		/// <summary>
		///		Determines whether two <see cref="RpcError"/> instances have the same value. 
		/// </summary>
		/// <param name="left">A <see cref="RpcError"/> instance to compare with <paramref name="right"/>.</param>
		/// <param name="right">A <see cref="RpcError"/> instance to compare with <paramref name="left"/>.</param>
		/// <returns>
		///		<c>true</c> if the <see cref="RpcError"/> instances are equivalent; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator ==(RpcError left, RpcError right) {
			if (left is null) {
				return right is null;
			}
			else {
				return left.Equals(right);
			}
		}

		/// <summary>
		///		Determines whether two <see cref="RpcError"/> instances do not have the same value. 
		/// </summary>
		/// <param name="left">A <see cref="RpcError"/> instance to compare with <paramref name="right"/>.</param>
		/// <param name="right">A <see cref="RpcError"/> instance to compare with <paramref name="left"/>.</param>
		/// <returns>
		///		<c>true</c> if the <see cref="RpcError"/> instances are not equivalent; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator !=(RpcError left, RpcError right) {
			return !(left == right);
		}
	}
}

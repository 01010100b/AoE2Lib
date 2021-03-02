using MsgPack.Rpc.Core.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Thrown if some arguments are wrong like its type was not match, its value was out of range, its value was null but it is not illegal, so on.
	/// </summary>
	[Serializable]
	public sealed class RpcArgumentException : RpcMethodInvocationException {
		const string _parameterNameKey = "ParameterName";
		internal static readonly MessagePackObject ParameterNameKeyUtf8 = MessagePackConvert.EncodeString(_parameterNameKey);

		// NOT readonly for safe deserialization
		string _parameterName;

		/// <summary>
		///		Gets the name of parameter causing this exception.
		/// </summary>
		/// <value>
		///		The mame of parameter causing this exception. This value will not be empty but may be <c>null</c>.
		/// </value>
		public string ParameterName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return _parameterName ?? string.Empty;
			}
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcArgumentException"/> class with the default error message.
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		///	<param name="parameterName">
		///		Name of parameter which is invalid.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is empty or blank.
		/// </exception>
		public RpcArgumentException(string methodName, string parameterName)
			: this(methodName, parameterName, null, null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcArgumentException"/> class with a specified error message.
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		///	<param name="parameterName">
		///		Name of parameter which is invalid.
		///	</param>
		/// <param name="message">
		///		Error message to desribe condition. Note that this message should not include security related information.
		///	</param>
		/// <param name="debugInformation">
		///		Debug information of error.
		///		This value can be null for security reason, and its contents are for developers, not end users.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is empty or blank.
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
		public RpcArgumentException(string methodName, string parameterName, string message, string debugInformation)
			: this(methodName, parameterName, message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcArgumentException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		///	<param name="parameterName">
		///		Name of parameter which is invalid.
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
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> or <paramref name="parameterName"/> is empty or blank.
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
		public RpcArgumentException(string methodName, string parameterName, string message, string debugInformation, Exception inner)
			: base(RpcError.ArgumentError, methodName, message ?? RpcError.ArgumentError.DefaultMessage, debugInformation, inner) {
			if (parameterName == null) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			if (string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentException("'parameterName' cannot be empty.", nameof(parameterName));
			}

			Contract.EndContractBlock();

			_parameterName = parameterName;
		}

		/// <summary>
		///		Initializes a new instance with serialized data. 
		/// </summary>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		internal RpcArgumentException(MessagePackObject unpackedException)
			: base(RpcError.ArgumentError, unpackedException) {
			_parameterName = unpackedException.GetString(ParameterNameKeyUtf8);
			Contract.Assume(_parameterName != null, "Unpacked data does not have ParameterName.");
		}

		/// <summary>
		///		Stores derived type specific information to specified dictionary.
		/// </summary>
		/// <param name="store">
		///		Dictionary to be stored. This value will not be <c>null</c>.
		///	</param>
		/// <param name="includesDebugInformation">
		///		<c>true</c>, when this method should include debug information; otherwise, <c>false</c>.
		///	</param>
		protected sealed override void GetExceptionMessage(IDictionary<MessagePackObject, MessagePackObject> store, bool includesDebugInformation) {
			base.GetExceptionMessage(store, includesDebugInformation);
			store.Add(ParameterNameKeyUtf8, MessagePackConvert.EncodeString(_parameterName));
		}

		/// <summary>
		///		When overridden on the derived class, handles <see cref="E:Exception.SerializeObjectState"/> event to add type-specified serialization state.
		/// </summary>
		/// <param name="sender">The <see cref="Exception"/> instance itself.</param>
		/// <param name="e">
		///		The <see cref="SafeSerializationEventArgs"/> instance containing the event data.
		///		The overriding method adds its internal state to this object via <see cref="M:SafeSerializationEventArgs.AddSerializedState"/>.
		///	</param>
		/// <seealso cref="ISafeSerializationData"/>
		protected override void OnSerializeObjectState(object sender, SafeSerializationEventArgs e) {
			base.OnSerializeObjectState(sender, e);
			e.AddSerializedState(
				new SerializedState() {
					ParameterName = _parameterName
				}
			);
		}

		[Serializable]
		sealed class SerializedState : ISafeSerializationData {
			public string ParameterName;

			public void CompleteDeserialization(object deserialized) {
				var enclosing = deserialized as RpcArgumentException;
				enclosing._parameterName = ParameterName;
			}
		}
	}
}

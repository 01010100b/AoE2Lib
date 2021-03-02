﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Thrown if something wrong during remote method invocation.
	/// </summary>
	[Serializable]
	public class RpcMethodInvocationException : RpcException {
		const string _methodNameKey = "MethodName";
		internal static readonly MessagePackObject MethodNameKeyUtf8 = MessagePackConvert.EncodeString(_methodNameKey);

		// NOT readonly for safe deserialization
		string _methodName;

		/// <summary>
		///		Gets the name of invoking method.
		/// </summary>
		/// <value>
		///		The name of invoking method. This value will not be empty but may be <c>null</c>.
		/// </value>
		public string MethodName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return _methodName;
			}
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMethodInvocationException"/> class with the default error message.
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.CallError"/> is used.
		///	</param>
		///	<param name="methodName">
		///		Name of method which is related to this error.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="methodName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="methodName"/> is empty or blank.
		/// </exception>
		public RpcMethodInvocationException(RpcError rpcError, string methodName) : this(rpcError, methodName, null, null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMethodInvocationException"/> class with a specified error message.
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.CallError"/> is used.
		///	</param>
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
		public RpcMethodInvocationException(RpcError rpcError, string methodName, string message, string debugInformation)
			: this(rpcError, methodName, message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcMethodInvocationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="RpcError.CallError"/> is used.
		///	</param>
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
		public RpcMethodInvocationException(RpcError rpcError, string methodName, string message, string debugInformation, Exception inner)
			: base(rpcError ?? RpcError.CallError, message, debugInformation, inner) {
			if (methodName == null) {
				throw new ArgumentNullException(nameof(methodName));
			}

			if (string.IsNullOrWhiteSpace(methodName)) {
				throw new ArgumentException("'methodName' cannot be empty nor blank.", nameof(methodName));
			}

			Contract.EndContractBlock();

			_methodName = methodName;
		}

		/// <summary>
		///		Initialize new instance with unpacked data.
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
		protected internal RpcMethodInvocationException(RpcError rpcError, MessagePackObject unpackedException)
			: base(rpcError, unpackedException) {
			_methodName = unpackedException.GetString(MethodNameKeyUtf8);
			Contract.Assume(_methodName != null, "Unpacked data does not have MethodName.");
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
		protected override void GetExceptionMessage(IDictionary<MessagePackObject, MessagePackObject> store, bool includesDebugInformation) {
			base.GetExceptionMessage(store, includesDebugInformation);
			store.Add(MethodNameKeyUtf8, MessagePackConvert.EncodeString(_methodName));
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
					MethodName = _methodName
				}
			);
		}

		[Serializable]
		sealed class SerializedState : ISafeSerializationData {
			public string MethodName;

			public void CompleteDeserialization(object deserialized) {
				var enclosing = deserialized as RpcMethodInvocationException;
				enclosing._methodName = MethodName;
			}
		}
	}
}

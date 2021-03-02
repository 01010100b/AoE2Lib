﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Thrown when RPC invocation was time out.
	/// </summary>
	[Serializable]
	public sealed class RpcTimeoutException : RpcException {
		const string _clientTimeoutKey = "ClientTimeout";
		internal static readonly MessagePackObject ClientTimeoutKeyUtf8 = MessagePackConvert.EncodeString(_clientTimeoutKey);

		/// <summary>
		///		Gets the timeout value which was expired in client.
		/// </summary>
		/// <value>The timeout value in client. This value may be <c>null</c> when the server turnes</value>
		public TimeSpan? ClientTimeout { get; private set; }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcTimeoutException"/> class with the default error message.
		/// </summary>
		/// <param name="timeout">Timeout value in client.</param>
		public RpcTimeoutException(TimeSpan timeout) : this(timeout, null, null, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcTimeoutException"/> class with a specified error message.
		/// </summary>
		/// <param name="timeout">Timeout value in client.</param>
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
		public RpcTimeoutException(TimeSpan timeout, string message, string debugInformation)
			: this(timeout, message, debugInformation, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcTimeoutException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="timeout">Timeout value in client.</param>
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
		public RpcTimeoutException(TimeSpan timeout, string message, string debugInformation, Exception inner)
			: base(RpcError.TimeoutError, message, debugInformation, inner) {
			ClientTimeout = timeout;
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="RpcTimeoutException"/> class with the unpacked data.
		/// </summary>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		internal RpcTimeoutException(MessagePackObject unpackedException)
			: base(RpcError.TimeoutError, unpackedException) {
			ClientTimeout = unpackedException.GetTimeSpan(ClientTimeoutKeyUtf8);
			Contract.Assume(ClientTimeout != null, "Unpacked data does not have ClientTimeout.");
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
			store.Add(ClientTimeoutKeyUtf8, ClientTimeout == null ? MessagePackObject.Nil : ClientTimeout.Value.Ticks);
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
					ClientTimeout = ClientTimeout
				}
			);
		}

		[Serializable]
		sealed class SerializedState : ISafeSerializationData {
			public TimeSpan? ClientTimeout;

			public void CompleteDeserialization(object deserialized) {
				var enclosing = deserialized as RpcTimeoutException;
				enclosing.ClientTimeout = ClientTimeout;
			}
		}
	}
}

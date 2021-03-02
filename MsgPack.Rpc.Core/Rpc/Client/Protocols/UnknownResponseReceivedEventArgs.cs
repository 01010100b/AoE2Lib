using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Represents event data of <see cref="E:ClientTransportManager.UnknownResponseReceived"/> event.
	/// </summary>
	public sealed class UnknownResponseReceivedEventArgs : EventArgs {
		/// <summary>
		///		Gets the message ID.
		/// </summary>
		/// <value>
		///		The received message ID.
		/// </value>
		public int? MessageId { get; }

		/// <summary>
		///		Gets the received error.
		/// </summary>
		/// <value>
		///		The received error.
		///		This value will not be <c>null</c>, will be <see cref="RpcErrorMessage.IsSuccess"/> is <c>true</c> when no error.
		/// </value>
		public RpcErrorMessage Error { get; }

		/// <summary>
		///		Gets the received return value.
		/// </summary>
		/// <value>
		///		The received return value.
		///		Note that this value will be <c>null</c> when <see cref="RpcErrorMessage.IsSuccess"/> property of <see cref="Error"/> is <c>true</c>.
		/// </value>
		public MessagePackObject? ReturnValue { get; }

		internal UnknownResponseReceivedEventArgs(int? messageId, RpcErrorMessage error, MessagePackObject? returnValue) {
			MessageId = messageId;
			Error = error;
			ReturnValue = returnValue;
		}
	}
}

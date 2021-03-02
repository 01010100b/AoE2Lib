using System;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Common <see cref="IAsyncResult"/> implementation for MsgPack-RPC async invocation.
	/// </summary>
	internal class MessageAsyncResult : AsyncResult {
		/// <summary>
		///		Gets the ID of message.
		/// </summary>
		/// <value>The ID of message.</value>
		public int? MessageId { get; }

		/// <summary>
		///		Initialize new instance.
		/// </summary>
		/// <param name="owner">
		///		The owner of asynchrnous invocation. This value will not be null.
		/// </param>
		/// <param name="messageId">The ID of message.</param>
		/// <param name="asyncCallback">
		///		The callback of asynchrnous invocation which should be called in completion.
		///		This value can be null.
		/// </param>
		/// <param name="asyncState">
		///		The state object of asynchrnous invocation which will be passed to <see cref="AsyncCallback"/>.
		///		This value can be null.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="owner"/> is null.
		/// </exception>
		public MessageAsyncResult(object owner, int? messageId, AsyncCallback asyncCallback, object asyncState)
			: base(owner, asyncCallback, asyncState) {
			MessageId = messageId;
		}
	}
}

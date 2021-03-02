using System;

namespace MsgPack.Rpc.Core.Client {
	internal sealed class NotificationMessageAsyncResult : MessageAsyncResult {
		public void OnCompleted(Exception exception, bool completedSynchronously) {
			if (exception != null) {
				OnError(exception, completedSynchronously);
			}
			else {
				Complete(completedSynchronously);
			}

			AsyncCallback?.Invoke(this);
		}

		/// <summary>
		///		Initialize new instance.
		/// </summary>
		/// <param name="owner">
		///		Owner of asynchrnous invocation. This value will not be null.
		/// </param>
		/// <param name="asyncCallback">
		///		Callback of asynchrnous invocation which should be called in completion.
		///		This value can be null.
		/// </param>
		/// <param name="asyncState">
		///		State object of asynchrnous invocation which will be passed to <see cref="AsyncCallback"/>.
		///		This value can be null.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="owner"/> is null.
		/// </exception>
		public NotificationMessageAsyncResult(object owner, AsyncCallback asyncCallback, object asyncState)
			: base(owner, null, asyncCallback, asyncState) { }
	}
}

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Minimal implementation of <see cref="IAsyncResult"/>.
	/// </summary>
	internal class AsyncResult : IAsyncResult {
		// State flags
		const int _initialized = 0;
		const int _completed = 0x100;
		const int _completedSynchronously = 0x101;
		const int _finished = 0x2;
		const int _neverSet = unchecked((int)0x80000000);

		/// <summary>
		///		Gets an owner of asynchrnous invocation.
		/// </summary>
		/// <value>
		///		An owner of asynchrnous invocation. This value will not be null.
		/// </value>
		internal object Owner { get; }

		/// <summary>
		///		Gets a callback of asynchrnous invocation which should be called in completion.
		/// </summary>
		/// <value>
		///		A callback of asynchrnous invocation which should be called in completion.
		///		This value could be null.
		/// </value>
		public AsyncCallback AsyncCallback { get; }

		/// <summary>
		///		Gets a state object of asynchrnous invocation which will be passed to <see cref="AsyncCallback"/>.
		/// </summary>
		/// <value>
		///		A state object of asynchrnous invocation which will be passed to <see cref="AsyncCallback"/>.
		///		This value could be null.
		/// </value>
		public object AsyncState { get; }

		ManualResetEvent _asyncWaitHandle;

		/// <summary>
		///		Gets a <see cref="WaitHandle"/> to be used coordinate multiple asynchronous invocation.
		/// </summary>
		/// <value>
		///		A <see cref="WaitHandle"/> to be used coordinate multiple asynchronous invocation.
		/// </value>
		public WaitHandle AsyncWaitHandle {
			get {
				var waitHandle = LazyInitializer.EnsureInitialized(ref _asyncWaitHandle, () => new ManualResetEvent(false));
				if (IsCompleted) {
					waitHandle.Set();
				}

				return waitHandle;
			}
		}

		// manipulated via Interlocked methods.
		int _state;

		bool IAsyncResult.CompletedSynchronously => (_state & _completedSynchronously) == _completedSynchronously;

		/// <summary>
		///		Gets a value asynchronous invocation is completed.
		/// </summary>
		/// <value>
		///		If asynchronous invocation is completed, that is, BeginInvoke is finished then true.
		/// </value>
		public bool IsCompleted => (_state & _completed) == _completed;

		/// <summary>
		///		Gets a value asynchronous invocation is finished.
		/// </summary>
		/// <value>
		///		If asynchronous invocation is finished, that is, EncInvoke is finished then true.
		/// </value>
		public bool IsFinished => (_state & _finished) == _finished;

		Exception _error;

		/// <summary>
		///		Gets an error corresponds to this message.
		/// </summary>
		/// <value>
		///		An error corresponds to this message.
		/// </value>
		public Exception Error => _error;

		/// <summary>
		///		Initializes new instance.
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
		protected AsyncResult(object owner, AsyncCallback asyncCallback, object asyncState) {
			Owner = owner ?? throw new ArgumentNullException(nameof(owner));
			AsyncCallback = asyncCallback;
			AsyncState = asyncState;
		}

		/// <summary>
		///		Records asynchronous invocation result and set completion.
		/// </summary>
		/// <param name="completedSynchronously">
		///		When operation is completed same thread as initiater then <c>true</c>; otherwise, <c>false</c>.
		/// </param>
		internal void Complete(bool completedSynchronously) {
			var state = _completed | (completedSynchronously ? _completedSynchronously : 0);
			if (Interlocked.CompareExchange(ref _state, state, _initialized) == _initialized) {
				var waitHandle = _asyncWaitHandle;
				if (waitHandle != null) {
					waitHandle.Set();
				}
			}
		}

		/// <summary>
		///		Completes this invocation as error.
		/// </summary>
		/// <param name="error">
		///		Occurred exception.
		///	</param>
		/// <param name="completedSynchronously">
		///		When operation is completed same thread as initiater then <c>true</c>; otherwise, <c>false</c>.
		/// </param>
		public void OnError(Exception error, bool completedSynchronously) {
			try { }
			finally {
				Interlocked.Exchange(ref _error, error);
				Complete(completedSynchronously);
			}
		}

		/// <summary>
		///		Waits until asynchronous operation is completed.
		/// </summary>
		public void WaitForCompletion() {
			var current = Interlocked.CompareExchange(ref _state, _neverSet, _neverSet);
			if ((current & _completed) == 0) {
				AsyncWaitHandle.WaitOne();
			}
		}

		/// <summary>
		///		Records all operation is finished and clean ups internal resources.
		/// </summary>
		public void Finish() {
			Contract.Assert(_state != _initialized);
			try {
				var oldValue = _state;
				var newValue = _state | _finished;
				while (Interlocked.CompareExchange(ref _state, newValue, oldValue) != oldValue) {
					oldValue = _state;
					newValue = oldValue | _finished;
				}

				if (_error != null) {
					throw _error;
				}
			}
			finally {
				var waitHandle = Interlocked.Exchange(ref _asyncWaitHandle, null);
				if (waitHandle != null) {
					waitHandle.Dispose();
				}
			}
		}

		/// <summary>
		///		Verifies ownership and return typed instance.
		/// </summary>
		/// <typeparam name="TAsyncResult">Type of returning <paramref name="asyncResult"/>.</typeparam>
		/// <param name="asyncResult"><see cref="IAsyncResult"/> passed to EndInvoke.</param>
		/// <param name="owner">'this' reference of EndInvoke to be verified.</param>
		/// <returns>Verified <paramref name="asyncResult"/>.</returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="asyncResult"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		<paramref name="asyncResult"/> is not <typeparamref name="TAsyncResult"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///		<paramref name="owner"/> is not same as <see cref="Owner"/>.
		///		Or <see cref="IsFinished"/> is true.
		/// </exception>
		internal static TAsyncResult Verify<TAsyncResult>(IAsyncResult asyncResult, object owner)
			where TAsyncResult : AsyncResult {
			Contract.Assert(owner != null);
			if (asyncResult == null) {
				throw new ArgumentNullException(nameof(asyncResult));
			}

			if (!(asyncResult is TAsyncResult result)) {
				throw new ArgumentException("Unknown asyncResult.", nameof(asyncResult));
			}

			if (!ReferenceEquals(result.Owner, owner)) {
				throw new InvalidOperationException("Async operation was not started on this instance.");
			}

			if (result.IsFinished) {
				throw new InvalidOperationException("Async operation has already been finished.");
			}

			return result;
		}
	}
}

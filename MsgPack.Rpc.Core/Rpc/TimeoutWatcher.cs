using System;
using System.Threading;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Watches timeout.
	/// </summary>
	internal class TimeoutWatcher : IDisposable {
		const int StateIdle = 0;
		const int StateWatching = 1;
		const int StateTimeout = 2;
		const int StateDisposed = 3;

		readonly object _resourceLock = new object();
		ManualResetEvent _waitHandle;
		RegisteredWaitHandle _registeredWaitHandle;
		int _state;

		/// <summary>
		///		Gets a value indicating whether timeout is occurred.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if timeout is occurred; otherwise, <c>false</c>.
		/// </value>
		public bool IsTimeout {
			get {
				VerifyIsNotDisposed();

				return Interlocked.CompareExchange(ref _state, 0, 0) == StateTimeout;
			}
		}

		EventHandler _timeout;

		/// <summary>
		///		Occurs when operation timeout.
		/// </summary>
		public event EventHandler Timeout {
			add {
				VerifyIsNotDisposed();

				EventHandler oldHandler;
				var currentHandler = _timeout;
				do {
					oldHandler = currentHandler;
					var newHandler = Delegate.Combine(oldHandler, value) as EventHandler;
					currentHandler = Interlocked.CompareExchange(ref _timeout, newHandler, oldHandler);
				} while (oldHandler != currentHandler);
			}
			remove {
				VerifyIsNotDisposed();

				EventHandler oldHandler;
				var currentHandler = _timeout;
				do {
					oldHandler = currentHandler;
					var newHandler = Delegate.Remove(oldHandler, value) as EventHandler;
					currentHandler = Interlocked.CompareExchange(ref _timeout, newHandler, oldHandler);
				} while (oldHandler != currentHandler);
			}
		}

		void OnTimeout() {
			Interlocked.CompareExchange(ref _timeout, null, null)?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutWatcher"/> class.
		/// </summary>
		public TimeoutWatcher() {
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			if (Interlocked.Exchange(ref _state, StateDisposed) != StateDisposed) {
				lock (_resourceLock) {
					if (_waitHandle != null) {
						if (_registeredWaitHandle != null) {
							_registeredWaitHandle.Unregister(_waitHandle);
							_registeredWaitHandle = null;
						}

						_waitHandle.Close();
					}
				}
			}
		}

		void VerifyIsNotDisposed() {
			if (Interlocked.CompareExchange(ref _state, 0, 0) == StateDisposed) {
				throw new ObjectDisposedException(ToString());
			}
		}

		/// <summary>
		///		Resets this instance.
		/// </summary>
		public void Reset() {
			lock (_resourceLock) {
				VerifyIsNotDisposed();

				if (_waitHandle != null) {
					if (_registeredWaitHandle != null) {
						_registeredWaitHandle.Unregister(_waitHandle);
						_registeredWaitHandle = null;
					}

					// It is unstable to reuse wait handles...
					_waitHandle.Close();
					_waitHandle = null;
				}

				Interlocked.Exchange(ref _state, StateIdle);
			}
		}

		/// <summary>
		///		Starts timeout watch.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		/// <exception cref="InvalidOperationException">
		///		This instance already start wathing.
		/// </exception>
		public void Start(TimeSpan timeout) {
			lock (_resourceLock) {
				VerifyIsNotDisposed();

				if (_registeredWaitHandle != null) {
					throw new InvalidOperationException("Already started.");
				}

				if (_waitHandle == null) {
					_waitHandle = new ManualResetEvent(false);
				}

				_registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_waitHandle, OnPulse, null, timeout, true);
				Interlocked.Exchange(ref _state, StateWatching);
			}
		}

		void OnPulse(object state, bool isTimeout) {
			if (isTimeout && Interlocked.CompareExchange(ref _state, StateTimeout, StateWatching) == StateWatching) {
				OnTimeout();
			}
		}

		/// <summary>
		///		Stops timeout watch.
		/// </summary>
		public void Stop() {
			lock (_resourceLock) {
				VerifyIsNotDisposed();

				// Do not override Disposed/Timeout
				Interlocked.CompareExchange(ref _state, StateIdle, StateWatching);

				if (_waitHandle != null) {
					_waitHandle.Set();
				}
			}
		}
	}
}

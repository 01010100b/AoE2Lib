using System;
using System.Security;
using System.Threading;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Provides access to RPC server application context information.
	/// </summary>
	public sealed class RpcApplicationContext : IDisposable {
		internal static readonly object HardTimeoutToken = new object();
		const int StateActive = 0;
		const int StateSoftTimeout = 1;
		const int StateHardTimeout = 2;
		const int StateDisposed = 3;

		[ThreadStatic]
		static RpcApplicationContext _current;

		/// <summary>
		///		Gets the current context.
		/// </summary>
		/// <value>
		///		The current context.
		///		If this thread is initiated by the dispatcher, then <c>null</c>.
		/// </value>
		public static RpcApplicationContext Current => _current;

		/// <summary>
		///		Sets the current context for this thread.
		/// </summary>
		/// <param name="context">The context.</param>
		internal static void SetCurrent(RpcApplicationContext context) {
			_current = context;
			context._boundThread = new WeakReference(Thread.CurrentThread);
			context._hardTimeoutWatcher.Reset();
			context._softTimeoutWatcher.Reset();
		}

		/// <summary>
		///		Clears current instance.
		/// </summary>
		internal static void Clear() {
			var current = _current;
			if (current != null) {
				try {
					current.StopTimeoutWatch();
				}
				finally {
					current._boundThread = null;
					Interlocked.Exchange(ref current._state, StateActive);
					_current = null;
				}
			}
		}

		/// <summary>
		///		Gets a value indicating whether this application thread is canceled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this pplication thread is canceled; otherwise, <c>false</c>.
		/// 	Note that if <see cref="Current"/> returns <c>null</c>, then this property returns <c>false</c>.
		/// </value>
		public static bool IsCanceled {
			get {
				var current = Current;
				return current != null && current.CancellationToken.IsCancellationRequested;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the execution timeout is enabled on this application thread.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the execution timeout is enabled on this application thread; otherwise, <c>false</c>.
		/// 	Note that if <see cref="Current"/> returns <c>null</c>, then this property returns <c>false</c>.
		/// </value>
		public static bool IsExecutionTimeoutEnabled {
			get {
				var current = Current;
				return current != null && current._softTimeout != null;
			}
		}

		internal bool IsSoftTimeout => _softTimeoutWatcher.IsTimeout;

		// Set from SetCurrent
		WeakReference _boundThread;
		AggregateException _exceptionInCancellationCallback;
		readonly TimeoutWatcher _softTimeoutWatcher;
		readonly TimeoutWatcher _hardTimeoutWatcher;
		readonly CancellationTokenSource _cancellationTokenSource;

#if DEBUG
		[Obsolete("DO NOT use this member except testing purposes.")]
		internal event EventHandler DebugSoftTimeout;

		void OnDebugSoftTimeout() {
			DebugSoftTimeout?.Invoke(this, EventArgs.Empty);
		}
#endif

		/// <summary>
		///		Gets the <see cref="CancellationToken"/> associated with this context.
		/// </summary>
		/// <value>
		///		The <see cref="CancellationToken"/> associated with this context.
		/// </value>
		public CancellationToken CancellationToken => _cancellationTokenSource.Token;

		TimeSpan? _softTimeout;

		TimeSpan? _hardTimeout;

		int _state;

		internal bool IsDisposed => Interlocked.CompareExchange(ref _state, 0, 0) == StateDisposed;

		// called from SetCurrent
		internal RpcApplicationContext(TimeSpan? softTimeout, TimeSpan? hardTimeout) {
			_softTimeout = softTimeout;
			_hardTimeout = hardTimeout;
			_softTimeoutWatcher = new TimeoutWatcher();
			_softTimeoutWatcher.Timeout += (sender, e) => OnSoftTimeout();
			_hardTimeoutWatcher = new TimeoutWatcher();
			_hardTimeoutWatcher.Timeout += (sender, e) => OnHardTimeout();
			_cancellationTokenSource = new CancellationTokenSource();
		}

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			if (Interlocked.Exchange(ref _state, StateDisposed) != StateDisposed) {
				_hardTimeoutWatcher.Dispose();
				_softTimeoutWatcher.Dispose();
				_cancellationTokenSource.Dispose();
			}
		}

		void OnSoftTimeout() {
			if (Interlocked.CompareExchange(ref _state, StateSoftTimeout, StateActive) != StateActive) {
				return;
			}

			try {
				_cancellationTokenSource.Cancel();
			}
			catch (AggregateException ex) {
				Interlocked.Exchange(ref _exceptionInCancellationCallback, ex);
			}

			if (_hardTimeout != null) {
				_hardTimeoutWatcher.Start(_hardTimeout.Value);
			}

#if DEBUG
			OnDebugSoftTimeout();
#endif
		}

		void OnHardTimeout() {
			if (Interlocked.CompareExchange(ref _state, StateHardTimeout, StateSoftTimeout) != StateSoftTimeout) {
				return;
			}

			try {
				DoHardTimeout();
			}
			catch (SecurityException) { }
			catch (MemberAccessException) { }
		}

		[SecuritySafeCritical]
		void DoHardTimeout() {
			if (_boundThread.Target is Thread thread) {
				try {
					thread.Abort(HardTimeoutToken);
				}
				catch (ThreadStateException) { }
			}
		}

		internal void StartTimeoutWatch() {
			if (_softTimeout == null) {
				return;
			}

			_softTimeoutWatcher.Start(_softTimeout.Value);
		}

		internal void StopTimeoutWatch() {
			_hardTimeoutWatcher.Stop();
			_softTimeoutWatcher.Stop();

			var exceptionInCancellationCallback = Interlocked.Exchange(ref _exceptionInCancellationCallback, null);
			if (exceptionInCancellationCallback != null) {
				throw exceptionInCancellationCallback;
			}
		}
	}
}

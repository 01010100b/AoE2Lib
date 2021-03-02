using MsgPack.Rpc.Core.StandardObjectPoolTracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Implements standard <see cref="ObjectPool{T}"/>.
	/// </summary>
	/// <typeparam name="T">
	///		The type of objects to be pooled.
	/// </typeparam>
	internal sealed class StandardObjectPool<T> : ObjectPool<T>
		where T : class {
		static readonly bool isDisposableTInternal = typeof(IDisposable).IsAssignableFrom(typeof(T));

		// name for debugging purpose, explicitly specified, or automatically constructed.
		readonly string name;

		internal TraceSource TraceSource { get; }

		readonly ObjectPoolConfiguration configuration;

		int isCorrupted;
		bool IsCorrupted => Interlocked.CompareExchange(ref isCorrupted, 0, 0) != 0;

		readonly Func<T> factory;
		readonly BlockingCollection<T> pool;
		readonly TimeSpan borrowTimeout;

		// Debug
		internal int PooledCount => pool.Count;

		readonly BlockingCollection<WeakReference> leases;
		readonly ReaderWriterLockSlim leasesLock;

		internal int LeasedCount => leases.Count;

		// TODO: Timer might be too heavy.
		readonly Timer evictionTimer;
		readonly int? evictionIntervalMilliseconds;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardObjectPool&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="factory">
		///		The factory delegate to create <typeparamref name="T"/> type instance.
		///	</param>
		/// <param name="configuration">
		///		The <see cref="ObjectPoolConfiguration"/> which contains various settings of this object pool.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="factory"/> is <c>null</c>.
		/// </exception>
		public StandardObjectPool(Func<T> factory, ObjectPoolConfiguration configuration) {
			Contract.EndContractBlock();

			var safeConfiguration = (configuration ?? ObjectPoolConfiguration.Default).AsFrozen();

			if (string.IsNullOrWhiteSpace(safeConfiguration.Name)) {
				TraceSource = new TraceSource(GetType().FullName);
				name = GetType().FullName + "@" + GetHashCode().ToString("X", CultureInfo.InvariantCulture);
			}
			else {
				TraceSource = new TraceSource(safeConfiguration.Name);
				name = safeConfiguration.Name;
			}

			if (configuration == null && TraceSource.ShouldTrace(StandardObjectPoolTrace.InitializedWithDefaultConfiguration)) {
				TraceSource.TraceEvent(
					StandardObjectPoolTrace.InitializedWithDefaultConfiguration,
					"Initialized with default configuration. { \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }",
					name,
					GetType(),
					GetHashCode()
				);
			}
			else if (TraceSource.ShouldTrace(StandardObjectPoolTrace.InitializedWithConfiguration)) {
				TraceSource.TraceEvent(
					StandardObjectPoolTrace.InitializedWithConfiguration,
					"Initialized with specified configuration. { \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Configuration\" : {3} }",
					name,
					GetType(),
					GetHashCode(),
					configuration
				);
			}

			this.configuration = safeConfiguration;
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			leasesLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			borrowTimeout = safeConfiguration.BorrowTimeout ?? TimeSpan.FromMilliseconds(Timeout.Infinite);
			pool =
				new BlockingCollection<T>(
					new ConcurrentStack<T>()
				);

			if (safeConfiguration.MaximumPooled == null) {
				leases = new BlockingCollection<WeakReference>(new ConcurrentQueue<WeakReference>());
			}
			else {
				leases = new BlockingCollection<WeakReference>(new ConcurrentQueue<WeakReference>(), safeConfiguration.MaximumPooled.Value);
			}

			for (var i = 0; i < safeConfiguration.MinimumReserved; i++) {
				if (!AddToPool(factory(), 0)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.FailedToAddPoolInitially,
						"Failed to add item. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }}",
						name,
						GetType(),
						GetHashCode()
					);
				}
			}

			evictionIntervalMilliseconds = safeConfiguration.EvitionInterval == null ? default(int?) : unchecked((int)safeConfiguration.EvitionInterval.Value.TotalMilliseconds);

			if (safeConfiguration.MaximumPooled != null
						 && safeConfiguration.MinimumReserved != safeConfiguration.MaximumPooled.GetValueOrDefault()
						 && evictionIntervalMilliseconds != null) {
				evictionTimer = new Timer(OnEvictionTimerElapsed, null, evictionIntervalMilliseconds.Value, Timeout.Infinite);
			}
			else {
				evictionTimer = null;
			}
		}

		bool AddToPool(T value, int millisecondsTimeout) {
			var result = false;
			try { }
			finally {
				if (pool.TryAdd(value, millisecondsTimeout)) {
					if (leases.TryAdd(new WeakReference(value))) {
						result = true;
					}
				}
			}

			return result;
		}

		protected sealed override void Dispose(bool disposing) {
			if (disposing) {
				pool.Dispose();

				if (evictionTimer != null) {
					evictionTimer.Dispose();
				}

				leases.Dispose();
				leasesLock.Dispose();

				if (TraceSource.ShouldTrace(StandardObjectPoolTrace.Disposed)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.Disposed,
						"Object pool is disposed. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }}",
						name,
						GetType(),
						GetHashCode()
					);
				}
			}
			else {
				if (TraceSource.ShouldTrace(StandardObjectPoolTrace.Finalized)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.Finalized,
						"Object pool is finalized. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }}",
						name,
						GetType(),
						GetHashCode()
					);
				}
			}

			base.Dispose(disposing);
		}

		void VerifyIsNotCorrupted() {
			if (IsCorrupted) {
				throw new ObjectPoolCorruptedException();
			}
		}

		void SetIsCorrupted() {
			Interlocked.Exchange(ref isCorrupted, 1);
		}

		void OnEvictionTimerElapsed(object state) {
			EvictExtraItemsCore(false);

			Contract.Assert(evictionIntervalMilliseconds.HasValue);

			if (IsCorrupted) {
				return;
			}

			if (!evictionTimer.Change(evictionIntervalMilliseconds.Value, Timeout.Infinite)) {
				TraceSource.TraceEvent(
					StandardObjectPoolTrace.FailedToRefreshEvictionTImer,
					"Failed to refresh evition timer. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }}",
					name,
					GetType(),
					GetHashCode()
				);
			}
		}

		/// <summary>
		///		Evicts the extra items from current pool.
		/// </summary>
		public sealed override void EvictExtraItems() {
			EvictExtraItemsCore(true);
		}

		void EvictExtraItemsCore(bool isInduced) {
			var remains = pool.Count - configuration.MinimumReserved;
			var evicting = remains / 2 + remains % 2;
			if (evicting > 0) {
				if (isInduced && TraceSource.ShouldTrace(StandardObjectPoolTrace.EvictingExtraItemsInduced)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.EvictingExtraItemsInduced,
						"Start induced eviction. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Evicting\" : {3} }}",
						name,
						GetType(),
						GetHashCode(),
						evicting
					);
				}
				else if (TraceSource.ShouldTrace(StandardObjectPoolTrace.EvictingExtraItemsPreiodic)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.EvictingExtraItemsPreiodic,
						"Start periodic eviction. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Evicting\" : {3} }}",
						name,
						GetType(),
						GetHashCode(),
						evicting
					);
				}

				var disposed = EvictItems(evicting);

				if (isInduced && TraceSource.ShouldTrace(StandardObjectPoolTrace.EvictedExtraItemsInduced)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.EvictedExtraItemsInduced,
						"Finish induced eviction. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Evicted\" : {3} }}",
						name,
						GetType(),
						GetHashCode(),
						disposed.Count
					);
				}
				else if (TraceSource.ShouldTrace(StandardObjectPoolTrace.EvictedExtraItemsPreiodic)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.EvictedExtraItemsPreiodic,
						"Finish periodic eviction. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Evicted\" : {3} }}",
						name,
						GetType(),
						GetHashCode(),
						disposed.Count
					);
				}

				CollectLeases(disposed);
			}
			else {
				// Just GC
				CollectLeases(new List<T>(0));
			}
		}

		List<T> EvictItems(int count) {
			var disposed = new List<T>(count);
			for (var i = 0; i < count; i++) {
				if (!pool.TryTake(out var disposing, 0)) {
					// Race, cancel eviction now.
					return disposed;
				}

				DisposeItem(disposing);
				disposed.Add(disposing);
			}

			return disposed;
		}

		void CollectLeases(List<T> disposed) {
			var isSuccess = false;
			try {
				leasesLock.EnterWriteLock();
				try {
					var buffer = new List<WeakReference>(leases.Count + Environment.ProcessorCount * 2);

					while (leases.TryTake(out var dequeud)) {
						buffer.Add(dequeud);
					}

					var isFlushed = false;
					var freed = 0;
					foreach (var item in buffer) {
						if (!isFlushed && item.IsAlive && !disposed.Exists(x => ReferenceEquals(x, SafeGetTarget(item)))) {
							if (!leases.TryAdd(item)) {
								// Just evict
								isFlushed = true;
								freed++;
							}
						}
						else {
							freed++;
						}
					}

					if (freed - disposed.Count > 0 && TraceSource.ShouldTrace(StandardObjectPoolTrace.GarbageCollectedWithLost)) {
						TraceSource.TraceEvent(
							StandardObjectPoolTrace.GarbageCollectedWithLost,
							"Garbage items are collected, but there may be lost items. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Collected\" : {3}, \"MayBeLost\" : {4} }}",
							name,
							GetType(),
							GetHashCode(),
							freed,
							freed - disposed.Count
						);
					}
					else if (freed > 0 && TraceSource.ShouldTrace(StandardObjectPoolTrace.GarbageCollectedWithoutLost)) {
						TraceSource.TraceEvent(
							StandardObjectPoolTrace.GarbageCollectedWithoutLost,
							"Garbage items are collected. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Collected\" : {3} }}",
							name,
							GetType(),
							GetHashCode(),
							freed
						);
					}
				}
				finally {
					leasesLock.ExitWriteLock();
				}

				isSuccess = true;
			}
			finally {
				if (!isSuccess) {
					SetIsCorrupted();
				}
			}
		}

		static T SafeGetTarget(WeakReference item) {
			try {
				return item.Target as T;
			}
			catch (InvalidOperationException) {
				return null;
			}
		}

		protected sealed override T BorrowCore() {
			VerifyIsNotCorrupted();

			T result;
			while (true) {
				if (pool.TryTake(out result, 0)) {
					if (TraceSource.ShouldTrace(StandardObjectPoolTrace.BorrowFromPool)) {
						TraceBorrow(result);
					}

					return result;
				}

				leasesLock.EnterReadLock(); // TODO: Timeout
				try {
					if (leases.Count < leases.BoundedCapacity) {
						var newObject = factory();
						Contract.Assume(newObject != null);

						if (leases.TryAdd(new WeakReference(newObject), 0)) {
							if (TraceSource.ShouldTrace(StandardObjectPoolTrace.ExpandPool)) {
								TraceSource.TraceEvent(
									StandardObjectPoolTrace.ExpandPool,
									"Expand the pool. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"NewCount\" : {3} }}",
									name,
									GetType(),
									GetHashCode(),
									pool.Count
								);
							}

							TraceBorrow(newObject);
							return newObject;
						}
						else {
							if (TraceSource.ShouldTrace(StandardObjectPoolTrace.FailedToExpandPool)) {
								TraceSource.TraceEvent(
									StandardObjectPoolTrace.FailedToExpandPool,
									"Failed to expand the pool. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"NewCount\" : {3} }}",
									name,
									GetType(),
									GetHashCode(),
									pool.Count
								);
							}

							DisposeItem(newObject);
						}
					}
				}
				finally {
					leasesLock.ExitReadLock();
				}

				// Wait or exception
				break;
			}

			if (TraceSource.ShouldTrace(StandardObjectPoolTrace.PoolIsEmpty)) {
				TraceSource.TraceEvent(
					StandardObjectPoolTrace.PoolIsEmpty,
					"Pool is empty. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X} }}",
					name,
					GetType(),
					GetHashCode()
				);
			}

			if (configuration.ExhausionPolicy == ExhausionPolicy.ThrowException) {
				throw new ObjectPoolEmptyException();
			}
			else {
				if (!pool.TryTake(out result, borrowTimeout)) {
					throw new TimeoutException(string.Format(CultureInfo.CurrentCulture, "The object borrowing is not completed in the time out {0}.", borrowTimeout));
				}

				if (TraceSource.ShouldTrace(StandardObjectPoolTrace.BorrowFromPool)) {
					TraceBorrow(result);
				}

				return result;
			}
		}

		void TraceBorrow(T result) {
			TraceSource.TraceEvent(
				StandardObjectPoolTrace.BorrowFromPool,
				"Borrow the value from the pool. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"HashCode\" : 0x{2:X}, \"Evicted\" : 0x{2:X}, \"Resource\" : \"{3}\", \"HashCodeOfResource\" : 0x{4:X} }}",
				name,
				GetType(),
				GetHashCode(),
				result,
				result.GetHashCode()
			);
		}

		static void DisposeItem(T item) {
			if (isDisposableTInternal) {
				((IDisposable)item).Dispose();
			}
		}

		protected sealed override void ReturnCore(T value) {
			if (!pool.TryAdd(value)) {
				TraceSource.TraceEvent(
					StandardObjectPoolTrace.FailedToReturnToPool,
					"Failed to return the value to the pool. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"Value\" : 0x{2:X}, \"Resource\" : \"{3}\", \"HashCodeOfResource\" : 0x{4:X} }}",
					name,
					GetType(),
					GetHashCode(),
					value,
					value.GetHashCode()
				);
				SetIsCorrupted();
				throw new ObjectPoolCorruptedException("Failed to return the value to the pool.");
			}
			else {
				if (TraceSource.ShouldTrace(StandardObjectPoolTrace.ReturnToPool)) {
					TraceSource.TraceEvent(
						StandardObjectPoolTrace.ReturnToPool,
						"Return the value to the pool. {{ \"Name\" : \"{0}\", \"Type\" : \"{1}\", \"Value\" : 0x{2:X}, \"Resource\" : \"{3}\", \"HashCodeOfResource\" : 0x{4:X} }}",
						name,
						GetType(),
						GetHashCode(),
						value,
						value.GetHashCode()
					);
				}
			}
		}
	}
}

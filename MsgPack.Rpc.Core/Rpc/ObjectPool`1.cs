using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Defines common interfaces and basic features of the object pool.
	/// </summary>
	/// <typeparam name="T">
	///		The type of objects to be pooled.
	/// </typeparam>
	[ContractClass(typeof(ObjectPoolContracts<>))]
	public abstract class ObjectPool<T> : IDisposable
		where T : class {
		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPool&lt;T&gt;"/> class.
		/// </summary>
		protected ObjectPool() { }

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			// nop
		}

		/// <summary>
		///		Evicts the extra items from current pool.
		/// </summary>
		public virtual void EvictExtraItems() { }

		/// <summary>
		///		Borrows the item from this pool.
		/// </summary>
		/// <returns>
		///		The item borrowed.
		///		This value will not be <c>null</c>.
		/// </returns>
		public T Borrow() {
			Contract.Ensures(Contract.Result<T>() != null);

			return BorrowCore();
		}

		/// <summary>
		///		Borrows the item from this pool.
		/// </summary>
		/// <returns>
		///		The item borrowed.
		///		This value cannot be <c>null</c>.
		/// </returns>
		protected abstract T BorrowCore();

		/// <summary>
		///		Returns the specified borrowed item.
		/// </summary>
		/// <param name="value">The borrowed item.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="value"/> is <c>null</c>.
		/// </exception>
		public void Return(T value) {
			if (value == null) {
				throw new ArgumentNullException(nameof(value));
			}

			Contract.EndContractBlock();

			ReturnCore(value);
		}

		/// <summary>
		///		Returns the specified borrowed item.
		/// </summary>
		/// <param name="value">The borrowed item. This value will not be <c>null</c>.</param>
		protected abstract void ReturnCore(T value);
	}

	[ContractClassFor(typeof(ObjectPool<>))]
	internal abstract class ObjectPoolContracts<T> : ObjectPool<T>
		where T : class {
		ObjectPoolContracts() { }

		protected sealed override T BorrowCore() {
			Contract.Ensures(Contract.Result<T>() != null);
			return default;
		}

		protected sealed override void ReturnCore(T value) {
			Contract.Requires(value != null);
		}
	}
}

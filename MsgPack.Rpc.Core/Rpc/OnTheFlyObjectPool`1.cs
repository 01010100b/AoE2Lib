using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		The dummy implementation of the <see cref="ObjectPool{T}"/> for mainly testing purposes.
	/// </summary>
	/// <typeparam name="T">
	///		The type of objects to be pooled.
	/// </typeparam>
	/// <remarks>
	///		This object actually does not pool any objects, simply creates and returns <typeparamref name="T"/> type instances.
	/// </remarks>
	public sealed class OnTheFlyObjectPool<T> : ObjectPool<T>
		where T : class {
		readonly Func<ObjectPoolConfiguration, T> _factory;
		readonly ObjectPoolConfiguration _configuration;

		/// <summary>
		///		Initializes a new instance of the <see cref="OnTheFlyObjectPool&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="factory">
		///		The factory delegate to create <typeparamref name="T"/> type instance using <see cref="ObjectPoolConfiguration"/>.
		///	</param>
		/// <param name="configuration">
		///		The <see cref="ObjectPoolConfiguration"/> which contains various settings of this object pool.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="factory"/> is <c>null</c>.
		///		Or <paramref name="configuration"/> is <c>null</c>.
		/// </exception>
		public OnTheFlyObjectPool(Func<ObjectPoolConfiguration, T> factory, ObjectPoolConfiguration configuration) {
			Contract.EndContractBlock();

			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		/// <summary>
		///		Borrows the item from this pool.
		/// </summary>
		/// <returns>
		///		The item borrowed.
		///		This value cannot be <c>null</c>.
		/// </returns>
		protected sealed override T BorrowCore() {
			var result = _factory(_configuration);
			Contract.Assume(result != null);
			return result;
		}

		/// <summary>
		///		Returns the specified borrowed item.
		/// </summary>
		/// <param name="value">The borrowed item. This value will not be <c>null</c>.</param>
		protected sealed override void ReturnCore(T value) {
			// nop.
		}
	}
}

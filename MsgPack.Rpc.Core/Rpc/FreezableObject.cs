using System;
using System.Threading;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		The standard implementation of the <see cref="IFreezable"/> interface.
	/// </summary>
	public abstract class FreezableObject : IFreezable, ICloneable {
		int _isFrozen;

		/// <summary>
		///		Gets a value indicating whether this instance is frozen.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is frozen; otherwise, <c>false</c>.
		/// </value>
		public bool IsFrozen => Interlocked.CompareExchange(ref _isFrozen, 0, 0) != 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="FreezableObject"/> class.
		/// </summary>
		protected FreezableObject() { }

		/// <summary>
		///		Verifies this instance is not frozen.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///		This instance is already frozen.
		/// </exception>
		protected void VerifyIsNotFrozen() {
			if (IsFrozen) {
				throw new InvalidOperationException("This instance is frozen.");
			}
		}

		/// <summary>
		///		Clones all of the fields of this instance.
		/// </summary>
		/// <returns>
		///		The shallow copy of this instance. Returned instance always is not frozen.
		/// </returns>
		protected virtual FreezableObject CloneCore() {
			var clone = MemberwiseClone() as FreezableObject;
			Interlocked.Exchange(ref clone._isFrozen, 0);
			return clone;
		}

		/// <summary>
		///		Freezes this instance.
		/// </summary>
		/// <returns>
		///		This instance.
		/// </returns>
		protected virtual FreezableObject FreezeCore() {
			Interlocked.Exchange(ref _isFrozen, 1);

			return this;
		}

		/// <summary>
		/// Gets the frozen copy of this instance.
		/// </summary>
		/// <returns>
		/// This instance if it is already frozen.
		/// Otherwise, frozen copy of this instance.
		/// </returns>
		protected virtual FreezableObject AsFrozenCore() {
			if (IsFrozen) {
				return this;
			}

			return CloneCore().FreezeCore();
		}

		object ICloneable.Clone() {
			return CloneCore();
		}

		/// <summary>
		/// Gets the frozen copy of this instance.
		/// </summary>
		/// <returns>
		/// This instance if it is already frozen.
		/// Otherwise, frozen copy of this instance.
		/// </returns>
		IFreezable IFreezable.AsFrozen() {
			return AsFrozenCore();
		}

		IFreezable IFreezable.Freeze() {
			return FreezeCore();
		}
	}
}

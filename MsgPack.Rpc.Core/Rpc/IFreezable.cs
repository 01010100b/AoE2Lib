using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Defines common interface for freezable objects.
	/// </summary>
	[ContractClass(typeof(IFreezableContract))]
	public interface IFreezable {
		/// <summary>
		///		Gets a value indicating whether this instance is frozen.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is frozen; otherwise, <c>false</c>.
		/// </value>
		bool IsFrozen { get; }

		/// <summary>
		///		Freezes this instance.
		/// </summary>
		/// <returns>
		///		This instance.
		/// </returns>
		IFreezable Freeze();

		/// <summary>
		///		Gets the frozen copy of this instance.
		/// </summary>
		/// <returns>
		///		This instance if it is already frozen.
		///		Otherwise, frozen copy of this instance.
		/// </returns>
		IFreezable AsFrozen();
	}

	[ContractClassFor(typeof(IFreezable))]
	internal abstract class IFreezableContract : IFreezable {
		public bool IsFrozen => false;

		public IFreezable Freeze() {
			Contract.Ensures(Contract.Result<IFreezable>() != null);
			Contract.Ensures(ReferenceEquals(Contract.Result<IFreezable>(), this));
			Contract.Ensures(IsFrozen);

			return null;
		}

		public IFreezable AsFrozen() {
			Contract.Ensures(Contract.Result<IFreezable>() != null);
			Contract.Ensures(!ReferenceEquals(Contract.Result<IFreezable>(), this));
			Contract.Ensures(Contract.Result<IFreezable>().IsFrozen);
			Contract.Ensures(IsFrozen == Contract.OldValue(IsFrozen));

			throw new NotImplementedException();
		}
	}
}

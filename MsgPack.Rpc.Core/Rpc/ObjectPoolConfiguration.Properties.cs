using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace MsgPack.Rpc.Core {
	// This file generated from ObjectPoolConfiguration.tt T4Template.
	// Do not modify this file. Edit ObjectPoolConfiguration.tt instead.

	partial class ObjectPoolConfiguration {
		string _name = null;

		/// <summary>
		/// 	Gets or sets the name of the pool for debugging.
		/// </summary>
		/// <value>
		/// 	The name of the pool for debugging. The default is <c>null</c>.
		/// </value>
		public string Name {
			get {
				return _name;
			}
			set {
				VerifyIsNotFrozen();
				var coerced = value;
				CoerceNameValue(ref coerced);
				_name = coerced;
			}
		}

		/// <summary>
		/// 	Resets the Name property value.
		/// </summary>
		public void ResetName() {
			_name = null;
		}

		static partial void CoerceNameValue(ref string value);

		int _minimumReserved = 1;

		/// <summary>
		/// 	Gets or sets the minimum reserved object count in the pool.
		/// </summary>
		/// <value>
		/// 	The minimum reserved object count in the pool. The default is 1.
		/// </value>
		public int MinimumReserved {
			get {
				Contract.Ensures(Contract.Result<int>() >= default(int));

				return _minimumReserved;
			}
			set {
				if (!(value >= default(int))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument cannot be negative number.");
				}

				Contract.Ensures(Contract.Result<int>() >= default(int));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceMinimumReservedValue(ref coerced);
				_minimumReserved = coerced;
			}
		}

		/// <summary>
		/// 	Resets the MinimumReserved property value.
		/// </summary>
		public void ResetMinimumReserved() {
			_minimumReserved = 1;
		}

		static partial void CoerceMinimumReservedValue(ref int value);

		int? _maximumPooled = null;

		/// <summary>
		/// 	Gets or sets the maximum poolable objects count.
		/// </summary>
		/// <value>
		/// 	The maximum poolable objects count. <c>null</c> indicates unlimited pooling. The default is <c>null</c>.
		/// </value>
		public int? MaximumPooled {
			get {
				Contract.Ensures(Contract.Result<int?>() == null || Contract.Result<int?>().Value >= default(int));

				return _maximumPooled;
			}
			set {
				if (!(value == null || value.Value >= default(int))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument cannot be negative number.");
				}

				Contract.Ensures(Contract.Result<int?>() == null || Contract.Result<int?>().Value >= default(int));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceMaximumPooledValue(ref coerced);
				_maximumPooled = coerced;
			}
		}

		/// <summary>
		/// 	Resets the MaximumPooled property value.
		/// </summary>
		public void ResetMaximumPooled() {
			_maximumPooled = null;
		}

		static partial void CoerceMaximumPooledValue(ref int? value);

		ExhausionPolicy _exhausionPolicy = ExhausionPolicy.BlockUntilAvailable;

		/// <summary>
		/// 	Gets or sets the exhausion policy of the pool.
		/// </summary>
		/// <value>
		/// 	The exhausion policy of the pool. The default is <see cref="F:ExhausionPolicy.BlockUntilAvailable"/>.
		/// </value>
		public ExhausionPolicy ExhausionPolicy {
			get {
				Contract.Ensures(Enum.IsDefined(typeof(ExhausionPolicy), Contract.Result<ExhausionPolicy>()));

				return _exhausionPolicy;
			}
			set {
				if (!(Enum.IsDefined(typeof(ExhausionPolicy), value))) {
					throw new ArgumentOutOfRangeException(nameof(value), string.Format(CultureInfo.CurrentCulture, "Argument must be valid enum value of '{0}' type.", typeof(ExhausionPolicy)));
				}

				Contract.Ensures(Enum.IsDefined(typeof(ExhausionPolicy), Contract.Result<ExhausionPolicy>()));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceExhausionPolicyValue(ref coerced);
				_exhausionPolicy = coerced;
			}
		}

		/// <summary>
		/// 	Resets the ExhausionPolicy property value.
		/// </summary>
		public void ResetExhausionPolicy() {
			_exhausionPolicy = ExhausionPolicy.BlockUntilAvailable;
		}

		static partial void CoerceExhausionPolicyValue(ref ExhausionPolicy value);

		TimeSpan? _borrowTimeout = null;

		/// <summary>
		/// 	Gets or sets the maximum concurrency for the each clients.
		/// </summary>
		/// <value>
		/// 	The timeout of blocking of the borrowing when the pool is exhausited. <c>null</c> indicates unlimited waiting. The default is <c>null</c>.
		/// </value>
		public TimeSpan? BorrowTimeout {
			get {
				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				return _borrowTimeout;
			}
			set {
				if (!(value == null || value.Value > default(TimeSpan))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument must be positive number.");
				}

				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceBorrowTimeoutValue(ref coerced);
				_borrowTimeout = coerced;
			}
		}

		/// <summary>
		/// 	Resets the BorrowTimeout property value.
		/// </summary>
		public void ResetBorrowTimeout() {
			_borrowTimeout = null;
		}

		static partial void CoerceBorrowTimeoutValue(ref TimeSpan? value);

		TimeSpan? _evitionInterval = TimeSpan.FromMinutes(3);

		/// <summary>
		/// 	Gets or sets the interval to evict extra pooled objects.
		/// </summary>
		/// <value>
		/// 	The interval to evict extra pooled objects. The default is 3 minutes.
		/// </value>
		public TimeSpan? EvitionInterval {
			get {
				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				return _evitionInterval;
			}
			set {
				if (!(value == null || value.Value > default(TimeSpan))) {
					throw new ArgumentOutOfRangeException(nameof(value), "Argument must be positive number.");
				}

				Contract.Ensures(Contract.Result<TimeSpan?>() == null || Contract.Result<TimeSpan?>().Value > default(TimeSpan));

				VerifyIsNotFrozen();
				var coerced = value;
				CoerceEvitionIntervalValue(ref coerced);
				_evitionInterval = coerced;
			}
		}

		/// <summary>
		/// 	Resets the EvitionInterval property value.
		/// </summary>
		public void ResetEvitionInterval() {
			_evitionInterval = TimeSpan.FromMinutes(3);
		}

		static partial void CoerceEvitionIntervalValue(ref TimeSpan? value);

		/// <summary>
		/// 	Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// 	A string that represents the current object.
		/// </returns>
		public sealed override string ToString() {
			var buffer = new StringBuilder(256);
			buffer.Append("{ ");
			buffer.Append("\"Name\" : ");
			ToString(Name, buffer);
			buffer.Append(", ");
			buffer.Append("\"MinimumReserved\" : ");
			ToString(MinimumReserved, buffer);
			buffer.Append(", ");
			buffer.Append("\"MaximumPooled\" : ");
			ToString(MaximumPooled, buffer);
			buffer.Append(", ");
			buffer.Append("\"ExhausionPolicy\" : ");
			ToString(ExhausionPolicy, buffer);
			buffer.Append(", ");
			buffer.Append("\"BorrowTimeout\" : ");
			ToString(BorrowTimeout, buffer);
			buffer.Append(", ");
			buffer.Append("\"EvitionInterval\" : ");
			ToString(EvitionInterval, buffer);
			buffer.Append(" }");
			return buffer.ToString();
		}

		static void ToString<T>(T value, StringBuilder buffer) {
			if (value == null) {
				buffer.Append("null");
			}

			if (typeof(Delegate).IsAssignableFrom(typeof(T))) {
				var asDelegate = (Delegate)(object)value;
				buffer.Append("\"Type='").Append(asDelegate.Method.DeclaringType);

				if (asDelegate.Target != null) {
					buffer.Append("', Instance='").Append(asDelegate.Target);
				}

				buffer.Append("', Method='").Append(asDelegate.Method).Append("'\"");
				return;
			}

			switch (Type.GetTypeCode(typeof(T))) {
				case TypeCode.Boolean: {
					buffer.Append(value.ToString().ToLowerInvariant());
					break;
				}
				case TypeCode.Byte:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: {
					buffer.Append(value.ToString());
					break;
				}
				default: {
					buffer.Append('"').Append(value.ToString()).Append('"');
					break;
				}
			}
		}
	}
}

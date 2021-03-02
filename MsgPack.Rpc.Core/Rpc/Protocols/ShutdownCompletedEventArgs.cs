using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Contains event data for shutdown completion events both of client and server sides.
	/// </summary>
	public class ShutdownCompletedEventArgs : EventArgs {
		/// <summary>
		///		Gets a <see cref="ShutdownSource"/> value which indicates shutdown source.
		/// </summary>
		/// <value>
		///		A <see cref="ShutdownSource"/> value which indicates shutdown source.
		/// </value>
		public ShutdownSource Source { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShutdownCompletedEventArgs"/> class.
		/// </summary>
		/// <param name="source">A <see cref="ShutdownSource"/> value which indicates shutdown source.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		The <paramref name="source"/> is not valid <see cref="ShutdownSource"/> enumeration value.
		/// </exception>
		public ShutdownCompletedEventArgs(ShutdownSource source) {
			switch (source) {
				case ShutdownSource.Client:
				case ShutdownSource.Server:
				case ShutdownSource.Unknown:
				case ShutdownSource.Disposing: {
					break;
				}
				default: {
					throw new ArgumentOutOfRangeException(nameof(source));
				}
			}

			Contract.EndContractBlock();

			Source = source;
		}
	}
}

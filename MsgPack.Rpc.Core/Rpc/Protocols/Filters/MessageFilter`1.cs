using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.Protocols.Filters {
	/// <summary>
	///		Defines interfaces of the message filter.
	/// </summary>
	/// <remarks>
	///		Message filters intercept serialization/deserialization pipeline of the RPC runtime,
	///		applies own custom logic mainly inspection like auditing or data stream tweaking like compression.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public abstract class MessageFilter<T>
		where T : MessageContext {
		/// <summary>
		///		Initializes a new instance of the <see cref="MessageFilter&lt;T&gt;"/> class.
		/// </summary>
		protected MessageFilter() { }

		/// <summary>
		///		Applies this filter to the specified message.
		/// </summary>
		/// <param name="context">The message context.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		public void ProcessMessage(T context) {
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			Contract.EndContractBlock();

			ProcessMessageCore(context);
		}

		/// <summary>
		///		Applies this filter to the specified message.
		/// </summary>
		/// <param name="context">The message context. This value is not <c>null</c>.</param>
		protected abstract void ProcessMessageCore(T context);
	}
}

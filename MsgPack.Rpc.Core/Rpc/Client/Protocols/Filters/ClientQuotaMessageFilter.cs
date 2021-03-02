using MsgPack.Rpc.Core.Protocols.Filters;
using System;

namespace MsgPack.Rpc.Core.Client.Protocols.Filters {
	/// <summary>
	///		<see cref="QuotaMessageFilter{T}"/> for <see cref="ClientResponseContext" />.
	/// </summary>
	public sealed class ClientQuotaMessageFilter : QuotaMessageFilter<ClientResponseContext> {
		/// <summary>
		///		Initializes a new instance of the <see cref="ClientQuotaMessageFilter"/> class.
		/// </summary>
		/// <param name="quota">The quota. <c>0</c> means no quota (infinite).</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		The value of <paramref name="quota"/> is negative.
		/// </exception>
		public ClientQuotaMessageFilter(long quota) : base(quota) { }
	}
}
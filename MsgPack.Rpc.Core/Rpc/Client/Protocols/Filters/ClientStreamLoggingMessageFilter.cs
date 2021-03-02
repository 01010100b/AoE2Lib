using MsgPack.Rpc.Core.Diagnostics;
using MsgPack.Rpc.Core.Protocols.Filters;
using System;

namespace MsgPack.Rpc.Core.Client.Protocols.Filters {
	/// <summary>
	///		<see cref="StreamLoggingMessageFilter{T}"/> for <see cref="ClientResponseContext" />.
	/// </summary>
	public sealed class ClientStreamLoggingMessageFilter : StreamLoggingMessageFilter<ClientResponseContext> {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientStreamLoggingMessageFilter"/> class.
		/// </summary>
		/// <param name="logger">The <see cref="IMessagePackStreamLogger"/> which will be log sink.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="logger"/> is <c>null</c>.
		/// </exception>
		public ClientStreamLoggingMessageFilter(IMessagePackStreamLogger logger) : base(logger) { }
	}
}

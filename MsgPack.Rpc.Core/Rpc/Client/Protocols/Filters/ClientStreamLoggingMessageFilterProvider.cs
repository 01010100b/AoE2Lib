using MsgPack.Rpc.Core.Diagnostics;
using MsgPack.Rpc.Core.Protocols.Filters;
using System;

namespace MsgPack.Rpc.Core.Client.Protocols.Filters {
	/// <summary>
	///		<see cref="StreamLoggingMessageFilterProvider{T}"/> for <see cref="ClientResponseContext"/>.
	/// </summary>
	public sealed class ClientStreamLoggingMessageFilterProvider : StreamLoggingMessageFilterProvider<ClientResponseContext> {
		readonly ClientStreamLoggingMessageFilter filterInstance;

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientStreamLoggingMessageFilterProvider"/> class.
		/// </summary>
		/// <param name="logger">The <see cref="IMessagePackStreamLogger"/> which will be log sink.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="logger"/> is <c>null</c>.
		/// </exception>
		public ClientStreamLoggingMessageFilterProvider(IMessagePackStreamLogger logger) : base(logger) {
			filterInstance = new ClientStreamLoggingMessageFilter(Logger);
		}

		/// <summary>
		///		Returns a <see cref="MessageFilter{T}"/> instance.
		/// </summary>
		/// <param name="location">The location of the filter to be applied.</param>
		/// <returns>A <see cref="MessageFilter{T}"/> instance.</returns>
		public override MessageFilter<ClientResponseContext> GetFilter(MessageFilteringLocation location) {
			if (location != MessageFilteringLocation.BeforeDeserialization) {
				return null;
			}
			else {
				return filterInstance;
			}
		}
	}
}

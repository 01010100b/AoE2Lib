namespace MsgPack.Rpc.Core.Protocols.Filters {

	/// <summary>
	///		Defines interface of filter providers.
	/// </summary>
	/// <typeparam name="T">The actual type of the <see cref="MessageContext"/>.</typeparam>
	public abstract class MessageFilterProvider<T> : MessageFilterProvider
			where T : MessageContext {
		/// <summary>
		///		Initializes a new instance of the <see cref="MessageFilterProvider&lt;T&gt;"/> class.
		/// </summary>
		protected MessageFilterProvider() { }

		/// <summary>
		///		Returns a <see cref="MessageFilter{T}"/> instance.
		/// </summary>
		/// <param name="location">The location of the filter to be applied.</param>
		/// <returns>A <see cref="MessageFilter{T}"/> instance.</returns>
		public abstract MessageFilter<T> GetFilter(MessageFilteringLocation location);
	}
}
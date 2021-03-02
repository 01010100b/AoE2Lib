using System;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Marks the <see cref="Exception"/> can preserve its stack trace.
	/// </summary>
	public interface IStackTracePreservable {
		/// <summary>
		///		Preserves the current stack trace.
		/// </summary>
		void PreserveStackTrace();
	}
}

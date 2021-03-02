using System;
using System.Collections.Generic;
using System.Net;

namespace MsgPack.Rpc.Core.Diagnostics {
	/// <summary>
	///		Defines common interface of loggers for MessagePack stream.
	/// </summary>
	public interface IMessagePackStreamLogger {
		/// <summary>
		///		Writes the specified data to log sink.
		/// </summary>
		/// <param name="sessionStartTime">The <see cref="DateTimeOffset"/> when session was started.</param>
		/// <param name="remoteEndPoint">The <see cref="EndPoint"/> which is data source of the <paramref name="stream"/>.</param>
		/// <param name="stream">The MessagePack data stream. This value might be corrupted or actually not a MessagePack stream.</param>
		void Write(DateTimeOffset sessionStartTime, EndPoint remoteEndPoint, IEnumerable<byte> stream);
	}
}

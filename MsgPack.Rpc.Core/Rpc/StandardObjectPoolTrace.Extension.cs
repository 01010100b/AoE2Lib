using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.StandardObjectPoolTracing {
	partial class StandardObjectPoolTrace {
		public static bool ShouldTrace(this TraceSource source, MessageId id) {
			Contract.Assert(source != null);

			return source.Switch.ShouldTrace(_typeTable[id]);
		}

		public static void TraceEvent(this TraceSource source, MessageId id, string format, params object[] args) {
			Contract.Assert(source != null);

			source.TraceEvent(
				_typeTable[id],
				(int)id,
				format,
				args
			);
		}

		public static void TraceData(this TraceSource source, MessageId id, params object[] data) {
			Contract.Assert(source != null);

			source.TraceData(
				_typeTable[id],
				(int)id,
				data
			);
		}
	}
}

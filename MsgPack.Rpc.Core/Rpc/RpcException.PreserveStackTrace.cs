using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MsgPack.Rpc.Core {
	partial class RpcException : IStackTracePreservable {
		List<string> _preservedStackTrace;

		void IStackTracePreservable.PreserveStackTrace() {
			if (_preservedStackTrace == null) {
				_preservedStackTrace = new List<string>();
			}

			_preservedStackTrace.Add(new StackTrace(this, true).ToString());
		}

		/// <summary>
		///		Gets a string representation of the immediate frames on the call stack.
		/// </summary>
		/// <returns>A string that describes the immediate frames of the call stack.</returns>
		public override string StackTrace {
			get {
				if (_preservedStackTrace == null || _preservedStackTrace.Count == 0) {
					return base.StackTrace;
				}

				var buffer = new StringBuilder();
				foreach (var preserved in _preservedStackTrace) {
					buffer.Append(preserved);
					buffer.AppendLine("   --- End of preserved stack trace ---");
				}

				buffer.Append(base.StackTrace);
				return buffer.ToString();
			}
		}
	}
}

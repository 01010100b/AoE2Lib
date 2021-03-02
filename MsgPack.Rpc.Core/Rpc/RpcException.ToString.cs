using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace MsgPack.Rpc.Core {
	partial class RpcException {
		/// <summary>
		///		Returns string representation of this exception for debugging.
		/// </summary>
		/// <returns>
		///		String representation of this exception for debugging.
		///		Note that this value contains debug information, so you SHOULD NOT transfer to remote site.
		/// </returns>
		/// <remarks>
		///		This method is equivelant to call <see cref="ToString(bool)"/> with true.
		/// </remarks>
		public override string ToString() {
			// UnSafe to-string.
			return ToString(true);
		}

		/// <summary>
		///		Returns string representation of this exception.
		///		specofying incluses debugging information.
		/// </summary>
		/// <param name="includesDebugInformation">
		///		If you want to include debugging information then true.
		/// </param>
		/// <returns>
		///		String representation of this exception.
		/// </returns>
		public string ToString(bool includesDebugInformation) {
			if ((!includesDebugInformation || remoteExceptions == null) && (_preservedStackTrace == null || _preservedStackTrace.Count == 0)) {
				return base.ToString();
			}
			else {
				// <Type>: <Message> ---> <InnerType1>: <InnerMessage1> ---> <InnerType2>: <InnerMessage2> ---> ...
				// <ServerInnerStackTraceN>
				//    --- End of inner exception stack trace ---
				// <ServerInnerStackTrace1>
				//    --- End of inner exception stack trace ---
				// 
				// Server statck trace:
				// <ServerStackTrace>
				// 
				// Exception rethrown at[N]:
				// <ClientInnerStackTraceN>
				//    --- End of inner exception stack trace ---
				// <ClientInnerStackTrace1>
				//    --- End of inner exception stack trace ---
				// <StackTrace>
				var stringBuilder = new StringBuilder();
				// Build <Type>: <Message> chain
				BuildExceptionMessage(stringBuilder);
				// Build stacktrace chain.
				BuildExceptionStackTrace(stringBuilder);

				return stringBuilder.ToString();
			}
		}

		/// <summary>
		///		Build exception message to specified buffer.
		/// </summary>
		/// <param name="stringBuilder">Buffer.</param>
		void BuildExceptionMessage(StringBuilder stringBuilder) {
			stringBuilder.Append(GetType().FullName).Append(": ").Append(Message);

			if (InnerException != null) {
				Contract.Assert(remoteExceptions == null);

				for (var inner = InnerException; inner != null; inner = inner.InnerException) {
					if (inner is RpcException asRpcException) {
						asRpcException.BuildExceptionMessage(stringBuilder);
					}
					else {
						stringBuilder.Append(" ---> ").Append(inner.GetType().FullName).Append(": ").Append(inner.Message);
					}
				}

				stringBuilder.AppendLine();
			}
			else if (remoteExceptions != null) {
				foreach (var remoteException in remoteExceptions) {
					stringBuilder.Append(" ---> ").Append(remoteException.TypeName).Append(": ").Append(remoteException.Message);
				}

				stringBuilder.AppendLine();
			}
		}

		/// <summary>
		///		Build stack trace string to specified buffer.
		/// </summary>
		/// <param name="stringBuilder">Buffer.</param>
		void BuildExceptionStackTrace(StringBuilder stringBuilder) {
			if (InnerException != null) {
				Contract.Assert(remoteExceptions == null);

				for (var inner = InnerException; inner != null; inner = inner.InnerException) {
					if (inner is RpcException asRpcException) {
						asRpcException.BuildExceptionStackTrace(stringBuilder);
					}
					else {
						BuildGeneralStackTrace(inner, stringBuilder);
					}

					stringBuilder.Append("   --- End of inner exception stack trace ---").AppendLine();
				}
			}
			else if (remoteExceptions != null && remoteExceptions.Length > 0) {
				for (var i = 0; i < remoteExceptions.Length; i++) {
					if (i > 0
						&& remoteExceptions[i].Hop != remoteExceptions[i - 1].Hop
						&& remoteExceptions[i].TypeName == remoteExceptions[i - 1].TypeName
					) {
						// Serialized -> Deserialized case
						stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "Exception transferred at [{0}]:", remoteExceptions[i - 1].Hop).AppendLine();
					}
					else {
						// Inner exception case
						stringBuilder.Append("   --- End of inner exception stack trace ---").AppendLine();
					}

					foreach (var frame in remoteExceptions[i].StackTrace) {
						WriteStackFrame(frame, stringBuilder);
						stringBuilder.AppendLine();
					}
				}

				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "Exception transferred at [{0}]:", remoteExceptions[^1].Hop).AppendLine();
			}

			BuildGeneralStackTrace(this, stringBuilder);
		}

		/// <summary>
		///		Build general statck trace string of specified exception to buffer.
		/// </summary>
		/// <param name="target">Exception which is source of stack trace.</param>
		/// <param name="stringBuilder">Buffer.</param>
		static void BuildGeneralStackTrace(Exception target, StringBuilder stringBuilder) {
			stringBuilder.Append(target.StackTrace);
		}

		/// <summary>
		///		Write remote stack framew string to specified buffer.
		/// </summary>
		/// <param name="frame">Stack frame to write.</param>
		/// <param name="stringBuilder">Buffer.</param>
		static void WriteStackFrame(RemoteStackFrame frame, StringBuilder stringBuilder) {
			const string stackFrameTemplateWithFileInfo = "   at {0} in {1}:line {2}";
			const string stackFrameTemplateWithoutFileInfo = "   at {0}";
			if (frame.FileName != null) {
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, stackFrameTemplateWithFileInfo, frame.MethodSignature, frame.FileName, frame.FileLineNumber);
			}
			else {
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, stackFrameTemplateWithoutFileInfo, frame.MethodSignature);
			}
		}
	}
}

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace MsgPack.Rpc.Core {
	internal sealed class ExceptionDispatchInfo {
		static readonly Type[] constructorParameterStringException = new[] { typeof(string), typeof(Exception) };
		static readonly PropertyInfo exceptionHResultProperty = typeof(Exception).GetProperty("HResult", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo safeCreateMatroshikaMethod = typeof(ExceptionDispatchInfo).GetMethod("SafeCreateMatroshika", BindingFlags.Static | BindingFlags.NonPublic);
		static readonly MethodInfo safeCreateWrapperWin32ExceptionMethod = typeof(ExceptionDispatchInfo).GetMethod("SafeCreateWrapperWin32Exception", BindingFlags.Static | BindingFlags.NonPublic);

		readonly Exception source;

		ExceptionDispatchInfo(Exception source) {
			Contract.EndContractBlock();

			this.source = source ?? throw new ArgumentNullException(nameof(source));
			if (source is IStackTracePreservable preservable) {
				preservable.PreserveStackTrace();
			}
		}

		public void Throw() {
			if (source is IStackTracePreservable) {
				throw source;
			}
			else {
				throw CreateMatroshika(source);
			}
		}

		internal static Exception CreateMatroshika(Exception inner) {
			Contract.Requires(inner != null);
			Contract.Ensures(Contract.Result<Exception>() != null);

			var result = HandleKnownWin32Exception(inner);
			if (result != null) {
				return result;
			}

			result = TryCreateMatroshikaWithExternalExceptionMatroshka(inner);
			if (result != null) {
				return result;
			}

			result = HandleExternalExceptionInPartialTrust(inner);
			if (result != null) {
				return result;
			}

			return GetMatroshika(inner) ?? new TargetInvocationException(inner.Message, inner);
		}
		static Exception HandleKnownWin32Exception(Exception inner) {
			if (inner is SocketException asSocketException) {
				var result = new WrapperSocketException(asSocketException);
				SetMatroshika(inner);
				return result;
			}

			if (inner is HttpListenerException asHttpListenerException) {
				var result = new WrapperHttpListenerException(asHttpListenerException);
				SetMatroshika(inner);
				return result;
			}

			if (inner is NetworkInformationException asNetworkInformationException) {
				var result = new WrapperNetworkInformationException(asNetworkInformationException);
				SetMatroshika(inner);
				return result;
			}

			if (inner is Win32Exception asWin32Exception) {
				if (safeCreateWrapperWin32ExceptionMethod.IsSecuritySafeCritical) {
					var result = SafeCreateWrapperWin32Exception(asWin32Exception);
					return result;
				}
				else {
					return new TargetInvocationException(asWin32Exception.Message, asWin32Exception);
				}
			}

			return null;
		}

		static Exception TryCreateMatroshikaWithExternalExceptionMatroshka(Exception inner) {
			// Try matroshika with HResult setting(requires full trust).
			if (safeCreateMatroshikaMethod.IsSecuritySafeCritical) {
				if (inner is ExternalException asExternalException) {
					var matroshika = SafeCreateMatroshika(asExternalException);
					if (matroshika != null) {
						return matroshika;
					}
					else {
						// Fallback.
						return new TargetInvocationException(inner.Message, inner);
					}
				}
			}

			return null;
		}

		static Exception HandleExternalExceptionInPartialTrust(Exception inner) {
			if (inner is COMException asCOMException) {
				var result = new WrapperCOMException(asCOMException.Message, asCOMException);
				SetMatroshika(inner);
				return result;
			}

			if (inner is SEHException asSEHException) {
				var result = new WrapperSEHException(asSEHException.Message, asSEHException);
				SetMatroshika(inner);
				return result;
			}

			if (inner is ExternalException asExternalException) {
				var result = new WrapperExternalException(asExternalException.Message, asExternalException);
				SetMatroshika(inner);
				return result;
			}

			return null;
		}

		[SecuritySafeCritical]
		static Exception SafeCreateMatroshika(ExternalException inner) {
			var result = GetMatroshika(inner);
			if (result != null) {
				exceptionHResultProperty.SetValue(result, Marshal.GetHRForException(inner), null);
			}

			return result;
		}

		[SecuritySafeCritical]
		static WrapperWin32Exception SafeCreateWrapperWin32Exception(Win32Exception inner) {
			var result = new WrapperWin32Exception(inner.Message, inner);
			SetMatroshika(inner);
			return result;
		}

		static Exception GetMatroshika(Exception inner) {
			var ctor = inner.GetType().GetConstructor(constructorParameterStringException);
			if (ctor == null) {
				return null;
			}
			var result = ctor.Invoke(new object[] { inner.Message, inner }) as Exception;
			SetMatroshika(inner);
			return result;
		}
		static void SetMatroshika(Exception exception) {
			exception.Data[ExceptionModifiers.IsMatrioshkaInner] = null;
		}

		public static ExceptionDispatchInfo Capture(Exception source) {
			// TODO: Capture Watson information.
			return new ExceptionDispatchInfo(source);
		}

		[Serializable]
		sealed class WrapperExternalException : ExternalException {
			public WrapperExternalException(string message, ExternalException inner)
				: base(message, inner) {
				HResult = inner.ErrorCode;
			}

			WrapperExternalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		sealed class WrapperCOMException : COMException {
			public WrapperCOMException(string message, COMException inner)
				: base(message, inner) {
				HResult = inner.ErrorCode;
			}

			WrapperCOMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		sealed class WrapperSEHException : SEHException {
			public WrapperSEHException(string message, SEHException inner)
				: base(message, inner) {
				HResult = inner.ErrorCode;
			}

			WrapperSEHException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		[SecuritySafeCritical]
		sealed class WrapperWin32Exception : Win32Exception {
			public WrapperWin32Exception(string message, Win32Exception inner)
				: base(message, inner) {
				HResult = inner.ErrorCode;
			}

			WrapperWin32Exception(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		sealed class WrapperHttpListenerException : HttpListenerException {
			readonly string innerStackTrace;

			public sealed override string StackTrace => string.Join(
							innerStackTrace,
							"   --- End of preserved stack trace ---",
							Environment.NewLine,
							base.StackTrace
						);

			public WrapperHttpListenerException(HttpListenerException inner)
				: base(inner.ErrorCode) {
				innerStackTrace = inner.StackTrace;
			}

			WrapperHttpListenerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		sealed class WrapperNetworkInformationException : NetworkInformationException {
			readonly string innerStackTrace;

			public sealed override string StackTrace => string.Join(
							innerStackTrace,
							"   --- End of preserved stack trace ---",
							Environment.NewLine,
							base.StackTrace
						);

			public WrapperNetworkInformationException(NetworkInformationException inner)
				: base(inner.ErrorCode) {
				innerStackTrace = inner.StackTrace;
			}

			WrapperNetworkInformationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}

		[Serializable]
		sealed class WrapperSocketException : SocketException {
			readonly string innerStackTrace;

			public sealed override string StackTrace => string.Join(
							innerStackTrace,
							"   --- End of preserved stack trace ---",
							Environment.NewLine,
							base.StackTrace
						);

			public WrapperSocketException(SocketException inner)
				: base(inner.ErrorCode) {
				innerStackTrace = inner.StackTrace;
			}

			WrapperSocketException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
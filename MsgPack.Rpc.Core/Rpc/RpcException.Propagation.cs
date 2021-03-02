using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace MsgPack.Rpc.Core {
	partial class RpcException {
		static readonly MethodInfo safeGetHRFromExceptionMethod = typeof(RpcException).GetMethod("SafeGetHRFromException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

		/// <summary>
		///		Initialize new instance with unpacked data.
		/// </summary>
		/// <param name="rpcError">
		///		Metadata of error. If you specify null, <see cref="Core.RpcError.RemoteRuntimeError"/> is used.
		///	</param>
		/// <param name="unpackedException">
		///		Exception data from remote MessagePack-RPC server.
		///	</param>
		/// <exception cref="SerializationException">
		///		Cannot deserialize instance from <paramref name="unpackedException"/>.
		/// </exception>
		protected internal RpcException(RpcError rpcError, MessagePackObject unpackedException)
			: this(rpcError, unpackedException.GetString(messageKeyUtf8), unpackedException.GetString(debugInformationKeyUtf8)) {
			if (unpackedException.IsDictionary) {
				if (unpackedException.AsDictionary().TryGetValue(remoteExceptionsUtf8, out var mayBeArray) && mayBeArray.IsArray) {
					var array = mayBeArray.AsList();
					remoteExceptions = new RemoteExceptionInformation[array.Count];
					for (var i = 0; i < remoteExceptions.Length; i++) {
						if (array[i].IsList) {
							remoteExceptions[i] = new RemoteExceptionInformation(array[i].AsList());
						}
						else {
							// Unexpected type.
							Debug.WriteLine("Unexepcted ExceptionInformation at {0}, type: {1}, value: \"{2}\".", i, array[i].UnderlyingType, array[i]);
							remoteExceptions[i] = new RemoteExceptionInformation(new MessagePackObject[] { array[i] });
						}
					}
				}
			}

			RegisterSerializeObjectStateEventHandler();
		}

		// NOT readonly for safe-deserialization
		RemoteExceptionInformation[] remoteExceptions;

		[Serializable]
		sealed class RemoteExceptionInformation {
			public readonly int Hop;
			public readonly string TypeName;
			public readonly int HResult;
			public readonly string Message;
			public readonly RemoteStackFrame[] StackTrace;
			public readonly MessagePackObjectDictionary Data;

			public RemoteExceptionInformation(IList<MessagePackObject> unpacked) {
				if (unpacked.Count != 6) {
					throw new SerializationException("Count of remote exception information must be 6.");
				}

				Hop = unpacked[0].AsInt32();
				TypeName = unpacked[1].AsString();
				HResult = unpacked[2].AsInt32();
				Message = unpacked[3].AsString();
				StackTrace = unpacked[4].AsList().Select(item => new RemoteStackFrame(item.AsList())).ToArray();
				Data = unpacked[5].AsDictionary();
			}
		}

		[Serializable]
		sealed class RemoteStackFrame {
			public readonly string MethodSignature;
			public readonly int ILOffset;
			public readonly int NativeOffset;
			public readonly string FileName;
			public readonly int FileLineNumber;
			public readonly int FileColumnNumber;

			public RemoteStackFrame(IList<MessagePackObject> unpacked) {
				switch (unpacked.Count) {
					case 3: {
						MethodSignature = unpacked[0].AsString();
						ILOffset = unpacked[1].AsInt32();
						NativeOffset = unpacked[2].AsInt32();
						FileName = null;
						FileLineNumber = 0;
						FileColumnNumber = 0;
						break;
					}
					case 6: {
						MethodSignature = unpacked[0].AsString();
						ILOffset = unpacked[1].AsInt32();
						NativeOffset = unpacked[2].AsInt32();
						FileName = unpacked[3].AsString();
						FileLineNumber = unpacked[4].AsInt32();
						FileColumnNumber = unpacked[5].AsInt32();
						break;
					}
					default: {
						throw new SerializationException("Count of remote stack frames must be 3 or 6.");
					}
				}
			}
		}

		internal static readonly MessagePackObject messageKeyUtf8 = MessagePackConvert.EncodeString("Message");
		internal static readonly MessagePackObject debugInformationKeyUtf8 = MessagePackConvert.EncodeString("DebugInformation");
		static readonly MessagePackObject errorCodeUtf8 = MessagePackConvert.EncodeString("ErrorCode");
		static readonly MessagePackObject remoteExceptionsUtf8 = MessagePackConvert.EncodeString("RemoteExceptions");

		/// <summary>
		///		Get <see cref="MessagePackObject"/> which contains data about this instance.
		/// </summary>
		/// <param name="isDebugMode">
		///		If this method should include debug information then true.
		/// </param>
		/// <returns>
		///		<see cref="MessagePackObject"/> which contains data about this instance.
		/// </returns>
		public MessagePackObject GetExceptionMessage(bool isDebugMode) {
			var store = new MessagePackObjectDictionary(2) {
				{ errorCodeUtf8, RpcError.ErrorCode },
				{ messageKeyUtf8, isDebugMode ? Message : RpcError.DefaultMessageInvariant }
			};
			GetExceptionMessage(store, isDebugMode);

			return new MessagePackObject(store);
		}

		/// <summary>
		///		Stores derived type specific information to specified dictionary.
		/// </summary>
		/// <param name="store">
		///		Dictionary to be stored. This value will not be <c>null</c>.
		///	</param>
		/// <param name="includesDebugInformation">
		///		<c>true</c>, when this method should include debug information; otherwise, <c>false</c>.
		///	</param>
		protected virtual void GetExceptionMessage(IDictionary<MessagePackObject, MessagePackObject> store, bool includesDebugInformation) {
			Contract.Requires(store != null);

			if (!includesDebugInformation) {
				return;
			}

			if (InnerException != null || remoteExceptions != null) {
				var innerList = new List<MessagePackObject>();
				if (remoteExceptions != null) {
					foreach (var remoteException in remoteExceptions) {
						var properties = new MessagePackObject[6];
						properties[0] = remoteException.Hop + 1;
						properties[1] = MessagePackConvert.EncodeString(remoteException.TypeName);
						// HResult is significant for some exception (e.g. IOException).
						properties[2] = remoteException.HResult;
						properties[3] = MessagePackConvert.EncodeString(remoteException.Message);
						properties[4] =
							Array.ConvertAll(
								remoteException.StackTrace,
								frame =>
									frame.FileName == null
									? new MessagePackObject(new MessagePackObject[] { frame.MethodSignature, frame.ILOffset, frame.NativeOffset })
									: new MessagePackObject(new MessagePackObject[] { frame.MethodSignature, frame.ILOffset, frame.NativeOffset, frame.FileName, frame.FileLineNumber, frame.FileColumnNumber })
							);
						properties[5] = new MessagePackObject(remoteException.Data);
						innerList.Add(properties);
					}
				}

				for (var inner = InnerException; inner != null; inner = inner.InnerException) {
					var properties = new MessagePackObject[6];
					properties[0] = 0;
					properties[1] = MessagePackConvert.EncodeString(inner.GetType().FullName);
					// HResult is significant for some exception (e.g. IOException).
					properties[2] = SafeGetHRFromException(inner);
					properties[3] = MessagePackConvert.EncodeString(inner.Message);

					// stack trace
					var innerStackTrace =
						new StackTrace(inner, true);
					var frames = new MessagePackObject[innerStackTrace.FrameCount];
					for (var i = 0; i < frames.Length; i++) {
						var frame = innerStackTrace.GetFrame(innerStackTrace.FrameCount - (i + 1));
						if (frame.GetFileName() == null) {
							frames[i] = new MessagePackObject[] { ToStackFrameMethodSignature(frame.GetMethod()), frame.GetILOffset(), frame.GetNativeOffset() };
						}
						else {
							frames[i] = new MessagePackObject[] { ToStackFrameMethodSignature(frame.GetMethod()), frame.GetILOffset(), frame.GetNativeOffset(), frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber() };
						}
					}
					properties[4] = new MessagePackObject(frames);

					// data
					if (inner.Data != null && inner.Data.Count > 0) {
						var data = new MessagePackObjectDictionary(inner.Data.Count);
						foreach (System.Collections.DictionaryEntry entry in inner.Data) {
							data.Add(MessagePackObject.FromObject(entry.Key), MessagePackObject.FromObject(entry.Value));
						}

						properties[5] = new MessagePackObject(data);
					}

					innerList.Add(properties);
				}

				store.Add(remoteExceptionsUtf8, new MessagePackObject(innerList));
			}

			store.Add(debugInformationKeyUtf8, DebugInformation);

		}

		static MessagePackObject ToStackFrameMethodSignature(MethodBase methodBase) {
			return string.Concat(methodBase.DeclaringType.FullName, ".", methodBase.Name, "(", string.Join(", ", methodBase.GetParameters().Select(p => p.ParameterType.FullName)), ")");
		}

		[SecuritySafeCritical]
		static int SafeGetHRFromException(Exception exception) {
			if (exception is ExternalException asExternalException) {
				// ExternalException.ErrorCode is SecuritySafeCritical and its assembly must be fully trusted.
				return asExternalException.ErrorCode;
			}
			else if (safeGetHRFromExceptionMethod.IsSecuritySafeCritical) {
				try {
					// Can invoke Marshal.GetHRForException because this assembly is fully trusted.
					return Marshal.GetHRForException(exception);
				}
				catch (SecurityException) { }
				catch (MemberAccessException) { }

				return 0;
			}
			else {
				// Cannot get HResult due to partial trust.
				return 0;
			}
		}
	}
}

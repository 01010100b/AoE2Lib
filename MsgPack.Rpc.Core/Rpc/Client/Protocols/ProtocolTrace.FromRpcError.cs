using System.Diagnostics;

namespace MsgPack.Rpc.Core.Client.Protocols {
	partial class MsgPackRpcClientProtocolsTrace {
		internal static void TraceRpcError(RpcError rpcError, string format, params object[] args) {
			Source.TraceEvent(GetTypeForRpcError(rpcError), GetIdForRpcError(rpcError), format, args);
		}

		static TraceEventType GetTypeForRpcError(RpcError rpcError) {
			if (0 < rpcError.ErrorCode || rpcError.ErrorCode == -31) {
				return TraceEventType.Warning;
			}

			switch (rpcError.ErrorCode % 10) {
				case -2:
				case -4: {
					return TraceEventType.Warning;
				}
				case -1:
				case -3: {
					return TraceEventType.Critical;
				}
				default: {
					return TraceEventType.Error;
				}
			}
		}

		static int GetIdForRpcError(RpcError rpcError) {
			if (0 < rpcError.ErrorCode) {
				return 20000;
			}
			else {
				return 10000 + (rpcError.ErrorCode * -1);
			}
		}
	}
}

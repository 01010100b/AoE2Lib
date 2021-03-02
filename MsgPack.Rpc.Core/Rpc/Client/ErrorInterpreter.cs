using MsgPack.Rpc.Core.Client.Protocols;
using System.Diagnostics.Contracts;
using System.Linq;

namespace MsgPack.Rpc.Core.Client {
	/// <summary>
	///		Interprets error stream.
	/// </summary>
	internal static class ErrorInterpreter {
		/// <summary>
		///		Unpacks <see cref="RpcErrorMessage"/> from stream in the specified context.
		/// </summary>
		/// <param name="context"><see cref="ClientResponseContext"/> which stores serialized error.</param>
		/// <returns>An unpacked <see cref="RpcErrorMessage"/>.</returns>
		internal static RpcErrorMessage UnpackError(ClientResponseContext context) {
			Contract.Assert(context != null);
			Contract.Assert(context.errorBuffer != null);
			Contract.Assert(context.errorBuffer.Length > 0);
			Contract.Assert(context.resultBuffer != null);
			Contract.Assert(context.resultBuffer.Length > 0);

			MessagePackObject error;
			try {
				error = Unpacking.UnpackObject(context.errorBuffer);
			}
			catch (UnpackException) {
				error = new MessagePackObject(context.errorBuffer.GetBuffer().SelectMany(segment => segment.AsEnumerable()).ToArray());
			}

			if (error.IsNil) {
				return RpcErrorMessage.Success;
			}

			var isUnknown = false;
			RpcError errorIdentifier;
			if (error.IsTypeOf<string>().GetValueOrDefault()) {
				var asString = error.AsString();
				errorIdentifier = RpcError.FromIdentifier(asString, null);
				// Check if the error is truely Unexpected error.
				isUnknown = errorIdentifier.ErrorCode == RpcError.Unexpected.ErrorCode && asString != RpcError.Unexpected.Identifier;
			}
			else if (error.IsTypeOf<int>().GetValueOrDefault()) {
				errorIdentifier = RpcError.FromIdentifier(null, error.AsInt32());
			}
			else {
				errorIdentifier = RpcError.Unexpected;
				isUnknown = true;
			}

			MessagePackObject detail;
			if (context.resultBuffer.Length == 0) {
				detail = MessagePackObject.Nil;
			}
			else {
				try {
					detail = Unpacking.UnpackObject(context.resultBuffer);
				}
				catch (UnpackException) {
					detail = new MessagePackObject(context.resultBuffer.GetBuffer().SelectMany(segment => segment.AsEnumerable()).ToArray());
				}
			}

			if (isUnknown) {
				// Unknown error, the error should contain original Error field as message.
				if (detail.IsNil) {
					return new RpcErrorMessage(errorIdentifier, error.AsString(), null);
				}
				else {
					var details = new MessagePackObjectDictionary(2) {
						[RpcException.messageKeyUtf8] = error,
						[RpcException.debugInformationKeyUtf8] = detail
					};

					return new RpcErrorMessage(errorIdentifier, new MessagePackObject(details, true));
				}
			}
			else {
				return new RpcErrorMessage(errorIdentifier, detail);
			}
		}
	}
}

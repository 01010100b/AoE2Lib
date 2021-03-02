using MsgPack.Rpc.Core.Protocols;
using System;
using System.Diagnostics.Contracts;

namespace MsgPack.Rpc.Core.Client.Protocols {
	/// <summary>
	///		Represents context information for the client side response.
	/// </summary>
	public sealed class ClientResponseContext : InboundMessageContext {
		/// <summary>
		///		Next (that is, resuming) process on the deserialization pipeline.
		/// </summary>
		internal Func<ClientResponseContext, bool> nextProcess;

		internal long errorStartAt;

		/// <summary>
		///		Subtree <see cref="Unpacker"/> to parse error value as opaque sequence.
		/// </summary>
		internal ByteArraySegmentStream errorBuffer;

		internal long resultStartAt;

		/// <summary>
		///		Subtree <see cref="Unpacker"/> to parse return value as opaque sequence.
		/// </summary>
		internal ByteArraySegmentStream resultBuffer;

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientResponseContext"/> class with default settings.
		/// </summary>
		public ClientResponseContext()
			: this(null) {
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="ClientResponseContext"/> class with specified configuration.
		/// </summary>
		/// <param name="configuration">
		///		An <see cref="RpcClientConfiguration"/> to tweak this instance initial state.
		/// </param>
		public ClientResponseContext(RpcClientConfiguration configuration)
			: base((configuration ?? RpcClientConfiguration.Default).InitialReceiveBufferLength) {
			errorStartAt = -1;
			resultStartAt = -1;
		}

		public ByteArraySegmentStream GetResultBuffer() {
			return resultBuffer;
		}

		public ByteArraySegmentStream GetErrorBuffer() {
			return errorBuffer;
		}

		internal long? SkipResultSegment() {
#if DEBUG
			Contract.Assert(resultStartAt > -1);
#endif
			return SkipHeader(resultStartAt);
		}

		internal long? SkipErrorSegment() {
#if DEBUG
			Contract.Assert(errorStartAt > -1);
#endif
			return SkipHeader(errorStartAt);
		}

		long? SkipHeader(long origin) {
			var result = HeaderUnpacker.Skip();
			if (result == null) {
				// Revert buffer position to handle next attempt.
				UnpackingBuffer.Position = origin;
			}

			return result;
		}

		/// <summary>
		///		Sets the bound <see cref="ClientTransport"/>.
		/// </summary>
		/// <param name="transport">The binding transport.</param>
		internal void SetTransport(ClientTransport transport) {
			Contract.Requires(transport != null);

			nextProcess = transport.UnpackResponseHeader;
			base.SetTransport(transport);
		}

		static bool InvalidFlow(ClientResponseContext context) {
			throw new InvalidOperationException("Invalid state transition.");
		}

		/// <summary>
		///		Clears this instance internal buffers for reuse.
		/// </summary>
		internal sealed override void Clear() {
			ClearBuffers();
			nextProcess = InvalidFlow;
			base.Clear();
		}

		/// <summary>
		///		Clears the buffers to deserialize message.
		/// </summary>
		internal override void ClearBuffers() {
			if (errorBuffer != null) {
				errorBuffer.Dispose();
				errorBuffer = null;
			}

			if (resultBuffer != null) {
				resultBuffer.Dispose();
				resultBuffer = null;
			}

			errorStartAt = -1;
			resultStartAt = -1;

			base.ClearBuffers();
		}
	}
}

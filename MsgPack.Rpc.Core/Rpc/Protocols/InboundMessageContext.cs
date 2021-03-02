using System;
using System.Collections.Generic;
using System.Linq;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Defines basic functionality for inbound message contexts.
	/// </summary>
	public abstract class InboundMessageContext : MessageContext {
		readonly List<ArraySegment<byte>> _receivedData;

		/// <summary>
		///		Gets the received data.
		/// </summary>
		/// <value>
		///		The received data.
		///		This value wlll not be <c>null</c>.
		/// </value>
		public IList<ArraySegment<byte>> ReceivedData => _receivedData;

		/// <summary>
		///		Gets the buffer to receive data.
		/// </summary>
		/// <value>
		///		The buffer to receive data.
		///		This value will not be <c>null</c>.
		///		Available section is started with _receivingBufferOffset.
		/// </value>
		internal byte[] CurrentReceivingBuffer { get; private set; }

		/// <summary>
		///		Gets the current offset of the <see cref="CurrentReceivingBuffer"/>.
		/// </summary>
		/// <value>
		///		The current offset of the <see cref="CurrentReceivingBuffer"/>.
		/// </value>
		internal int CurrentReceivingBufferOffset { get; private set; }

		/// <summary>
		///		Buffer that stores unpacking binaries received.
		/// </summary>
		internal ByteArraySegmentStream UnpackingBuffer;

		/// <summary>
		///		<see cref="Unpacker"/> to unpack entire request/notification message.
		/// </summary>
		internal Unpacker RootUnpacker;

		/// <summary>
		///		Subtree <see cref="Unpacker"/> to unpack request/notification message as array.
		/// </summary>
		internal Unpacker HeaderUnpacker;

		/// <summary>
		///		Initializes a new instance of the <see cref="InboundMessageContext"/> class.
		/// </summary>
		protected InboundMessageContext()
			: this(65536) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="InboundMessageContext"/> class.
		/// </summary>
		/// <param name="initialReceivingBufferSize">
		///		Initial size of receiving buffer.
		/// </param>
		protected InboundMessageContext(int initialReceivingBufferSize)
			: base() {
			CurrentReceivingBuffer = new byte[initialReceivingBufferSize];
			// TODO: ArrayDeque is preferred.
			_receivedData = new List<ArraySegment<byte>>(1);
		}

		internal bool ReadFromRootUnpacker() {
			return RootUnpacker.TryRead(UnpackingBuffer);
		}

		internal bool ReadFromHeaderUnpacker() {
			return HeaderUnpacker.TryRead(UnpackingBuffer);
		}

		/// <summary>
		///		Set internal received data buffer for testing purposes.
		/// </summary>
		/// <param name="data">Data to be set.</param>
		internal void SetReceivedData(IList<ArraySegment<byte>> data) {
			_receivedData.Clear();
			_receivedData.AddRange(data);
		}

		/// <summary>
		///		Set internal receiving buffer for testing purposes.
		/// </summary>
		/// <param name="data">Data to be set.</param>
		internal void SetReceivingBuffer(byte[] data) {
			SocketContext.SetBuffer(data, 0, data.Length);
		}

		/// <summary>
		///		Shifts the current receiving buffer offset with transferred bytes,
		///		and reallocates buffer for receiving if necessary.
		/// </summary>
		internal void ShiftCurrentReceivingBuffer() {
			var shift = BytesTransferred;
			_receivedData.Add(new ArraySegment<byte>(CurrentReceivingBuffer, SocketContext.Offset, shift));
			CurrentReceivingBufferOffset += shift;
			if (CurrentReceivingBufferOffset >= CurrentReceivingBuffer.Length) {
				// Replace with new buffer.
				CurrentReceivingBuffer = new byte[CurrentReceivingBuffer.Length];
				CurrentReceivingBufferOffset = 0;
			}

			// Set new offset and length.
			SocketContext.SetBuffer(CurrentReceivingBuffer, CurrentReceivingBufferOffset, CurrentReceivingBuffer.Length - CurrentReceivingBufferOffset);
		}

		/// <summary>
		///		Prepares socket context array buffer with <see cref="CurrentReceivingBuffer"/>
		/// </summary>
		internal void PrepareReceivingBuffer() {
			SocketContext.SetBuffer(CurrentReceivingBuffer, 0, CurrentReceivingBuffer.Length);
		}

		internal override void Clear() {
			if (UnpackingBuffer != null) {
				UnpackingBuffer.Dispose();
				UnpackingBuffer = null;
			}

			base.Clear();
		}

		internal virtual void ClearBuffers() {
			if (HeaderUnpacker != null) {
				try {
					HeaderUnpacker.Dispose();
				}
				catch (InvalidMessagePackStreamException) {
					// Handles cleanup for corruppted stream.
				}

				HeaderUnpacker = null;
			}

			if (RootUnpacker != null) {
				try {
					RootUnpacker.Dispose();
				}
				catch (InvalidMessagePackStreamException) {
					// Handles cleanup for corruppted stream.
				}

				RootUnpacker = null;
			}

			if (UnpackingBuffer != null) {
				TruncateUsedReceivedData();
			}
		}

		/// <summary>
		///		Truncates the used segments from the received data.
		/// </summary>
		void TruncateUsedReceivedData() {
			var removals = UnpackingBuffer.Position;
			var segments = UnpackingBuffer.GetBuffer();
			while (segments.Any() && 0 < removals) {
				if (segments[0].Count <= removals) {
					removals -= segments[0].Count;
					segments.RemoveAt(0);
				}
				else {
					var newCount = segments[0].Count - unchecked((int)removals);
					var newOffset = segments[0].Offset + unchecked((int)removals);
					segments[0] = new ArraySegment<byte>(segments[0].Array, newOffset, newCount);
					removals = 0;
				}
			}
		}
	}
}

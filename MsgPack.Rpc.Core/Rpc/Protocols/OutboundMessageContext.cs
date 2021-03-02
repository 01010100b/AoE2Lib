using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace MsgPack.Rpc.Core.Protocols {
	/// <summary>
	///		Defines basic functionality for outbound message contexts.
	/// </summary>
	public abstract class OutboundMessageContext : MessageContext {
		/// <summary>
		///		The reusable buffer to pack message ID.
		///		This value will not be <c>null</c>.
		/// </summary>
		readonly MemoryStream _idBuffer;

		/// <summary>
		///		Gets or sets the buffer lists for sending by socket.
		/// </summary>
		/// <value>
		///		The <see cref="IList{T}"/> of <see cref="ArraySegment{T}"/> of <see cref="byte"/> which is the buffer lists for sending by socket.
		/// </value>
		public IList<ArraySegment<byte>> SendingSocketBuffers {
			get { return SocketContext.BufferList; }
			set { SocketContext.BufferList = value; }
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="OutboundMessageContext"/> class.
		/// </summary>
		protected OutboundMessageContext()
			: base() {
			_idBuffer = new MemoryStream(5);
		}

		/// <summary>
		///		Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_idBuffer.Dispose();
			}

			base.Dispose(disposing);
		}

		internal ArraySegment<byte> GetPackedMessageId() {
			Contract.Assert(_idBuffer.Position == 0);

			using (var packer = Packer.Create(_idBuffer, false)) {
				packer.Pack(MessageId);
			}

			return new ArraySegment<byte>(_idBuffer.GetBuffer(), 0, unchecked((int)_idBuffer.Length));
		}

		internal virtual void ClearBuffers() {
			_idBuffer.SetLength(0);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace MsgPack.Rpc.Core {
	[DebuggerTypeProxy(typeof(DebuggerProxy))]
	public sealed class ByteArraySegmentStream : Stream {
		readonly IList<ArraySegment<byte>> segments;

		int segmentIndex;
		int offsetInCurrentSegment;

		public sealed override bool CanRead => true;

		public sealed override bool CanSeek => true;

		public sealed override bool CanWrite => false;

		public sealed override long Length => segments.Sum(item => (long)item.Count);

		long position;

		public sealed override long Position {
			get {
				return position;
			}
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				Seek(value - position);
			}
		}

		public ByteArraySegmentStream(IList<ArraySegment<byte>> underlying) {
			segments = underlying;
		}

		public sealed override int Read(byte[] buffer, int offset, int count) {
			var remains = count;
			var result = 0;
			while (0 < remains && segmentIndex < segments.Count) {
				var copied = segments[segmentIndex].CopyTo(offsetInCurrentSegment, buffer, offset + result, remains);
				result += copied;
				remains -= copied;
				offsetInCurrentSegment += copied;

				if (offsetInCurrentSegment == segments[segmentIndex].Count) {
					segmentIndex++;
					offsetInCurrentSegment = 0;
				}

				position += copied;
			}

			return result;
		}

		public sealed override long Seek(long offset, SeekOrigin origin) {
			var length = Length;
			long offsetFromCurrent;
			switch (origin) {
				case SeekOrigin.Begin: {
					offsetFromCurrent = offset - position;
					break;
				}
				case SeekOrigin.Current: {
					offsetFromCurrent = offset;
					break;
				}
				case SeekOrigin.End: {
					offsetFromCurrent = length + offset - position;
					break;
				}
				default: {
					throw new ArgumentOutOfRangeException(nameof(origin));
				}
			}

			if (offsetFromCurrent + position < 0 || length < offsetFromCurrent + position) {
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			Seek(offsetFromCurrent);
			return position;
		}

		void Seek(long offsetFromCurrent) {
#if DEBUG
			Contract.Assert(0 <= offsetFromCurrent + position, offsetFromCurrent + position + " < 0");
			Contract.Assert(offsetFromCurrent + position <= Length, Length + " <= " + offsetFromCurrent + position);
#endif

			if (offsetFromCurrent < 0) {
				for (long i = 0; offsetFromCurrent < i; i--) {
					if (offsetInCurrentSegment == 0) {
						segmentIndex--;
						Contract.Assert(0 <= segmentIndex);
						offsetInCurrentSegment = segments[segmentIndex].Count - 1;
					}
					else {
						offsetInCurrentSegment--;
					}

					position--;
				}
			}
			else {
				for (long i = 0; i < offsetFromCurrent; i++) {
					if (offsetInCurrentSegment == segments[segmentIndex].Count - 1) {
						segmentIndex++;
						Contract.Assert(segmentIndex <= segments.Count);
						offsetInCurrentSegment = 0;
					}
					else {
						offsetInCurrentSegment++;
					}

					position++;
				}
			}
		}

		public IList<ArraySegment<byte>> GetBuffer() {
			return segments;
		}

		public IList<ArraySegment<byte>> GetBuffer(long start, long length) {
			if (start < 0) {
				throw new ArgumentOutOfRangeException(nameof(start));
			}

			if (length < 0) {
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			var result = new List<ArraySegment<byte>>(segments.Count);
			long taken = 0;
			var toBeSkipped = start;
			foreach (var segment in segments) {
				var skipped = 0;
				if (toBeSkipped > 0) {
					if (segment.Count <= toBeSkipped) {
						toBeSkipped -= segment.Count;
						continue;
					}

					skipped = unchecked((int)toBeSkipped);
					toBeSkipped = 0;
				}

				var available = segment.Count - skipped;
				var required = length - taken;
				if (required <= available) {
					taken += required;
					result.Add(new ArraySegment<byte>(segment.Array, segment.Offset + skipped, unchecked((int)required)));
					break;
				}
				else {
					taken += available;
					result.Add(new ArraySegment<byte>(segment.Array, segment.Offset + skipped, available));
				}
			}

			return result;
		}

		public byte[] ToArray() {
			if (segments.Count == 0) {
				return Array.Empty<byte>();
			}

			var result = segments[0].AsEnumerable();
			for (var i = 1; i < segments.Count; i++) {
				result = result.Concat(segments[i].AsEnumerable());
			}

			return result.ToArray();
		}

		public sealed override void Flush() {
			// nop
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			throw new NotSupportedException();
		}

		public sealed override void EndWrite(IAsyncResult asyncResult) {
			throw new NotSupportedException();
		}

		public sealed override void Write(byte[] buffer, int offset, int count) {
			throw new NotSupportedException();
		}

		public sealed override void WriteByte(byte value) {
			throw new NotSupportedException();
		}

		internal sealed class DebuggerProxy {
			readonly ByteArraySegmentStream source;

			public bool CanSeek => source.CanSeek;

			public bool CanRead => source.CanRead;

			public bool CanWrite => source.CanWrite;

			public bool CanTimeout => source.CanTimeout;

			public int ReadTimeout {
				get { return source.ReadTimeout; }
				set { source.ReadTimeout = value; }
			}

			public int WriteTimeout {
				get { return source.WriteTimeout; }
				set { source.WriteTimeout = value; }
			}

			public long Position {
				get { return source.Position; }
				set { source.Position = value; }
			}

			public long Length => source.Length;

			public IList<ArraySegment<byte>> Segments => source.segments ?? Array.Empty<ArraySegment<byte>>();

			public string Data => "[" +
						string.Join(
							",",
							Segments.Select(
								s => s.AsEnumerable().Select(b => b.ToString("X2"))
							).Aggregate((current, subsequent) => current.Concat(subsequent))
						) + "]";

			public DebuggerProxy(ByteArraySegmentStream source) {
				this.source = source;
			}
		}
	}
}

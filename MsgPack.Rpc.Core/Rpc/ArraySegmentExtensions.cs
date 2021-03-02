using System;
using System.Collections.Generic;

namespace MsgPack.Rpc.Core {
	internal static class ArraySegmentExtensions {
		public static T Get<T>(this ArraySegment<T> source, int index) {
			if (source.Array == null) {
				throw new ArgumentNullException(nameof(source));
			}

			if (index < 0 || source.Count <= index) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return source.Array[source.Offset + index];
		}

		public static int CopyTo<T>(this ArraySegment<T> source, int sourceOffset, T[] array, int arrayOffset, int count) {
			if (array == null) {
				throw new ArgumentNullException(nameof(array));
			}

			if (source.Count == 0) {
				return 0;
			}

			if (source.Count <= sourceOffset) {
				throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			}

			int length;
			if (source.Count - sourceOffset < count) {
				length = source.Count - sourceOffset;
			}
			else {
				length = count;
			}

			if (array.Length - arrayOffset < length) {
				throw new ArgumentException("Array is too small.", nameof(array));
			}

			if (source.Array == null) {
				return 0;
			}

			Array.ConstrainedCopy(source.Array, source.Offset + sourceOffset, array, arrayOffset, length);

			return length;
		}

		public static IEnumerable<T> AsEnumerable<T>(this ArraySegment<T> source) {
			if (source.Array == null) {
				yield break;
			}

			for (var i = 0; i < source.Count; i++) {
				yield return source.Array[i + source.Offset];
			}
		}
	}
}

using System.IO;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Defines extension methods for RPC.
	/// </summary>
	internal static class UnpackerExtensions {
		/// <summary>
		///		Tries read from <see cref="Unpacker"/> considering fragmented receival.
		/// </summary>
		/// <param name="unpacker">The unpacker.</param>
		/// <param name="underyingStream">The underying stream.</param>
		/// <returns><c>true</c> if data read successfully; otherwise, <c>false</c></returns>
		public static bool TryRead(this Unpacker unpacker, Stream underyingStream) {
			var position = underyingStream.Position;
			try {
				return unpacker.Read();
			}
			catch (InvalidMessagePackStreamException) {
				if (underyingStream.Position == underyingStream.Length) {
					// It was fragmented data, so we may be able to read them on next time.
					underyingStream.Position = position;
					return false;
				}

				throw;
			}
		}
	}
}

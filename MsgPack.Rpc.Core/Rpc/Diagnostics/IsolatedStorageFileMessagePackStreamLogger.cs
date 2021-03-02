using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text.RegularExpressions;

namespace MsgPack.Rpc.Core.Diagnostics {
	/// <summary>
	///		Isolated storage file based <see cref="MessagePackStreamLogger"/> implementation.
	/// </summary>
	public class IsolatedStorageFileMessagePackStreamLogger : MessagePackStreamLogger {
		static readonly Regex ipAddressEscapingRegex = new Regex(@"[:\./]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

		/// <summary>
		/// Initializes a new instance of the <see cref="IsolatedStorageFileMessagePackStreamLogger"/> class.
		/// </summary>
		public IsolatedStorageFileMessagePackStreamLogger() { }

		/// <summary>
		/// Writes the specified data to log sink.
		/// </summary>
		/// <param name="sessionStartTime">The <see cref="DateTimeOffset"/> when session was started.</param>
		/// <param name="remoteEndPoint">The <see cref="EndPoint"/> which is data source of the <paramref name="stream"/>.</param>
		/// <param name="stream">The MessagePack data stream. This value might be corrupted or actually not a MessagePack stream.</param>
		public override void Write(DateTimeOffset sessionStartTime, EndPoint remoteEndPoint, IEnumerable<byte> stream) {
			string remoteEndPointString;
			if (remoteEndPoint is DnsEndPoint dnsEndPoint) {
				remoteEndPointString = ipAddressEscapingRegex.Replace(dnsEndPoint.Host, "_");
			}
			else if (remoteEndPoint is IPEndPoint ipEndPoint) {
				remoteEndPointString = ipAddressEscapingRegex.Replace(ipEndPoint.Address.ToString(), "_");
			}
			else {
				remoteEndPointString = "(unknown)";
			}

			var fileName = string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd_HHmmss_fff}-{1}-{2}.mpac", sessionStartTime.UtcDateTime, remoteEndPointString, ThreadId);

			while (true) {
				try {
					using var storage = IsolatedStorageFile.GetUserStoreForApplication();
					using var fileStream = storage.OpenFile(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);

					if (stream != null) {
						var written = fileStream.Length;
						foreach (var b in Skip(stream, written)) {
							fileStream.WriteByte(b);
						}
					}

					break;
				}
				catch (DirectoryNotFoundException) { }
			}
		}

		static IEnumerable<T> Skip<T>(IEnumerable<T> source, long count) {
			long index = 0;
			foreach (var item in source) {
				if (index >= count) {
					yield return item;
				}

				index++;
			}
		}
	}
}

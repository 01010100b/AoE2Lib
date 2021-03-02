using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace MsgPack.Rpc.Core.Diagnostics {
	/// <summary>
	///		File based <see cref="MessagePackStreamLogger"/> implementation.
	/// </summary>
	/// <remarks>
	///		The log file path will be <c>{BaseDirectory}\{ProcessName}[-{AppDomainName}]\{ProcessStartTime}-{ProcessId}\{TimeStamp}-{EndPoint}-{ThreadId}.mpac</c>,
	///		so you should specify short path to <see cref="BaseDirectoryPath"/>.
	///		<note>
	///			AppDomainName is omitted in default AppDomain.
	///		</note>
	/// </remarks>
	public class FileMessagePackStreamLogger : MessagePackStreamLogger {
		static readonly Regex ipAddressEscapingRegex = new Regex(@"[:\./]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

		/// <summary>
		///		Gets the base directory path.
		/// </summary>
		/// <value>
		///		The base directory path.
		/// </value>
		public string BaseDirectoryPath { get; }

		/// <summary>
		///		Gets the calculated directory path which will store logfiles.
		/// </summary>
		/// <value>
		///		The calculated directory path which will store logfiles.
		/// </value>
		public string DirectoryPath { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileMessagePackStreamLogger"/> class.
		/// </summary>
		/// <param name="baseDirectoryPath">The base directory path.</param>
		public FileMessagePackStreamLogger(string baseDirectoryPath) {
			BaseDirectoryPath = baseDirectoryPath;
			// {BaseDirectory}\{ProcessName}[-{AppDomainName}]\{ProcessStartTime}-{ProcessId}\{TimeStamp}-{EndPoint}-{ThreadId}.mpac
			if (AppDomain.CurrentDomain.IsDefaultAppDomain()) {
				DirectoryPath = Path.Combine(BaseDirectoryPath, ProcessName, string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd_HHmmss}-{1}", ProcessStartTimeUtc, ProcessId));
			}
			else {
				DirectoryPath = Path.Combine(BaseDirectoryPath, ProcessName + "-" + AppDomain.CurrentDomain.FriendlyName, string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd_HHmmss}-{1}", ProcessStartTimeUtc, ProcessId));
			}
		}

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

			var filePath = Path.Combine(DirectoryPath, string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd_HHmmss_fff}-{1}-{2}.mpac", sessionStartTime.UtcDateTime, remoteEndPointString, ThreadId));

			while (true) {
				if (!Directory.Exists(DirectoryPath)) {
					Directory.CreateDirectory(DirectoryPath);
				}

				try {
					using var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.None);

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

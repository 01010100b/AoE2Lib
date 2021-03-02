using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;

namespace MsgPack.Rpc.Core.Diagnostics {
	/// <summary>
	///		Implements basic common features for <see cref="IMessagePackStreamLogger"/>s.
	/// </summary>
	public abstract class MessagePackStreamLogger : IMessagePackStreamLogger, IDisposable {
		static readonly ThreadLocal<TraceEventCache> traceEventCache = new ThreadLocal<TraceEventCache>(() => new TraceEventCache());

		static DateTime GetProcessStartTimeUtc() {
			try {
				return PrivilegedGetProcessStartTimeUtc();
			}
			catch (SecurityException) {
				// This value ensures that resulting process identifier is unique.
				return DateTime.UtcNow;
			}
			catch (MemberAccessException) {
				// This value ensures that resulting process identifier is unique.
				return DateTime.UtcNow;
			}
		}

		[SecuritySafeCritical]
		static DateTime PrivilegedGetProcessStartTimeUtc() {
			using var process = Process.GetCurrentProcess();
			return process.StartTime.ToUniversalTime();
		}

		static string GetProcessName() {
			try {
				return PrivilegedGetProcessName();
			}
			catch (SecurityException) {
				return string.Empty;
			}
			catch (MemberAccessException) {
				return string.Empty;
			}
		}

		[SecuritySafeCritical]
		static string PrivilegedGetProcessName() {
			using var process = Process.GetCurrentProcess();
			return Path.GetFileNameWithoutExtension(process.MainModule.ModuleName);
		}

		/// <summary>
		///		Gets the current process id.
		/// </summary>
		/// <value>
		///		The current process id.
		/// </value>
		protected static int ProcessId => traceEventCache.Value.ProcessId;

		/// <summary>
		///		Gets the current process start time in UTC.
		/// </summary>
		/// <value>
		///		The current process start time in UTC. 
		/// </value>
		protected static DateTime ProcessStartTimeUtc { get; } = GetProcessStartTimeUtc();

		/// <summary>
		///		Gets the name of the current process.
		/// </summary>
		/// <value>
		///		The name of the current process.
		/// </value>
		protected static string ProcessName { get; } = GetProcessName();

		/// <summary>
		///		Gets the managed thread identifier.
		/// </summary>
		/// <value>
		///		The managed thread identifier.
		/// </value>
		protected static string ThreadId => traceEventCache.Value.ThreadId;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePackStreamLogger"/> class.
		/// </summary>
		protected MessagePackStreamLogger() { }

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			// nop
		}

		/// <summary>
		///		Writes the specified data to log sink.
		/// </summary>
		/// <param name="sessionStartTime">The <see cref="DateTimeOffset"/> when session was started.</param>
		/// <param name="remoteEndPoint">The <see cref="EndPoint"/> which is data source of the <paramref name="stream"/>.</param>
		/// <param name="stream">The MessagePack data stream. This value might be corrupted or actually not a MessagePack stream.</param>
		public abstract void Write(DateTimeOffset sessionStartTime, EndPoint remoteEndPoint, IEnumerable<byte> stream);
	}
}

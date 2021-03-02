using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MsgPack.Rpc.Core.Client {
	internal static class FileSystem {
		static readonly Regex _invalidPathChars =
			new Regex(
				"[" + Regex.Escape(string.Join(string.Empty, Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct())) + "]",
				 RegexOptions.Compiled |
				 RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline
			);

		public static string EscapeInvalidPathChars(string value, string replacement) {
			if (value == null) {
				throw new ArgumentNullException(nameof(value));
			}

			return _invalidPathChars.Replace(value, replacement ?? string.Empty);
		}
	}
}

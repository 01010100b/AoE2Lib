using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Utility to ensure valid identifier.
	/// </summary>
	internal static class RpcIdentifierUtility {
		// There is NO SPEC for RPC identifiers, so use UAX-31.
		// See http://www.unicode.org/reports/tr31/
		// Note that UAX-31 (and CLS) does not allow leading underscore('_') in the identifier.
		const string _idStart = @"\p{L}\p{Nl}";
		const string _idContinue = @"\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}";

		static readonly Regex _validIdentififerPattern = new Regex("^[" + _idStart + "]([" + _idContinue + "]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

		/// <summary>
		///		Verifies the specified identifier is compliant to MessagePack-RPC spec and returns normalized one.
		/// </summary>
		/// <param name="identifier">The target indentifier.</param>
		/// <param name="parameterName">The parameter name to be used in the error message.</param>
		/// <returns>
		///		Normalized identifier.
		/// </returns>
		public static string EnsureValidIdentifier(string identifier, string parameterName) {
			if (string.IsNullOrEmpty(identifier)) {
				return identifier;
			}

			var normalized = identifier.Normalize(NormalizationForm.FormC);

			if (!_validIdentififerPattern.IsMatch(normalized)) {
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"'{0}' is not valid identifier.",
						Escape(identifier)
					),
					parameterName
				);
			}

			return normalized;
		}

		static string Escape(string identifier) {
			var buffer = new StringBuilder(identifier.Length);
			foreach (var c in identifier) {
				switch (CharUnicodeInfo.GetUnicodeCategory(c)) {
					case UnicodeCategory.Control:
					case UnicodeCategory.OtherNotAssigned:
					case UnicodeCategory.PrivateUse: {
						buffer.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X}", (ushort)c);
						break;
					}
					default: {
						buffer.Append(c);
						break;
					}
				}
			}

			return buffer.ToString();
		}
	}
}

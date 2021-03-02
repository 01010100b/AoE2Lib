using System.Diagnostics.Contracts;
using System.Globalization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Defines utlities related to ServiceID.
	/// </summary>
	internal static class ServiceIdentifier {
		/// <summary>
		///		Creates new service ID.
		/// </summary>
		/// <param name="name">The name (required).</param>
		/// <param name="version">The version.</param>
		/// <returns>The service ID.</returns>
		public static string CreateServiceId(string name, int version) {
			Contract.Requires(!string.IsNullOrWhiteSpace(name));
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", name, version);
		}

		/// <summary>
		///		Truncates the generics suffix from the type name.
		/// </summary>
		/// <param name="typeName">Simple name of the type.</param>
		/// <returns>The name without generics suffix.</returns>
		public static string TruncateGenericsSuffix(string typeName) {
			Contract.Requires(typeName != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var positionOfBackQuote = typeName.IndexOf('`');
			return positionOfBackQuote < 0 ? typeName : typeName.Remove(positionOfBackQuote);
		}
	}
}

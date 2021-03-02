using System;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Marks the type represents service contract for the MessagePack-RPC.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class MessagePackRpcServiceContractAttribute : Attribute {
		/// <summary>
		///		Gets the name of the RPC procedure.
		/// </summary>
		/// <value>
		///		The name of the RPC procedure.
		///		If the value is <c>null</c>, empty or consisted by whitespace characters only, the qualified type name will be used.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		///		Gets or sets the version of the RPC procedure.
		/// </summary>
		/// <value>
		///		The version of the RPC procedure.
		/// </value>
		public int Version { get; set; }

		/// <summary>
		///		Initializes a new instance of the <see cref="MessagePackRpcServiceContractAttribute"/> class.
		/// </summary>
		public MessagePackRpcServiceContractAttribute() { }

		internal string ToServiceId(Type serviceType) {
			return
				ServiceIdentifier.CreateServiceId(
					string.IsNullOrWhiteSpace(Name) ? ServiceIdentifier.TruncateGenericsSuffix(serviceType.Name) : Name,
					Version
				);
		}
	}
}
using MsgPack.Serialization;
using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Net;

namespace MsgPack.Rpc.Core.Client {
	/// <summary>
	///		Acts as dynamic MessagePack RPC proxy object.
	/// </summary>
	/// <remarks>
	///		This object takes naming convention to interpret target method as following:
	///		<list type="bullet">
	///			<item>
	///				<para>
	///					If the method name starts with "Begin" 
	///					and its arguments contains <see cref="AsyncCallback"/>( or <see cref="Action{T}"/> of <see cref="IAsyncResult"/>) at just before tail,
	///					invoking method is considered as "Begin" call of the target method,
	///					and the rest of method name is actual neme of the remote method.
	///				</para>
	///				<para>
	///					For example, if you type <c>d.BeginFoo(arg1, callback, state)</c> then <c>Foo</c> with 1 argument (<c>arg1</c>) will be invoked,
	///					and <see cref="IAsyncResult"/> will be returned.
	///				</para>
	///				<para>
	///					Note that the method name is equal to "Begin",
	///					or arguments signature is not match,
	///					this will be considered as synchronous method invocation for the remote method whose name is "Begin".
	///				</para>
	///			</item>
	///			<item>
	///				<para>
	///					If the method name starts with "End" 
	///					and its arguments contains <see cref="AsyncCallback"/>( or <see cref="Action{T}"/> of <see cref="IAsyncResult"/>) at just before tail,
	///					invoking method is considered as "End" call of the target method,
	///					and the rest of method name is just ignored.
	///				</para>
	///				<para>
	///					For example, if you type <c>d.EndFoo(ar)</c> then the return value will be returned.
	///				</para>
	///				<para>
	///					Note that the method name is equal to "End",
	///					or arguments signature is not match,
	///					this will be considered as synchronous method invocation for the remote method whose name is "End".
	///				</para>
	///			</item>
	///			<item>
	///				<para>
	///					If the method name ends with "Async" 
	///					and its arguments are not empty
	///					invoking method is considered as "Async" call of the target method and the last argument is considered as <c>asyncState</c>,
	///					and the rest of method name is actual neme of the remote method.
	///				</para>
	///				<para>
	///					For example, if you type <c>d.FooAsync(arg1, state)</c> then <c>Foo</c> with 1 argument (<c>arg1</c>) will be invoked,
	///					and <see cref="System.Threading.Tasks.Task{T}"/> of <see cref="MessagePackObject"/> will be returned.
	///				</para>
	///				<para>
	///					Note that the method name is equal to "Async",
	///					or arguments signature is not match,
	///					this will be considered as synchronous method invocation for the remote method whose name is "Async".
	///				</para>
	///			</item>
	///			<item>
	///				Else, the remote method is invoke as specified name.
	///			</item>
	///		</list>
	///		<note>
	///			Every method always called as "Request" message, not "Notification".
	///			When you have to use "Notification" message, use <see cref="RpcClient"/> directly.
	///		</note>
	/// </remarks>
	public sealed class DynamicRpcProxy : DynamicObject, IDisposable {
		readonly RpcClient client;

		/// <summary>
		///		Initializes a new instance of the <see cref="DynamicRpcProxy"/> class.
		/// </summary>
		/// <param name="client">An underlying <see cref="RpcClient"/>.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="client"/> is <c>null</c>.
		/// </exception>
		public DynamicRpcProxy(RpcClient client) {
			Contract.EndContractBlock();

			this.client = client ?? throw new ArgumentNullException(nameof(client));
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="DynamicRpcProxy"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public DynamicRpcProxy(EndPoint targetEndPoint)
			: this(new RpcClient(targetEndPoint)) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="DynamicRpcProxy"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="configuration">
		///		A <see cref="RpcClientConfiguration"/> which contains protocol information etc.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public DynamicRpcProxy(EndPoint targetEndPoint, RpcClientConfiguration configuration)
			: this(new RpcClient(targetEndPoint, configuration)) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="DynamicRpcProxy"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="serializationContext">
		///		A <see cref="SerializationContext"/> to hold serializers.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public DynamicRpcProxy(EndPoint targetEndPoint, SerializationContext serializationContext)
			: this(new RpcClient(targetEndPoint, serializationContext)) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="DynamicRpcProxy"/> class.
		/// </summary>
		/// <param name="targetEndPoint">
		///		<see cref="EndPoint"/> for the target.
		/// </param>
		/// <param name="configuration">
		///		A <see cref="RpcClientConfiguration"/> which contains protocol information etc.
		/// </param>
		/// <param name="serializationContext">
		///		A <see cref="SerializationContext"/> to hold serializers.
		///	</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetEndPoint"/> is <c>null</c>.
		/// </exception>
		public DynamicRpcProxy(EndPoint targetEndPoint, RpcClientConfiguration configuration, SerializationContext serializationContext)
			: this(new RpcClient(targetEndPoint, configuration, serializationContext)) { }

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			client.Dispose();
		}

		/// <summary>
		///		Provides the implementation for operations that invoke a member. 
		/// </summary>
		/// <param name="binder">
		///		Provides information about the dynamic operation. 
		/// </param>
		/// <param name="args">
		///		The arguments that are passed to the object member during the invoke operation.
		/// </param>
		/// <param name="result">
		///		The result of the member invocation.
		/// </param>
		/// <returns>
		///		This method always returns <c>true</c> because client cannot determine whether the remote server actually implements specified method or not ultimately.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="binder"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		///		For detailed information, see the <strong>remarks</strong> section of the type description.
		/// </remarks>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
			if (binder == null) {
				throw new ArgumentNullException(nameof(binder));
			}

			if (binder.Name.StartsWith("Begin", binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
				&& binder.Name.Length > "Begin".Length
				&& args.Length >= 2) {
				var asAsyncCallback = args[^2] as AsyncCallback;
				if (args[^2] == null || asAsyncCallback != null || asAsyncCallback != null) {
					var realArgs = new object[args.Length - 2];
					Array.ConstrainedCopy(args, 0, realArgs, 0, args.Length - 2);
					if (asAsyncCallback == null && args[^2] is Action<IAsyncResult> asAction) {
						asAsyncCallback = ar => asAction(ar);
					}
					result = client.BeginCall(binder.Name.Substring("Begin".Length), realArgs, asAsyncCallback, args[^1]);
					return true;
				}
			}
			else if (binder.Name.StartsWith("End", binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
				&& binder.Name.Length > "End".Length
				&& args.Length == 1) {
				if (args[0] is IAsyncResult ar) {
					result = client.EndCall(ar);
					return true;
				}
			}
			else if (binder.Name.EndsWith("Async", binder.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
				&& binder.Name.Length > "Async".Length
				&& args.Length >= 1) {
				var realArgs = new object[args.Length - 1];
				Array.ConstrainedCopy(args, 0, realArgs, 0, args.Length - 1);
				result = client.CallAsync(binder.Name.Remove(binder.Name.Length - "Async".Length), realArgs, args[^1]);
				return true;
			}

			result = client.Call(binder.Name, args);
			return true;
		}
	}
}

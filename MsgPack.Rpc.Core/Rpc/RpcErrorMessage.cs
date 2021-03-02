using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Represents MsgPack-RPC error instance.
	/// </summary>
	public struct RpcErrorMessage : IEquatable<RpcErrorMessage> {
		/// <summary>
		///		Gets the instance which represents success (that is, not error.)
		/// </summary>
		/// <value>
		///		The instance which represents success (that is, not error.)
		/// </value>
		public static RpcErrorMessage Success => new RpcErrorMessage();

		/// <summary>
		///		Get the value whether this instance represents success.
		/// </summary>
		/// <value>
		///		If this instance represents success then true.
		/// </value>
		public bool IsSuccess => _error == null;

		readonly RpcError _error;

		/// <summary>
		///		Get error information for this error.
		/// </summary>
		/// <value>
		///		Error information for this error.
		/// </value>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsSuccess"/> is true.
		/// </exception>
		public RpcError Error {
			get {
				if (_error == null) {
					throw new InvalidOperationException("Operation success.");
				}

				return _error;
			}
		}

		readonly MessagePackObject _detail;

		/// <summary>
		///		Get detailed error information for this error.
		/// </summary>
		/// <value>
		///		Detailed error information for this error.
		/// </value>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsSuccess"/> is true.
		/// </exception>
		public MessagePackObject Detail {
			get {
				if (_error == null) {
					throw new InvalidOperationException("Operation success.");
				}

				return _detail;
			}
		}

		/// <summary>
		///		Initializes new instance.
		/// </summary>
		/// <param name="error">Error information of the error.</param>
		/// <param name="detail">Unpacked detailed information of the error which was occurred in remote endpoint.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="error"/> is null.
		/// </exception>
		public RpcErrorMessage(RpcError error, MessagePackObject detail) {
			Contract.EndContractBlock();

			_error = error ?? throw new ArgumentNullException(nameof(error));
			_detail = detail;
		}

		/// <summary>
		///		Initializes new instance.
		/// </summary>
		/// <param name="error">The metadata of the error.</param>
		/// <param name="description">The description of the error which was occurred in local.</param>
		/// <param name="debugInformation">The detailed debug information of the error which was occurred in local.</param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="error"/> is <c>null</c>.
		/// </exception>
		public RpcErrorMessage(RpcError error, string description, string debugInformation) {
			Contract.EndContractBlock();

			_error = error ?? throw new ArgumentNullException(nameof(error));

			var data = new MessagePackObjectDictionary(2);
			if (description != null) {
				data.Add(RpcException.messageKeyUtf8, description);
			}

			if (debugInformation != null) {
				data.Add(RpcException.debugInformationKeyUtf8, debugInformation);
			}

			_detail = new MessagePackObject(data);
		}

		/// <summary>
		///		Determines whether the specified <see cref="object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		///		<c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) {
			if (obj is null) {
				return false;
			}

			if (!(obj is RpcErrorMessage)) {
				return false;
			}

			return Equals((RpcErrorMessage)obj);
		}

		/// <summary>
		///		Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">
		///		An object to compare with this object.
		/// </param>
		/// <returns>
		///		<x>true</x> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(RpcErrorMessage other) {
			return _error == other._error && _detail == other._detail;
		}

		/// <summary>
		///		Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///		A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() {
			return (_error == null ? 0 : _error.GetHashCode()) ^ _detail.GetHashCode();
		}

		/// <summary>
		///		Returns string representation of this error.
		/// </summary>
		/// <returns>
		///		String representation of this error.
		/// </returns>
		public override string ToString() {
			if (IsSuccess) {
				return string.Empty;
			}
			else {
				return string.Format(CultureInfo.CurrentCulture, "{{ \"ID\" : \"{0}\", \"Code\" : {1}, \"Detail\" : {2} }}", _error.Identifier, _error.ErrorCode, _detail);
			}
		}

		/// <summary>
		///		Get <see cref="RpcException"/> which corresponds to this error.
		/// </summary>
		/// <returns><see cref="RpcException"/> which corresponds to this error.</returns>
		/// <exception cref="InvalidOperationException">
		///		<see cref="IsSuccess"/> is true.
		/// </exception>
		public RpcException ToException() {
			if (IsSuccess) {
				throw new InvalidOperationException("Operation has been succeeded.");
			}

			return _error.ToException(_detail);
		}

		/// <summary>
		///		Determines whether two <see cref="RpcErrorMessage"/> instances have the same value. 
		/// </summary>
		/// <param name="left">A <see cref="RpcErrorMessage"/> instance to compare with <paramref name="right"/>.</param>
		/// <param name="right">A <see cref="RpcErrorMessage"/> instance to compare with <paramref name="left"/>.</param>
		/// <returns>
		///		<c>true</c> if the <see cref="RpcErrorMessage"/> instances are equivalent; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator ==(RpcErrorMessage left, RpcErrorMessage right) {
			return left.Equals(right);
		}

		/// <summary>
		///		Determines whether two <see cref="RpcErrorMessage"/> instances do not have the same value. 
		/// </summary>
		/// <param name="left">A <see cref="RpcErrorMessage"/> instance to compare with <paramref name="right"/>.</param>
		/// <param name="right">A <see cref="RpcErrorMessage"/> instance to compare with <paramref name="left"/>.</param>
		/// <returns>
		///		<c>true</c> if the <see cref="RpcErrorMessage"/> instances are not equivalent; otherwise, <c>false</c>.
		/// </returns>
		public static bool operator !=(RpcErrorMessage left, RpcErrorMessage right) {
			return !left.Equals(right);
		}
	}
}

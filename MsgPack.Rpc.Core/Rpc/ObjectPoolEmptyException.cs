using System;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Throwsn when the object pool with <see cref="ExhausionPolicy.ThrowException"/> is empty at borrowing.
	/// </summary>
	[Serializable]
	public sealed class ObjectPoolEmptyException : Exception {
		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolEmptyException"/> class with a default message.
		/// </summary>
		public ObjectPoolEmptyException() : this(null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolEmptyException"/> class with a specified error message. 
		/// </summary>
		/// <param name="message">The message to describe the error.</param>
		public ObjectPoolEmptyException(string message) : this(message, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolEmptyException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="message">The message to describe the error.</param>
		/// <param name="innerException">
		///		The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.
		/// </param>
		public ObjectPoolEmptyException(string message, Exception innerException) : base(message ?? "The object pool is empty.", innerException) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolEmptyException"/> class with serialized data.
		/// </summary>
		/// <param name="info">
		///		The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.
		///	</param>
		/// <param name="context">
		///		The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.
		///	</param>
		/// <exception cref="T:System.ArgumentNullException">
		///		<paramref name="info"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		///		THe class name is <c>null</c>,
		///		or <see cref="P:System.Exception.HResult"/> is zero.
		///	</exception>
		ObjectPoolEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

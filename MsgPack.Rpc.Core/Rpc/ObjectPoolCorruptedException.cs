﻿using System;
using System.Runtime.Serialization;

namespace MsgPack.Rpc.Core {
	/// <summary>
	///		Thrown when object pool is in corrupted state.
	/// </summary>
	[Serializable]
	public sealed class ObjectPoolCorruptedException : Exception {
		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolCorruptedException"/> class with a default message.
		/// </summary>
		public ObjectPoolCorruptedException() : this(null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolCorruptedException"/> class with a specified error message. 
		/// </summary>
		/// <param name="message">The message to describe the error.</param>
		public ObjectPoolCorruptedException(string message) : this(message, null) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolCorruptedException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
		/// </summary>
		/// <param name="message">The message to describe the error.</param>
		/// <param name="inner">
		///		The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.
		/// </param>
		public ObjectPoolCorruptedException(string message, Exception inner) : base(message ?? "Object pool may be corrupted.", inner) { }

		/// <summary>
		///		Initializes a new instance of the <see cref="ObjectPoolCorruptedException"/> class with serialized data.
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
		ObjectPoolCorruptedException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}

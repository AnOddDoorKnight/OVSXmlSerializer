namespace OVS.XmlSerialization.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;

	/// <summary>
	/// An exception that states which object has failed to serialize. Can be stacked
	/// multiple times to specifically pin-point where.
	/// </summary>
	[Serializable]
	public class SerializationFailedException : Exception
	{
		/// <summary>
		/// Initializes a new instance that indicates that serialization has failed.
		/// </summary>
		public SerializationFailedException() { }
		/// <summary>
		/// Initializes a new instance that indicates that serialization has failed,
		/// along with a message.
		/// </summary>
		public SerializationFailedException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance that indicates that serialization has failed,
		/// along with a message and an inner exception.
		/// </summary>
		public SerializationFailedException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// No idea what this does its here because the IDE said so
		/// </summary>
		protected SerializationFailedException(
			SerializationInfo info,
			StreamingContext context) : base(info, context) { }
	}
}

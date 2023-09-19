namespace OVSXmlSerializer.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;

	[Serializable]
	public class SerializationFailedException : Exception
	{
		public SerializationFailedException() { }
		public SerializationFailedException(string message) : base(message) { }
		public SerializationFailedException(string message, Exception inner) : base(message, inner) { }
		protected SerializationFailedException(
			SerializationInfo info,
			StreamingContext context) : base(info, context) { }
	}
}

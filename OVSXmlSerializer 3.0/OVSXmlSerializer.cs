namespace OVSXmlSerializer
{
	using System;

	public class OVSXmlSerializer<T>
	{
		/// <summary>
		/// The configuration that changes the behaviour of the serializer.
		/// </summary>
		public OVSConfig Config { get; protected set; }
	}
}

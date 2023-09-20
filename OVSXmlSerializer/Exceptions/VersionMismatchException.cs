namespace OVSXmlSerializer.Exceptions
{
	using System;
	using Internals;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// An error that appears when a version checker is mis-matched. Mostly used
	/// by <see cref="OVSXmlReader{T}"/>
	/// </summary>
	public class VersionMismatchException : Exception
	{
		/// <summary>
		/// Initializes a new instance simply indicating that it is mismatched.
		/// </summary>
		public VersionMismatchException() : base("It does not match the current version!") { }
		/// <summary>
		/// Initializes a new instance basing off of the field name or class name indicating
		/// that the document version is mismatched.
		/// </summary>
		/// <param name="target">The document's targeted version.</param>
		/// <param name="current">The current version that configuration is on.</param>
		public VersionMismatchException(Version target, Version current) : base($"The document's version is {target}, which doesn't match the current version of {current}!") { }
	}
}

namespace OVSSerializer.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// An error that appears when an object doesn't have a parameterless constructor.
	/// Only present in classes or reference types, value types like structs are
	/// excepted.
	/// </summary>
	public class ParameteredOnlyException : Exception
	{
		/// <summary>
		/// Initializes a new instance simply indicating that it has an empty constructor.
		/// </summary>
		public ParameteredOnlyException() : base("It does not have an empty constructor!") { }
		/// <summary>
		/// Initializes a new instance basing off of the field name or class name indicating that it has an empty constructor.
		/// </summary>
		public ParameteredOnlyException(string objectName) : base($"{objectName} does not have an empty constructor!") { }
	}
}

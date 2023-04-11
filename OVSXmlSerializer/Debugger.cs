namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A class thats stores and handles debug messages like <see cref="OVSXmlDebugLine"/>.
	/// </summary>
	public class OVSXmlLogger
	{
		/// <summary>
		/// All the lines that has been logged on creation of the class.
		/// </summary>
		public IReadOnlyList<OVSXmlDebugLine> DebugLines => debugLines;
		private List<OVSXmlDebugLine> debugLines = new List<OVSXmlDebugLine>();
		/// <summary>
		/// An event that is called every time a new message is made.
		/// </summary>
		public event Action<OVSXmlDebugLine> Log;

		/// <summary>
		/// Creates a new instance to use for writers and readers.
		/// </summary>
		public OVSXmlLogger()
		{
			Log = (line) => debugLines.Add(line);
		}
		/// <summary>
		/// Prints all lines to console.
		/// </summary>
		public void PrintToConsole()
		{
			Console.WriteLine(string.Join("\n", DebugLines));
		}

		internal void InvokeMessage(OVSXmlDebugLine line) => Log.Invoke(line);
		internal void InvokeMessage(string source, string message) => InvokeMessage(new OVSXmlDebugLine(source, message));
		internal void InvokeMessage(string message) => InvokeMessage(new OVSXmlDebugLine(message));
	}

	/// <summary>
	/// Data about a specific debug line.
	/// </summary>
	public struct OVSXmlDebugLine
	{
		/// <summary>
		/// The raw message.
		/// </summary>
		public readonly string message;
		/// <summary>
		/// The source of the message; Usually reader or writer.
		/// </summary>
		public readonly string source;
		/// <summary>
		/// When the message was created.
		/// </summary>
		public readonly DateTime created;

		/// <summary>
		/// Creates a new instance of <see cref="OVSXmlDebugLine"/>
		/// </summary>
		/// <param name="source">The source of the message.</param>
		/// <param name="message">The message itself</param>
		public OVSXmlDebugLine(string source, string message)
		{
			created = DateTime.Now;
			this.message = message;
			this.source = source;
		}
		/// <summary>
		/// Creates a new instance of <see cref="OVSXmlDebugLine"/>
		/// </summary>
		/// <param name="message">The message itself</param>
		public OVSXmlDebugLine(string message)
		{
			created = DateTime.Now;
			this.message = message;
			source = string.Empty;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(source))
				return $"[{created}] {message}";
			return $"[{created}/{source}] {message}";
		}
	}
}
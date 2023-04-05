namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;

	public class OVSXmlLogger
	{
		public IReadOnlyList<OVSXmlDebugLine> DebugLines => debugLines;
		private List<OVSXmlDebugLine> debugLines = new List<OVSXmlDebugLine>();
		public event Action<OVSXmlDebugLine> Log;

		public OVSXmlLogger()
		{
			Log = (line) => debugLines.Add(line);
		}

		internal void InvokeMessage(OVSXmlDebugLine line) => Log.Invoke(line);
		internal void InvokeMessage(string source, string message) => InvokeMessage(new OVSXmlDebugLine(source, message));
		internal void InvokeMessage(string message) => InvokeMessage(new OVSXmlDebugLine(message));
	}

	public struct OVSXmlDebugLine
	{
		public readonly string message;
		public readonly string source;
		public readonly DateTime created;

		public OVSXmlDebugLine(string source, string message)
		{
			created = DateTime.Now;
			this.message = message;
			this.source = source;
		}
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
namespace OVS.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;



	/// <summary>
	/// A highly complicated 'string' that purely only cares about the path.
	/// Do not use this as an actual class that can do stuff!
	/// </summary>
	/// <remarks> 
	/// Prints the path that can be used and is the same as <see cref="Normalized"/>.
	/// <para>
	/// Shoutout to that one guy i took this from and forgot who made this, thanks
	/// for the help ;)
	/// </para>
	/// </remarks>
	public readonly struct OSPath
	{
		public static OSPath Combine(string left, params string[] extras) => Combine(new OSPath(left), extras);
		public static OSPath Combine(OSPath result, params string[] extras)
		{
			for (int i = 0; i < extras.Length; i++)
				result += extras[i];
			return result;
		}
		/// <summary>
		/// A path that leads to nothing.
		/// </summary>
		public static readonly OSPath Empty = new OSPath("");
		/// <summary>
		/// Provides a platform-specific character used to separate directory 
		/// levels in a path string that reflects a hierarchical file system 
		/// organization.
		/// </summary>
		public static char DirectorySeparatorChar => Path.DirectorySeparatorChar;
		/// <summary>
		/// Is the currently selected OS is windows.
		/// </summary>
		public static bool IsWindows => DirectorySeparatorChar == '\\';

		/// <summary>
		/// Creates a new path with the specified file path.
		/// </summary>
		public OSPath(string text)
		{
			Text = text.Trim();
		}

		public static implicit operator OSPath(string text) => text == null ? Empty : new OSPath(text);
		public static implicit operator string(OSPath path) => path.Normalized;

		/// <summary>
		/// Gets <see cref="Normalized"/> as a full path.
		/// </summary>
		public override string ToString() => Normalized;

		private string Text { get; }

		/// <summary>
		/// Gets the normalized full path.
		/// </summary>
		public string Normalized => IsWindows ? Windows : Unix;
		/// <summary>
		/// Gets the path for windows platforms.
		/// </summary>
		public string Windows => Text.Replace('/', '\\');
		/// <summary>
		/// Gets the path for unix platforms like Linux or Mac-OS
		/// </summary>
		public string Unix => Simplified.Text.Replace('\\', '/');

		public OSPath Relative => Simplified.Text.TrimStart('/', '\\');
		public OSPath Absolute => IsAbsolute ? this : "/" + Relative;

		public bool IsAbsolute => IsRooted || HasVolume;
		public bool IsRooted => Text.Length >= 1 && (Text[0] == '/' || Text[0] == '\\');
		public bool HasVolume => Text.Length >= 2 && Text[1] == ':';
		public OSPath Simplified => HasVolume ? Text.Substring(2) : Text;

		/// <summary>
		/// The path being split with directory separators.
		/// </summary>
		public string[] Split => Normalized.Split(DirectorySeparatorChar);
		/// <summary>
		/// The parent of the file or directory.
		/// </summary>
		public OSPath Parent => Path.GetDirectoryName(Text);

		/// <summary>
		/// If the path is in a certain directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool Contains(OSPath path) =>
			Normalized.StartsWith(path);

		public static OSPath operator +(OSPath left, OSPath right) =>
			new OSPath(Path.Combine(left, right.Relative));

		public static OSPath operator -(OSPath left, OSPath right) =>
			left.Contains(right)
			? new OSPath(left.Normalized.Substring(right.Normalized.Length)).Relative
			: left;

		/// <summary>
		/// The length of the path.
		/// </summary>
		public int Length => ToString().Length;
	}
}

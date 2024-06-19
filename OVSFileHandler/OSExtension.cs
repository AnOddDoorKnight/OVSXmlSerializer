namespace OVS.IO
{
	using System;
	using SysPath = System.IO.Path;

	/// <summary>
	/// A class that handles extensions. Exists to easily modify extensions easily.
	/// </summary>
	public static class OSExtension
	{
		/// <summary>
		/// If the filePath has an extension.
		/// </summary>
		/// <param name="inputPath"> The file path to check.</param>
		public static bool HasExtension(in string inputPath) =>
			HasExtension(inputPath, out _);
		/// <summary>
		/// If the filePath has an extension.
		/// </summary>
		/// <param name="inputPath"> The file path to check.</param>
		/// <param name="startExtension"> The index where the extension starts.</param>
		public static bool HasExtension(in string inputPath, out int startExtension)
		{
			string trimmed = inputPath.Trim();
			if (trimmed.EndsWith(OSPath.DirectorySeparatorChar.ToString()))
			{
				startExtension = -1;
				return false;
			}
			startExtension = trimmed.LastIndexOf('.');
			return startExtension != -1;
		}
		/// <summary>
		/// Removes the extension from input file path, assuming it exists.
		/// </summary>
		/// <param name="input"> The file path to remove the extension from. </param>
		public static string RemoveExtension(in string input)
		{
			string trimmed = input.Trim();
			if (HasExtension(trimmed, out int index))
				trimmed = trimmed.Remove(index);
			return trimmed;
		}
		/// <summary>
		/// Adds a new extension to the given file path, but removing the old extension
		/// if it exists.
		/// </summary>
		/// <param name="input">The given file path.</param>
		/// <param name="extension">The new extension.</param>
		/// <exception cref="InvalidCastException">If it is not a file path.</exception>
		public static string AddExtension(string input, string extension)
		{
			string trimmed = input.Trim();
			if (trimmed.EndsWith(OSPath.DirectorySeparatorChar.ToString()))
				throw new InvalidCastException($"'{input}' is not a file path!");
			trimmed = RemoveExtension(trimmed);
			if (!extension.StartsWith("."))
				extension = '.' + extension;
			string output = trimmed + extension;
			return output;
		}




		//private string extension;
		//private readonly OSFile pairedFile;
		//private OSPath FullPath { get => pairedFile.FullPath; set => pairedFile.FullPath = value; }
		//internal OSExtension(OSFile pairedFile)
		//{
		//	this.pairedFile = pairedFile;
		//	extension = SysPath.GetExtension(pairedFile.FullPath);
		//}
		//
		//public bool Exists => !string.IsNullOrEmpty(extension);
		//
		//public void Set(string newExtension)
		//{
		//	string fullPath = FullPath.ToString();
		//	if (Exists)
		//		FullPath = fullPath.Remove(fullPath.IndexOf(extension)) + newExtension;
		//	else
		//		FullPath = fullPath + newExtension;
		//	extension = newExtension;
		//}
		//
		//public override string ToString() => extension;
		//public string NameWithoutExtension() => SysPath.GetFileNameWithoutExtension(FullPath);
	}
}

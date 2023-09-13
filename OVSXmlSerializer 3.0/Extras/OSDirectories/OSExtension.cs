namespace OVSXmlSerializer.IO
{
	using System;
	using SysPath = System.IO.Path;

	/*
	public sealed class OSExtension
	{
		public static bool HasExtension(in string inputPath) =>
			HasExtension(inputPath, out _);
		public static bool HasExtension(in string inputPath, out int startExtension)
		{
			string trimmed = inputPath.Trim();
			if (trimmed.EndsWith(OSPath.DirectorySeparatorChar))
			{
				startExtension = -1;
				return false;
			}
			startExtension = trimmed.LastIndexOf('.');
			return startExtension != -1;
		}
		public static string RemoveExtension(in string input)
		{
			string trimmed = input.Trim();
			if (HasExtension(trimmed, out int index))
				trimmed = trimmed.Remove(index);
			return trimmed;
		}
		public static string AddExtension(string input, string extension)
		{
			string trimmed = input.Trim();
			if (trimmed.EndsWith(OSPath.DirectorySeparatorChar))
				throw new InvalidCastException($"'{input}' is not a file path!");
			trimmed = RemoveExtension(trimmed);
			if (!extension.StartsWith('.'))
				extension = '.' + extension;
			string output = trimmed + extension;
			return output;
		}




		private string extension;
		private readonly OSFile pairedFile;
		private OSPath FullPath { get => pairedFile.FullPath; set => pairedFile.FullPath = value; }
		internal OSExtension(OSFile pairedFile)
		{
			this.pairedFile = pairedFile;
			extension = SysPath.GetExtension(pairedFile.FullPath);
		}

		public bool Exists => !string.IsNullOrEmpty(extension);

		public void Set(string newExtension)
		{
			string fullPath = FullPath.ToString();
			if (Exists)
				FullPath = fullPath.Remove(fullPath.IndexOf(extension)) + newExtension;
			else
				FullPath = fullPath + newExtension;
			extension = newExtension;
		}

		public override string ToString() => extension;
		public string NameWithoutExtension() => SysPath.GetFileNameWithoutExtension(FullPath);
	}
	*/
}

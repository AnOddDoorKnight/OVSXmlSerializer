namespace OVS.IO
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.IO.Compression;

	/// <summary>
	/// Helper class for <see cref="OSFile"/>s to easily extract and compress.
	/// </summary>
	public static class ZipUtility
	{
		/// <summary>
		/// If the file is a zip file or not.
		/// </summary>
		public static bool IsZipFile(this OSFile file) => file.Extension == ".zip";
		/// <summary>
		/// Easily unzips using the existing <see cref="System.IO.Compression"/>
		/// library for OSFiles.
		/// </summary>
		/// <param name="file">The assumed zip file</param>
		/// <param name="targetDirectory">To put the contents too.</param>
		/// <exception cref="InvalidCastException"></exception>
		public static void UnzipTo(this OSFile file, OSDirectory targetDirectory)
		{
			if (file.IsZipFile())
				throw new InvalidCastException($"'{file.FullPath}' is not a zip file!");
			ZipFile.ExtractToDirectory(file.FullPath, targetDirectory.FullPath);
		}
		/// <summary>
		/// Since some zip files will have a single folder in them, this simply removes
		/// that and puts the contents into the <paramref name="targetDirectory"/> instead.
		/// </summary>
		/// <param name="targetDirectory">The contents to store into AND the single folder to 'liberate'.</param>
		/// <returns>If it actually needed to run the code to move the objects around.</returns>
		public static bool TryRemoveSingularLayers(this OSDirectory targetDirectory)
		{
			if (!targetDirectory.Exists)
				return false;
			// Ensure that it has 1 item or less 
			IEnumerable<OSSystemInfo> children = targetDirectory.EnumerateItems();
			using (IEnumerator<OSSystemInfo> enumerator = children.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
					if (i > 1)
						return false;
			// Moving contents to location
			OSDirectory takeContentsFrom = children.SingleOrDefault() as OSDirectory;
			if (takeContentsFrom is null)
				return false;
			OSSystemInfo[] movingItems = takeContentsFrom.GetItems();
			for (int i = 0; i < movingItems.Length; i++)
				movingItems[i].MoveTo(targetDirectory);
			takeContentsFrom.Delete();
			return true;
		}
		/// <summary>
		/// Simply uses a .zip file to compress the contents.
		/// </summary>
		/// <param name="contents">The contents of the archive that is created.</param>
		/// <param name="to">Where to save the archive file</param>
		/// <param name="nameWithoutExtension">The name of the .zip archive</param>
		/// <param name="level"></param>
		/// <returns>The archived .zip file.</returns>
		public static OSFile ZipContentsTo(this OSDirectory contents, OSDirectory to, string nameWithoutExtension = null, CompressionLevel level = CompressionLevel.Fastest)
		{
			if (string.IsNullOrWhiteSpace(nameWithoutExtension))
				nameWithoutExtension = contents.Name;
			OSFile archive = to.GetFile($"{nameWithoutExtension}.zip");
			ZipFile.CreateFromDirectory(contents.FullPath, archive.FullPath, level, false);
			return archive;
		}
	}
}
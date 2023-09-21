namespace OVSSerializer.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using SysPath = System.IO.Path;
	using System.Threading.Tasks;

	/// <summary>
	/// Directory info that considers unix-based and windows systems.
	/// </summary>
	public sealed class OSDirectory : OSSystemInfo, IEquatable<OSDirectory>, IEquatable<string>
	{
		public static explicit operator DirectoryInfo(OSDirectory file) => new DirectoryInfo(file.FullPath);
		public static explicit operator OSDirectory(DirectoryInfo file) => new OSDirectory(file);

#if UNITY_2017_1_OR_NEWER
		/// <summary>
		/// Gets the local appdata folder for saving settings, configs, etc.
		/// </summary>
		public static OSDirectory PersistentData
		{
			get
			{
				if (m_persist is null)
					m_persist = new OSDirectory(UnityEngine.Application.persistentDataPath);
				return m_persist;
			}
		}
		private static OSDirectory m_persist;


		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static OSDirectory StreamingAssets
		{
			get
			{
				if (m_streaming is null)
					m_streaming = new OSDirectory(UnityEngine.Application.streamingAssetsPath);
				return m_streaming;
			}
		}
		private static OSDirectory m_streaming;
#endif

		/// <summary>
		/// Creates a new instance based off of an existing info directory.
		/// </summary>
		public OSDirectory(DirectoryInfo info) : this(info.FullName) { }
		/// <summary>
		/// Copies the directory as a separate reference type.
		/// </summary>
		public OSDirectory(OSDirectory source) : base(source.FullPath) { }
		/// <summary>
		/// Initializes a directory info from a string path.
		/// </summary>
		public OSDirectory(OSPath fullPath) : base(fullPath) { }

		/// <summary>
		/// Gets the name of the directory, setting it will only change the class
		/// data.
		/// </summary>
		public override string Name
		{
			get
			{
				string fullPath = this.FullPath.ToString();
				fullPath = fullPath.Remove(fullPath.Length - 1);
				fullPath = fullPath.Substring(fullPath.LastIndexOf(SysPath.DirectorySeparatorChar) + 1);
				return fullPath;
			}
			set
			{
				string fullPath = this.FullPath.ToString();
				string oldName = Name;
				fullPath = fullPath.Remove(fullPath.Length - 1 - oldName.Length, oldName.Length);
				this.FullPath = fullPath.Insert(fullPath.Length - 1, value);
			}
		}

		/// <summary>
		/// If the directory exists or not. Setting it creates a new empty or 
		/// deletes it entirely.
		/// </summary>
		public override bool Exists
		{
			get => Directory.Exists(FullPath);
			set
			{
				if (Exists == value)
					return;
				if (value)
					Create();
				else
					Delete();
			}
		}

		/// <summary>
		/// Creates all directories and subdirectories in the path unless they
		/// already exist.
		/// </summary>
		public void Create() => Directory.CreateDirectory(FullPath);

		/// <summary>
		/// Creates subdirectories.
		/// </summary>
		/// <param name="subDirectories">The subdirectory names. </param>
		/// <returns>The final directory listed in <paramref name="subDirectories"/>.</returns>
		public OSDirectory GetSubdirectories(params string[] subDirectories) 
			=> GetSubdirectories(true, subDirectories);
		/// <summary>
		/// Creates subdirectories.
		/// </summary>
		/// <param name="createDirectories">If the directories should be created.</param>
		/// <param name="subDirectories">The subdirectory names. </param>
		/// <returns>The final directory listed in <paramref name="subDirectories"/>.</returns>
		public OSDirectory GetSubdirectories(bool createDirectories, params string[] subDirectories)
		{
			OSDirectory output = this;
			for (int i = 0; i < subDirectories.Length; i++)
			{
				output = new OSDirectory(SysPath.Combine(FullPath, subDirectories[i]));
				if (createDirectories && !output.Exists)
					output.Create();
			}
			return output;
		}

		/// <summary>
		/// Enumerates through directories that is in the current directory.
		/// </summary>
		public IEnumerable<OSDirectory> EnumerateDirectories()
		{
			return	from fullPath in Directory.EnumerateDirectories(FullPath)
					select new OSDirectory(fullPath);
		}
		/// <summary>
		/// Gets all directories into an array.
		/// </summary>
		public OSDirectory[] GetDirectories()
		{
			string[] directories = Directory.GetDirectories(FullPath);
			OSDirectory[] result = new OSDirectory[directories.Length];
			for (int i = 0; i < directories.Length; i++)
				result[i] = new OSDirectory(directories[i]);
			return result;
		}

		/// <summary>
		/// Enumerates through files that is in the current directory.
		/// </summary>
		public IEnumerable<OSFile> EnumerateFiles()
		{
			return	from fullPath in Directory.EnumerateFiles(FullPath)
					select new OSFile(fullPath);
		}
		/// <summary>
		/// Gets all files into an array.
		/// </summary>
		public OSFile[] GetFiles()
		{
			string[] files = Directory.GetFiles(FullPath);
			OSFile[] result = new OSFile[files.Length];
			for (int i = 0; i < files.Length; i++)
				result[i] = new OSFile(files[i]);
			return result;
		}

		/// <summary>
		/// Gets all files, and then filters them through a predicate.
		/// </summary>
		public List<OSFile> GetFiles(Predicate<OSFile> filter)
		{
			string[] files = Directory.GetFiles(FullPath);
			List<OSFile> result = new List<OSFile>(files.Length);
			for (int i = 0; i < files.Length; i++)
			{
				var file = new OSFile(files[i]);
				if (filter.Invoke(file))
					result.Add(file);
			}
			return result;
		}
		/// <summary>
		/// Creates new files 
		/// </summary>
		public OSFile[] GetFiles(params string[] fileNamesAndExtensions)
		{
			OSFile[] result = new OSFile[fileNamesAndExtensions.Length];
			for (int i = 0; i < fileNamesAndExtensions.Length; i++)
				result[i] = GetFile(fileNamesAndExtensions[i]);
			return result;
		}
		/// <summary>
		/// Gets some file metadata to manipulate. 
		/// </summary>
		/// <param name="fileNameAndExtension">The file name and its extension of the file.</param>
		/// <returns>The (possibly non-existent) file info.</returns>
		public OSFile GetFile(string fileNameAndExtension)
		{
			return new OSFile(SysPath.Combine(FullPath, fileNameAndExtension));
		}

		/// <summary>
		/// Enumerates and finds an empty slot based on <paramref name="fileName"/>,
		/// adding <c>_{index}</c> between the extension and the name itself (or
		/// just adding it normally if it doesn't have an extension).
		/// </summary>
		/// <param name="fileName">The desired file name.</param>
		/// <param name="alwaysIncludeNumber">
		/// This will always contains a number from 0 and above, instead of starting
		/// at 1 when a copy already exists.
		/// </param>
		/// <returns>A file info that doesn't exist (yet).</returns>
		public OSFile GetFileIncremental(string fileName, bool alwaysIncludeNumber = false)
		{
			HashSet<string> otherNames = new HashSet<string>(EnumerateFiles().Select(fileInfo => fileInfo.Name));

			int extensionIndex = fileName.LastIndexOf('.');
			bool hasExtension = extensionIndex != -1;
			string GetIncrementalFileName(int increment) => hasExtension
				? fileName.Insert(extensionIndex, $"_{increment}")
				: fileName + $"_{increment}";

			if (alwaysIncludeNumber || otherNames.Contains(fileName))
				for (int i = alwaysIncludeNumber ? 0 : 1; true; i++)
				{
					string incrementalName = GetIncrementalFileName(i);
					if (otherNames.Contains(incrementalName))
						continue;
					return GetFile(incrementalName);
				}
			return GetFile(fileName);
		}
		/// <summary>
		/// Deletes the directory and everything in it.
		/// </summary>
		/// <returns>if it used to exist.</returns>
		public override bool Delete()
		{
			bool output = Exists;
			Directory.Delete(FullPath, true);
			return output;
		}

		/// <summary>
		/// Moves the directory and its contents into another directory, by
		/// first copying it and deleting itself afterwards.
		/// </summary>
		/// <param name="intoDirectory">The new parent directory.</param>
		public override void MoveTo(OSDirectory intoDirectory)
		{
			OSDirectory newDir = (OSDirectory)CopyTo(intoDirectory);
			Delete();
			FullPath = newDir.FullPath;
		}
		/// <summary>
		/// Copies the directory and its contents into another directory, by 
		/// creating and destroying after.
		/// </summary>
		/// <param name="intoDirectory">Copy the directory and its contents to.</param>
		/// <returns>The new directory that hopefully contains the same contents.</returns>
		public override OSSystemInfo CopyTo(OSDirectory intoDirectory)
		{
			OSDirectory output = intoDirectory.GetSubdirectories(Name);
			if (Exists)
			{
				OSDirectory[] allDirectories = GetDirectories();
				OSFile[] allFiles = GetFiles();
				for (int i = 0; i < allDirectories.Length; i++)
					allDirectories[i].CopyTo(output);
				for (int i = 0; i < allFiles.Length; i++)
					allDirectories[i].CopyTo(output);
			}
			return output;
		}

		public bool Equals(OSDirectory other)
		{
			return Equals(other.FullPath);
		}

		public bool Equals(string other)
		{
			string left = FullPath.ToString();
			string right = new OSPath(other).ToString();
			return left == right;
		}
	}
}

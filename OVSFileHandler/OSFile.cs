namespace OVS.IO
{
	using System.Collections.Generic;
	using System.IO;
	using SysPath = System.IO.Path;
	using System.Threading.Tasks;
	using System;

	/// <summary>
	/// File info that considers unix-based and windows systems.
	/// </summary>
	public sealed class OSFile : OSSystemInfo, IDisposable
	{
		/// <summary>
		/// Explicitly converts a <see cref="FileInfo"/> to an <see cref="OSFile"/>
		/// </summary>
		public static explicit operator FileInfo(OSFile file) => new FileInfo(file.FullPath);
		/// <summary>
		/// Explicitly converts a <see cref="OSFile"/> to an <see cref="FileInfo"/>
		/// </summary>
		public static explicit operator OSFile(FileInfo file) => new OSFile(file);

		/// <summary>
		/// Creates a metadata or file info via directory or path.
		/// </summary>
		public OSFile(OSPath fullPath) : base(fullPath) { }
		/// <summary>
		/// Copies an OSFile as another reference.
		/// </summary>
		public OSFile(OSFile source) : base(source.FullPath) { }
		/// <summary>
		/// Converts a fileinfo to an OSfile.
		/// </summary>
		public OSFile(FileInfo info) : base(info.FullName) { }

		/// <summary>
		/// The name of the file; Extension included. Use <see cref="OSChanger.Rename(OSFile, string)"/>
		/// to also change the file itself located on the disk.
		/// </summary>
		public override string Name
		{
			get => SysPath.GetFileName(FullPath);
			set
			{
				string nameToReplace = Name;
				string fullPath = FullPath.ToString();
				FullPath = fullPath.Remove(fullPath.IndexOf(nameToReplace)) + value;
			}
		}
		/// <summary>
		/// Gets or sets the extension of the file. Typically starts with <c>'.'</c>
		/// </summary>
		public string Extension
		{
			get => SysPath.GetExtension(FullPath);
			set
			{
				string extensionToReplace = Extension;
				string fullPath = FullPath.ToString();
				if (string.IsNullOrEmpty(extensionToReplace))
					FullPath = fullPath + value;
				else
					FullPath = fullPath.Remove(fullPath.IndexOf(extensionToReplace)) + value;
			}
		}
		/// <summary>
		/// Gets or sets the name that has the extension removed.
		/// </summary>
		public string NameWithoutExtension
		{
			get => SysPath.GetFileNameWithoutExtension(FullPath);
			set
			{
				string nameToReplace = NameWithoutExtension;
				string fullPath = FullPath.ToString();
				int index = fullPath.IndexOf(nameToReplace);
				FullPath = fullPath.Remove(index, nameToReplace.Length).Insert(index, value);
			}
		}
		/// <summary>
		/// If the file actually exists or not. Setting it to <see langword="true"/> creates a new 'text'
		/// file that is empty, or deletes it as <see langword="false"/>.
		/// </summary>
		public override bool Exists
		{
			get => File.Exists(FullPath);
			set
			{
				if (Exists == value)
					return;
				if (value)
					File.WriteAllText(FullPath, "");
				else
					Delete();
			}
		}

		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally.
		/// </summary>
		/// <param name="extension">The extension to change to. </param>
		public void RenameExtension(string extension)
		{
			var newFile = new OSFile(this) { Extension = extension };
			if (this.Exists)
			{
				using (FileStream from = this.OpenRead())
				using (FileStream to = newFile.Create())
					from.CopyTo(to);
				this.Delete();
			}
			FullPath = newFile.FullPath;
		}
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally.
		/// </summary>
		/// <param name="name">The new name of the file.</param>
		public void Rename(string name)
		{
			var newFile = new OSFile(this) { Name = name };
			if (this.Exists)
			{
				using (FileStream from = this.OpenRead())
				using (FileStream to = newFile.Create())
					from.CopyTo(to);
				this.Delete();
			}
			FullPath = newFile.FullPath;
		}
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally. This particular command allows to keep the same extension.
		/// </summary>
		/// <param name="name">The new name of the file, excluding the extension as they are inherited.</param>
		public void RenameWithoutExtension(string name)
		{
			var newFile = new OSFile(this) { NameWithoutExtension = name };
			if (this.Exists)
			{
				FileStream from = this.OpenRead();
				FileStream to = newFile.Create();
				from.CopyTo(to);
				this.Delete();
				from.Dispose();
				to.Dispose();
			}
			FullPath = newFile.FullPath;
		}


		#region Reading/Writing
		/// <summary>
		/// Opens the file as a stream.
		/// </summary>
		public FileStream Open(FileMode fileMode, FileAccess fileAccess) =>
			File.Open(FullPath.Normalized, fileMode, fileAccess);
		/// <summary>
		/// Opens and keeps the data on the file to add onto, write-only. 
		/// useful for logs
		/// </summary>
		public FileStream Append() => Open(FileMode.Append, FileAccess.Write);
		/// <summary>
		/// Opens the file for reading.
		/// </summary>
		public FileStream OpenRead() => Open(FileMode.Open, FileAccess.Read);
		/// <summary>
		/// Creates (or overwrites) to an empty file.
		/// </summary>
		/// <returns></returns>
		public FileStream Create() => Open(FileMode.Create, FileAccess.Write);
		/// <summary>
		/// Reads all texts of the file, assuming it exists.
		/// </summary>
		public string ReadAllText()
		{
			using (FileStream stream = OpenRead())
				return new StreamReader(stream).ReadToEnd();
		}
		/// <summary>
		/// Reads all text and splits on new line, or the input parameter.
		/// </summary>
		public string[] ReadAllLines(char splitChar = '\n') => ReadAllText().Split(splitChar);
		/// <summary>
		/// Creates a new file and writes all the text that is given.
		/// </summary>
		public void WriteAllText(string text)
		{
			using (StreamWriter writer = new StreamWriter(Create()))
				writer.Write(text);

		}
		/// <summary>
		/// Creates a new file, merges it in \n and writes all the text that is given.
		/// </summary>
		public void WriteAllLines(string[] lines) => WriteAllText(string.Join("\n", lines));
		/// <summary>
		/// Creates a new file, merges it in \n and writes all the text that is given.
		/// </summary>
		public void WriteAllLines(IEnumerable<string> lines) => WriteAllText(string.Join("\n", lines));

		public void AppendAllText(string text)
		{
			using (StreamWriter writer = new StreamWriter(Append()))
				writer.Write(text);
		}
		public void AppendAllLines(string[] lines) => AppendAllText(string.Join("\n", lines));
		public void AppendAllLines(IEnumerable<string> lines) => AppendAllText(string.Join("\n", lines));
		#endregion
		public override bool Delete()
		{
			if (Exists)
				File.Delete(FullPath);
			return Exists;
		}

		#region Extras
		public override void MoveTo(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using (FileStream to = copy.Create())
						from.CopyTo(to);
				}
				Delete();
			}
			FullPath = copy.FullPath;
		}
		public void MoveIntoFile(OSFile file)
		{
			if (!file.Parent.Exists)
				throw new DirectoryNotFoundException(file.Parent.FullPath);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using (FileStream to = file.Create())
						from.CopyTo(to);
				}
				Delete();
			}
			FullPath = file.FullPath;
		}
		public async Task MoveToAsync(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using (FileStream to = copy.Create())
						await from.CopyToAsync(to);
				}
				Delete();
			}
			FullPath = copy.FullPath;
		}
		public override OSSystemInfo CopyTo(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				FileStream from = OpenRead();
				FileStream to = copy.Create();
				from.CopyTo(to);
				from.Dispose();
				to.Dispose();
			}
			FullPath = copy.FullPath;
			return copy;
		}
		public void CopyIntoFile(OSFile file)
		{
			if (!file.Parent.Exists)
				throw new DirectoryNotFoundException(file.Parent.FullPath);
			if (Exists)
			{
				FileStream from = OpenRead();
				FileStream to = file.Create();
				from.CopyTo(to);
				from.Dispose();
				to.Dispose();
			}
			FullPath = file.FullPath;
		}
		public async Task<OSSystemInfo> CopyToAsync(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				FileStream from = OpenRead();
				FileStream to = copy.Create();
				await from.CopyToAsync(to);
				from.Dispose();
				to.Dispose();
			}
			FullPath = copy.FullPath;
			return copy;
		}

		#endregion

		
		void IDisposable.Dispose()
		{
			Delete();
		}
	}
}
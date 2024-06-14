namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVSSerializer;
using OVSSerializer.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class IOTester
{
	private OSDirectory? _directory;
	public OSDirectory Directory => _directory ??= OSDirectory.CreateTemp(DIRECTORY_NAME).Create();
	public const string DIRECTORY_NAME = "IO Test OVSSerializer";
	public IOTester()
	{

	}

	[TestMethod("Temp Creation Test")]
	public void TempTest()
	{
		_directory = null;
		_ = Directory;
	}

	[TestMethod("Directory Inspection")]
	public void DirectoryTest()
	{
		Assert.IsTrue(DIRECTORY_NAME == Directory.Name);
		Directory.Name = DIRECTORY_NAME;
		Assert.IsTrue(DIRECTORY_NAME == Directory.Name);
	}

	[TestMethod("Create File & Write")]
	public void CreateFileText()
	{
		OSFile file = Directory.GetFile("file.txt");
		file.WriteAllText("poopoo\nsexy");
		Assert.IsTrue("file" == file.NameWithoutExtension);
		Assert.IsTrue("file.txt" == file.Name);
	}


	[TestMethod("Incremental File & Write")]
	public void IncrementFileText()
	{
		for (int i = 0; i < 3; i++)
			Directory.GetFileIncremental("incremental.txt").WriteAllText("poopoo\nsexy");
	}

	[TestMethod("Dispose Folder Creation")]
	public void DisposeTest()
	{
		using OSDirectory directory = Directory.GetSubdirectories("dispose");
		directory.GetFile("test.txt").Create();
	}
}

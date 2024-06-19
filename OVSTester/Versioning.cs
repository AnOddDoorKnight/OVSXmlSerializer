namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVS.XmlSerialization;
using System;
using System.IO;

[TestClass]
public sealed class VersionTester
{
	[TestMethod("Null Version")]
	public void NullVersion()
	{
		OVSXmlSerializer<StandardClass> xmlSerializer = new() { Version = null };
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsTrue(Versioning.IsVersion(memoryStream, new Version(1, 0)));
	}
	[TestMethod("Same Version")]
	public void SameVersion()
	{
		OVSXmlSerializer<StandardClass> xmlSerializer = new() { Version = new Version(2, 4) };
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsTrue(Versioning.IsVersion(memoryStream, new Version(2, 4)));
	}
	[TestMethod("Different Version")]
	public void DifferentVersion()
	{
		OVSXmlSerializer<StandardClass> xmlSerializer = new()
		{
			Version = new Version(2, 5)
		};
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsFalse(Versioning.IsVersion(memoryStream, new Version(2, 4)));
	}
	[TestMethod("Lenient Version")]
	public void LenientVersion()
	{
		OVSXmlSerializer<StandardClass> xmlSerializer = new()
		{
			Version = new Version(2, 5),
			VersionLeniency = Versioning.Leniency.Minor
		};
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		string pos = new StreamReader(memoryStream).ReadToEnd();
		memoryStream.Position = 0;
		Assert.IsTrue(Versioning.IsVersion(memoryStream, new Version(2, 4), Versioning.Leniency.Minor));
	}
	[TestMethod("Lenient Different Version")]
	public void LenientDifferentVersion()
	{
		OVSXmlSerializer<StandardClass> xmlSerializer = new()
		{
			Version = new Version(3, 4),
			VersionLeniency = Versioning.Leniency.Minor
		};
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsFalse(Versioning.IsVersion(memoryStream, new Version(2, 4), Versioning.Leniency.Minor));
	}
}
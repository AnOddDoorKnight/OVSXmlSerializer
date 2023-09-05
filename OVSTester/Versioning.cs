namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OVSXmlSerializer;
using OVSXmlSerializer.Extras;
using System.Xml;
using System.Numerics;
using System.Drawing;
using ColorTuple = System.ValueTuple<float, float, float>;


[TestClass]
public sealed class VersionTester
{
	/*
	[TestMethod("Same Version")]
	public void SameVersion()
	{
		XmlSerializer<StandardClass> xmlSerializer = new(new() { Version = new Version(2, 4) });
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsTrue(Versioning.IsVersion(memoryStream, new Version(2, 4)));
	}
	[TestMethod("Different Version")]
	public void DifferentVersion()
	{
		XmlSerializer<StandardClass> xmlSerializer = new(new() { Version = new Version(2, 5) });
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsFalse(Versioning.IsVersion(memoryStream, new Version(2, 4)));
	}
	[TestMethod("Lenient Version")]
	public void LenientVersion()
	{
		XmlSerializer<StandardClass> xmlSerializer = new(new() { Version = new Version(2, 5), VersionLeniency = Versioning.Leniency.Minor });
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		string pos = new StreamReader(memoryStream).ReadToEnd();
		memoryStream.Position = 0;
		Assert.IsTrue(Versioning.IsVersion(memoryStream, new Version(2, 4), Versioning.Leniency.Minor));
	}
	[TestMethod("Lenient Different Version")]
	public void LenientDifferentVersion()
	{
		XmlSerializer<StandardClass> xmlSerializer = new(new() { Version = new Version(3, 4), VersionLeniency = Versioning.Leniency.Minor });
		using var memoryStream = xmlSerializer.Serialize(new StandardClass());
		Assert.IsFalse(Versioning.IsVersion(memoryStream, new Version(2, 4), Versioning.Leniency.Minor));
	}
	*/
}
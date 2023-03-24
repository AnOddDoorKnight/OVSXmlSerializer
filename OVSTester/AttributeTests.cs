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
public class AttributeTests
{
	[TestMethod("XmlNamed Attribute")]
	public void XmlNamed()
	{
		const int IGNORED_VALUE = 5;
		var test = new XmlNamedStructTest() { bruh = true, bruhhy = IGNORED_VALUE };
		XmlSerializer<XmlNamedStructTest> serializer = new();
		MemoryStream stream = serializer.Serialize(test);
		Assert.IsTrue(XmlDocumentExtras.LoadNew(stream).SelectSingleNode($"{nameof(XmlNamedStructTest)}//Sex") != null);
		stream.Position = 0;
		var result = serializer.Deserialize(stream);
		Assert.IsTrue(result.bruhhy == IGNORED_VALUE);
	}
	internal class XmlNamedStructTest
	{
		public bool bruh;
		[XmlNamedAs("Sex")]
		public int bruhhy;
	}
	[TestMethod("XmlIgnore Attribute")]
	public void XmlIgnore()
	{
		const int IGNORED_VALUE = 5;
		var test = new XmlIgnoreStructTest() { bruh = true, bruhhy = IGNORED_VALUE };
		XmlSerializer<XmlIgnoreStructTest> serializer = new();
		MemoryStream stream = serializer.Serialize(test);
		var result = serializer.Deserialize(stream);
		Assert.IsFalse(result.bruhhy == IGNORED_VALUE);
	}
	internal class XmlIgnoreStructTest
	{
		public bool bruh;
		[XmlIgnore]
		public int bruhhy;
	}

	[TestMethod("XmlAttribute Attribute")]
	public void XmlAttribute()
	{
		XmlSerializer<XmlAttributeTest> test = new();
		Stream stream = test.Serialize(new XmlAttributeTest() { bruhhy = 5 }, "Sex");
		XmlDocument document = XmlDocumentExtras.LoadNew(stream);
		bool isAttribute = document.LastChild!.Attributes!.GetNamedItem("bruhhy") != null;
		stream.Position = 0;
		var result = test.Deserialize(stream);
		Assert.IsTrue(isAttribute && result.bruhhy == 5);
	}
	internal class XmlAttributeTest
	{
		public bool bruh = true;
		[XmlAttribute]
		public int bruhhy = 0;
	}
	[TestMethod("XmlAttribute Attribute Named")]
	public void XmlAttributeNamed()
	{
		XmlSerializer<XmlAttributeTestNamed> test = new();
		Stream stream = test.Serialize(new XmlAttributeTestNamed() { bruhhy = 5 }, "Sex");
		XmlDocument document = XmlDocumentExtras.LoadNew(stream);
		Assert.IsFalse(document.LastChild!.Attributes!.GetNamedItem("poopy") == null);
		stream.Position = 0;
		var result = test.Deserialize(stream);
		Assert.IsTrue(result.bruhhy == 5);
	}
	internal class XmlAttributeTestNamed
	{
		public bool bruh = true;
		[XmlAttribute, XmlNamedAs("poopy")]
		public int bruhhy = 0;
	}
	[TestMethod("XmlAttribute Attribute Named Alt")]
	public void XmlAttributeNamedAlt()
	{
		XmlSerializer<XmlAttributeTestNamedAlt> test = new();
		Stream stream = test.Serialize(new XmlAttributeTestNamedAlt() { bruhhy = 5 }, "Sex");
		XmlDocument document = XmlDocumentExtras.LoadNew(stream);
		Assert.IsFalse(document.LastChild!.Attributes!.GetNamedItem("poopy") == null);
		stream.Position = 0;
		var result = test.Deserialize(stream);
		Assert.IsTrue(result.bruhhy == 5);
	}
	internal class XmlAttributeTestNamedAlt
	{
		public bool bruh = true;
		[XmlAttribute("poopy")]
		public int bruhhy = 0;
	}

	[TestMethod("XmlAttribute Attribute Error")]
	public void XmlAttributeError()
	{
		var serializer = new XmlSerializer<XmlAttributeTestError>();
		Assert.ThrowsException<Exception>(() => serializer.Serialize(new XmlAttributeTestError()));
	}
	internal class XmlAttributeTestError
	{
		public bool bruh = true;
		[XmlAttribute]
		public Vector2 bruhhy = Vector2.One;
	}
	[TestMethod("XmlText Attribute")]
	public void XmlText()
	{
		var test = new XmlTextTest() { ohpdifbr = "tpewo" };
		XmlSerializer<XmlTextTest> serializer = new();
		MemoryStream stream = serializer.Serialize(test);
		string xml = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		var result = serializer.Deserialize(stream);
		Assert.IsTrue(result.ohpdifbr == test.ohpdifbr);
	}
	internal class XmlTextTest
	{
		[XmlIgnore]
		public string brih = "pro";
		[XmlAttribute]
		public int groekg = 5;
		[XmlText]
		public string ohpdifbr = "hoahaha";
	}
}
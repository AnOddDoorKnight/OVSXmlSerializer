namespace Tester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OVSXmlSerializer;
using OVSXmlSerializer.Extras;
using XmlSerializer = OVSXmlSerializer.XmlSerializer;
using System.Xml.Serialization;
using IXmlSerializable = OVSXmlSerializer.IXmlSerializable;
using System.Xml;
using System.Numerics;

[TestClass]
public class ObjectSerialization
{
	[TestMethod("String Serialization")]
	public void SimpleSerialize()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string));
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Key/Value Pair Serialization")]
	public void KeyValueSerializer()
	{
		KeyValuePair<string, int> value = new ("bruh", 4);
		XmlSerializer serializer = new(typeof(KeyValuePair<string, int>));
		var stream = serializer.Serialize(value, "Pair");
		string report = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		var output = (KeyValuePair<string, int>)serializer.Deserialize(stream);
		Assert.IsTrue(value.Key == output.Key && value.Value == output.Value);//(value, (KeyValuePair<string, int>)serializer.Deserialize(stream));
	}
	[TestMethod("List Serialization")]
	public void ListSerialization()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		XmlSerializer<List<string>> serializer = new();
		var stream = serializer.Serialize(value);
		List<string> result = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First == pair.Second) == value.Count);
	}
	[TestMethod("Dictionary Serialization")]
	public void DictionarySerialization()
	{
		Dictionary<int, string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add(Random.Shared.Next(int.MinValue, int.MaxValue), "bruh");
		XmlSerializer<Dictionary<int, string>> serializer = new(new XmlSerializerConfig() { alwaysIncludeTypes = true });
		var stream = serializer.Serialize(value, "Dictionary");
		string report = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Dictionary<int, string> output = serializer.Deserialize(stream);
		Assert.IsTrue(output.Count > 0, "output dictionary contains nothing!");
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key) == value.Count, "output doesn't match the contents of the original!");
	}
	[TestMethod("Empty Class Serialization")]
	public void ClassSerialization()
	{
		Program value = new();
		XmlSerializer<Program> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		Program result = xmlSerializer.Deserialize(stream);
		Assert.AreEqual(value, result);
	}
	[TestMethod("Indent & Formatting")]
	public void IndentFormatting()
	{
		string[] value = { "bruh" };
		XmlSerializer serializer = new(typeof(string));
		var stream = serializer.Serialize(value);
		stream.Position = 0;
		string[] strings = new StreamReader(stream).ReadToEnd().Split('\n');
		Assert.IsTrue(strings[2].StartsWith("\t"));
	}
	[TestMethod("Class Serialization")]
	public void StandardSerialization()
	{
		StandardClass value = new();
		XmlSerializer<StandardClass> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		StandardClass result = xmlSerializer.Deserialize(stream);
		//Assert.AreEqual(value, result);
	}
	
	[TestMethod("Ensure Disallow Parameter-only Constructor Serialization")]
	public void DisallowParameterConstructor()
	{
		try
		{
			XmlSerializer<XmlParameteredClassTest> tester = new();
			tester.Serialize(new XmlParameteredClassTest("h"));
		}
		catch (NullReferenceException ex) when (ex.Message.EndsWith("does not have an empty constructor!"))
		{
			return;
		}
		Assert.Fail();
	}
	[TestMethod("Xml Serializer Interface")]
	public void XmlSerializerInterface()
	{
		ByteArraySim simulator = ByteArraySim.WithRandomValues();
		XmlSerializer<ByteArraySim> serializer = new();
		var stream = serializer.Serialize(simulator);
		ByteArraySim result = serializer.Deserialize(stream);
		Assert.IsTrue(simulator.values.Zip(result.values).Count(pair => pair.First == pair.Second) == simulator.values.Length);
	}
	[TestMethod("False Xml Serializer Interface")]
	public void FalseXmlSerializerInterface()
	{
		ByteArraySim simulator = new();
		simulator.ShouldWrite = false;
		XmlSerializer<ByteArraySim> serializer = new();
		var stream = serializer.Serialize(simulator);
		ByteArraySim result = serializer.Deserialize(stream);
		Assert.IsTrue(result is null || simulator.values.Zip(result.values).Count(pair => pair.First == pair.Second) == simulator.values.Length);
	}
	[TestMethod("Property Serialization")]
	public void PropertySerialization()
	{
		AutoProp autoProp = new() { bruh = "bruh" };
		XmlSerializer<AutoProp> serializer = new();
		var stream = serializer.Serialize(autoProp);
		AutoProp result = serializer.Deserialize(stream);
		Assert.IsTrue(result.bruh == autoProp.bruh);
	}
}

internal class ByteArraySim : IXmlSerializable
{
	public static ByteArraySim WithRandomValues()
	{
		ByteArraySim output = new();
		for (byte i = 0; i < output.values.Length; i++)
			output.values[i] = (byte)Random.Shared.Next(byte.MaxValue);
		return output;
	}
	public byte[] values;
	public ByteArraySim()
	{
		values = new byte[byte.MaxValue];
	}

	public bool ShouldWrite { get; set; } = true;

	void IXmlSerializable.Read(XmlNode obj)
	{
		if (obj is null)
			return;
		values = Array.ConvertAll(obj.InnerText.Split('.'), @string => byte.Parse(@string));
	}

	void IXmlSerializable.Write(XmlWriter writer)
	{
		writer.WriteString(string.Join(".", values));
	}
}
internal class XmlParameteredClassTest
{
	public string bruh;
	public XmlParameteredClassTest(string bruh)
	{
		this.bruh = bruh;
	}
}
internal class StandardClass
{
	public string value = "no";
	public string sex = "sex";
	public int values = 2;
	public int genders = 1032;
}
internal class AutoProp
{
	public string bruh { get; set; } = "h";
}
internal class Program : IEquatable<Program>
{
	public bool Equals(Program? other) => true;
	public override bool Equals(object? obj)
	{
		if (obj is Program program)
			return Equals(program);
		return false;
	}
}

[TestClass]
public class AttributeTests
{
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
}

[TestClass]
public class ImplicitObjectSerialization
{
	[TestMethod("Implicit String Serialization")]
	public void SimpleImplicitSerialize()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string), new XmlSerializerConfig() { alwaysIncludeTypes = false });
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Implicit List String Serialization")]
	public void ListImplicitSerialize()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		XmlSerializer<List<string>> serializer = new(new XmlSerializerConfig() { alwaysIncludeTypes = false });
		var stream = serializer.Serialize(value);
		List<string> result = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First == pair.Second) == value.Count);
	}
}
[TestClass]
public class B1NARYSerialization
{
	[TestMethod("Player Config")]
	public void DictionarySerialization()
	{
		Dictionary<string, object> value = new();
		for (int i = 0; i < 10; i++)
			value.Add(Random.Shared.Next(int.MinValue, int.MaxValue).ToString(), Random.Shared.Next(int.MinValue, int.MaxValue));
		XmlSerializer serializer = new(typeof(Dictionary<string, object>), new XmlSerializerConfig() { alwaysIncludeTypes = true });
		var stream = serializer.Serialize(value, "PlayerConfig");
		Dictionary<string, object> output = (Dictionary<string, object>)serializer.Deserialize(stream);
		Dictionary<string, int> outputSequel = output.ToDictionary(key => key.Key, value => (int)value.Value);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key && pair.First.Value.Equals(pair.Second.Value)) == value.Count);
	}
}
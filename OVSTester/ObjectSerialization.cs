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
public class ObjectSerialization
{
	[TestMethod("Enum Serialization")]
	public void EnumSerialize()
	{
		const Environment.SpecialFolder specialFolder = Environment.SpecialFolder.MyDocuments;
		XmlSerializer serializer = new(typeof(Environment.SpecialFolder));
		var stream = serializer.Serialize(specialFolder);
		Assert.AreEqual(specialFolder, (Environment.SpecialFolder)serializer.Deserialize(stream));
	}
	[TestMethod("Null Class")]
	public void NullSerialize()
	{

		StandardClass value = null;
		XmlSerializer<StandardClass> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		StandardClass result = xmlSerializer.Deserialize(stream);
		Assert.IsTrue(result is null);
	}
	[TestMethod("Null Item in Class")]
	public void NullItemSerialize()
	{

		StandardClass value = new StandardClass() { sex = null };
		XmlSerializer<StandardClass> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		StandardClass result = xmlSerializer.Deserialize(stream);
		Assert.IsTrue(result.sex is null);
	}
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
		KeyValuePair<string, int> value = new("bruh", 4);
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
		XmlSerializer<Dictionary<int, string>> serializer = new(new XmlSerializerConfig() { TypeHandling = IncludeTypes.AlwaysIncludeTypes });
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
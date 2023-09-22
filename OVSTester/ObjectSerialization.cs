namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVSSerializer;
using OVSSerializer.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;

[TestClass]
public class ObjectSerialization
{
	[TestMethod("Enum Serialization")]
	public void EnumSerialize()
	{
		OVSXmlSerializer<Environment.SpecialFolder> serializer = new();
		serializer.OmitXmlDelcaration = false;
		using MemoryStream stream = serializer.Serialize(Environment.SpecialFolder.System);
		serializer.Deserialize(stream);
		return;
	}
	[TestMethod("Null Class")]
	public void NullSerialize()
	{
		Random? value = null;
		OVSXmlSerializer<Random> xmlSerializer = new();
		using MemoryStream stream = xmlSerializer.Serialize(value);
		Random? result = xmlSerializer.Deserialize(stream);
		Assert.IsTrue(result is null);
	}
	[TestMethod("Null Item in Class")]
	public void NullItemSerialize()
	{

		StandardClass value = new StandardClass() { sex = null };
		OVSXmlSerializer<StandardClass> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		StandardClass result = xmlSerializer.Deserialize(stream);
		Assert.IsTrue(result.sex is null);
	}
	[TestMethod("String Serialization")]
	public void SimpleSerialize()
	{
		const string value = "bruh";
		OVSXmlSerializer serializer = new();
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Readonly Serialize Serialization")]
	public void ReadonlySerialize()
	{
		object @readonly = new Readonly(4);
		Assert.ThrowsException<SerializationFailedException>(() => new OVSXmlSerializer() { HandleReadonlyFields = ReadonlyFieldHandle.ThrowError }.Serialize(@readonly));
	}
	[TestMethod("Key/Value Pair Serialization")]
	public void KeyValueSerializer()
	{
		KeyValuePair<string, int> value = new("bruh", 4);
		var stream = OVSXmlSerializer.Shared.Serialize(value, "Pair");
		string report = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		object obj = OVSXmlSerializer.Shared.Deserialize(stream);
		KeyValuePair<string, int> output = (KeyValuePair<string, int>)obj;
		Assert.IsTrue(value.Key == output.Key && value.Value == output.Value);//(value, (KeyValuePair<string, int>)serializer.Deserialize(stream));
	}
	[TestMethod("List Serialization")]
	public void ListSerialization()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		var stream = OVSXmlSerializer<List<string>>.Shared.Serialize(value);
		string output = new StreamReader(stream).ReadToEnd(); stream.Position = 0;
		List<string> result = OVSXmlSerializer<List<string>>.Shared.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First == pair.Second) == value.Count);
	}
	[TestMethod("Dictionary Serialization")]
	public void DictionarySerialization()
	{
		Dictionary<int, string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add(Random.Shared.Next(int.MinValue, int.MaxValue), "bruh");
		OVSXmlSerializer<Dictionary<int, string>> serializer = new();
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
		OVSXmlSerializer<Program> xmlSerializer = new();
		using var stream = xmlSerializer.Serialize(value);
		Program result = xmlSerializer.Deserialize(stream);
		Assert.AreEqual(value, result);
	}
	[TestMethod("Indent & Formatting")]
	public void IndentFormatting()
	{
		string[] value = { "bruh" };
		var stream = OVSXmlSerializer.Shared.Serialize(value);
		string[] strings = new StreamReader(stream).ReadToEnd().Split('\n');
		Assert.IsTrue(strings[2].StartsWith("\t"), $"'{strings[3]}' Does not match '{OVSXmlSerializer.Shared.IndentChars}'!");
	}
	[TestMethod("Class Serialization")]
	public void StandardSerialization()
	{
		StandardClass value = new();
		value.value = null;
		OVSXmlSerializer<StandardClass> xmlSerializer = new();
		xmlSerializer.IgnoreUndefinedValues = true;
		using var stream = xmlSerializer.Serialize(value);
		StandardClass result = xmlSerializer.Deserialize(stream);
		Assert.AreEqual(new StandardClass().value, result.value);
	}

	[TestMethod("Ensure Disallow Parameter-only Constructor Serialization")]
	public void DisallowParameterConstructor()
	{
		OVSXmlSerializer<XmlParameteredClassTest> tester = new();
		void Action() => tester.Serialize(new XmlParameteredClassTest("h"));
		Assert.ThrowsException<SerializationFailedException>(Action);

	}
	[TestMethod("Xml Serializer Interface")]
	public void XmlSerializerInterface()
	{
		ByteArraySim simulator = ByteArraySim.WithRandomValues();
		MemoryStream stream = OVSXmlSerializer<ByteArraySim>.Shared.Serialize(simulator);
		ByteArraySim result = OVSXmlSerializer<ByteArraySim>.Shared.Deserialize(stream);
		Assert.IsTrue(simulator.values.Zip(result.values).Count(pair => pair.First == pair.Second) == simulator.values.Length);
	}
	[TestMethod("False Xml Serializer Interface")]
	public void FalseXmlSerializerInterface()
	{
		ByteArraySim simulator = new();
		simulator.ShouldWrite = false;
		OVSXmlSerializer<ByteArraySim> serializer = new();
		var stream = serializer.Serialize(simulator);
		ByteArraySim result = serializer.Deserialize(stream);
		Assert.IsTrue(result is null || simulator.values.Zip(result.values).Count(pair => pair.First == pair.Second) == simulator.values.Length);
	}
	[TestMethod("Property Serialization")]
	public void PropertySerialization()
	{
		AutoProp autoProp = new() { bruh = "bruh" };
		OVSXmlSerializer<AutoProp> serializer = new();
		var stream = serializer.Serialize(autoProp);
		AutoProp result = serializer.Deserialize(stream);
		Assert.IsTrue(result.bruh == autoProp.bruh);
	}
	
	[TestMethod("Circular Serialization")]
	public void CircularSerialization()
	{
		CircularSerialization serialization = new();
		serialization.circularSerialization = serialization;
		OVSXmlSerializer<CircularSerialization> serializer = new();
		serializer.UseSingleInstanceInsteadOfMultiple = true;
		using var stream = serializer.Serialize(serialization);
		string outputStr = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		CircularSerialization output = serializer.Deserialize(stream);
		Assert.IsTrue(ReferenceEquals(output, output.circularSerialization));
	}
}
internal class CircularSerialization
{
	public CircularSerialization circularSerialization;
}
internal class ByteArraySim : IOVSXmlSerializable
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

	void IOVSXmlSerializable.Read(XmlNode obj)
	{
		if (obj is null)
			return;
		values = Array.ConvertAll(obj.InnerText.Split('.'), @string => byte.Parse(@string));
	}

	void IOVSXmlSerializable.Write(XmlNode writer)
	{
		writer.InnerText = string.Join(".", values);
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
internal class Readonly
{
	public readonly int h = 3;
	public Readonly()
	{

	}
	public Readonly(int h)
	{
		this.h = h;
	}
}
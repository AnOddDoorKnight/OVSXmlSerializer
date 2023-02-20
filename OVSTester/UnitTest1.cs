namespace Tester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OVSXmlSerializer;
using XmlSerializer = OVSXmlSerializer.XmlSerializer;

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
		Dictionary<int, string> value = new Dictionary<int, string>();
		for (int i = 0; i < 10; i++)
			value.Add(Random.Shared.Next(int.MinValue, int.MaxValue), "bruh");
		XmlSerializer<Dictionary<int, string>> serializer = new(new XmlSerializerConfig() { includeTypes = true });
		var stream = serializer.Serialize(value, "Dictionary");
		Dictionary<int, string> output = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key) == value.Count);
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
public class ImplicitObjectSerialization
{
	[TestMethod("Implicit String Serialization")]
	public void SimpleImplicitSerialize()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string), new XmlSerializerConfig() { includeTypes = false });
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Implicit List String Serialization")]
	public void ListImplicitSerialize()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		XmlSerializer<List<string>> serializer = new(new XmlSerializerConfig() { includeTypes = false });
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
		Dictionary<string, object> value = new Dictionary<string, object>();
		for (int i = 0; i < 10; i++)
			value.Add(Random.Shared.Next(int.MinValue, int.MaxValue).ToString(), new object());
		XmlSerializer<Dictionary<string, object>> serializer = new(new XmlSerializerConfig() { includeTypes = true });
		var stream = serializer.Serialize(value, "PlayerConfig");
		Dictionary<string, object> output = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key.Equals(pair.Second.Key) && pair.First.Value.Equals(pair.Second.Value)) == value.Count);
	}
}
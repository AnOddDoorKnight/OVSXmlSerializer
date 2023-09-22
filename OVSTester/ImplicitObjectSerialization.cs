namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVSSerializer;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[TestClass]
public class ImplicitObjectSerialization
{
	[TestMethod("Implicit String Serialization")]
	public void SimpleImplicitSerialize()
	{
		const string value = "bruh";
		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Implicit List String Serialization")]
	public void ListImplicitSerialize()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		OVSXmlSerializer<List<string>> serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		List<string> result = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First == pair.Second) == value.Count);
	}
	[TestMethod("Implicit Dictionary Int Serialization")]
	public void DictionaryImplicitSerialize()
	{
		Dictionary<string, int> value = new();
		for (int i = 0; i < 10; i++)
			value.Add($"bruh{i}", i);
		OVSXmlSerializer<Dictionary<string, int>> serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		string XML = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Dictionary<string, int> result = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key && pair.First.Value == pair.Second.Value) == value.Count);
	}
}
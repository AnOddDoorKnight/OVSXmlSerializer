namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DebuggingTest
{
	/*
	[TestMethod]
	public void TestDebug()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string));
		serializer.Config.Logger = new OVSXmlLogger();
		var stream = serializer.Serialize(value);
		string output = string.Join("\n", serializer.Config.Logger.DebugLines);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Class Serialization")]
	public void StandardSerialization()
	{
		StandardClass value = new();
		XmlSerializer<StandardClass> xmlSerializer = new();
		xmlSerializer.Config.Logger = new OVSXmlLogger();
		using var stream = xmlSerializer.Serialize(value);
		string output = string.Join("\n", xmlSerializer.Config.Logger.DebugLines);
		StandardClass result = xmlSerializer.Deserialize(stream);
		//Assert.AreEqual(value, result);
	}
	*/
}
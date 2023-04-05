using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVSXmlSerializer;

namespace OVSTester;

[TestClass]
public class DebuggingTest
{
	[TestMethod]
	public void TestDebug()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string));
		serializer.Config.logger = new OVSXmlLogger();
		var stream = serializer.Serialize(value);
		string output = string.Join("\n", serializer.Config.logger.DebugLines);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Class Serialization")]
	public void StandardSerialization()
	{
		StandardClass value = new();
		XmlSerializer<StandardClass> xmlSerializer = new();
		xmlSerializer.Config.logger = new OVSXmlLogger();
		using var stream = xmlSerializer.Serialize(value);
		string output = string.Join("\n", xmlSerializer.Config.logger.DebugLines);
		StandardClass result = xmlSerializer.Deserialize(stream);
		//Assert.AreEqual(value, result);
	}
}
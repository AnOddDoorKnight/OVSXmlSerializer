namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVS.XmlSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
internal class DelegatesSerialization
{
	[TestMethod("Anon Delegate Serialization")]
	public void AnonDelegateSerialization()
	{
		const int setTo = 1;
		int number = 0;
		Delegate serialize = () =>
		{
			number = setTo;
		};
		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(serialize);
		string print = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		(serializer.Deserialize(stream) as Delegate).DynamicInvoke();
		Assert.IsTrue(number == setTo);
	}
	[TestMethod("Delegate Serialization")]
	public void DelegateSerialization()
	{
		const int setTo = 1;
		int number = 0;
		Delegate serialize = Serialize;
		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(serialize);
		string print = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		(serializer.Deserialize(stream) as Delegate).DynamicInvoke();
		Assert.IsTrue(number == setTo);
		void Serialize()
		{
			number = setTo;
		}
	}
	[TestMethod("Method Serialization")]
	public void MethodSerialization()
	{
		int number = 0;
		Delegate serialize = Set;
		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(serialize);
		string print = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		number = (int)(serializer.Deserialize(stream) as Delegate).DynamicInvoke();
		Assert.IsTrue(number == 1);
	}
	private int Set()
	{
		return 1;
	}
}

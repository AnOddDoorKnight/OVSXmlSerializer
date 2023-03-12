namespace Tester;

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

[TestClass]
public class ImplicitObjectSerialization
{
	[TestMethod("Implicit String Serialization")]
	public void SimpleImplicitSerialize()
	{
		const string value = "bruh";
		XmlSerializer serializer = new(typeof(string), new XmlSerializerConfig() { TypeHandling = IncludeTypes.SmartTypes });
		var stream = serializer.Serialize(value);
		Assert.AreEqual(value, (string)serializer.Deserialize(stream));
	}
	[TestMethod("Implicit List String Serialization")]
	public void ListImplicitSerialize()
	{
		List<string> value = new();
		for (int i = 0; i < 10; i++)
			value.Add("bruh");
		XmlSerializer<List<string>> serializer = new(new XmlSerializerConfig() { TypeHandling = IncludeTypes.SmartTypes });
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
		XmlSerializer<Dictionary<string, int>> serializer = new(new XmlSerializerConfig() { TypeHandling = IncludeTypes.SmartTypes });
		var stream = serializer.Serialize(value);
		string XML = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Dictionary<string, int> result = serializer.Deserialize(stream);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key && pair.First.Value == pair.Second.Value) == value.Count);
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
		XmlSerializer serializer = new(typeof(Dictionary<string, object>), new XmlSerializerConfig() { TypeHandling = IncludeTypes.SmartTypes });
		var stream = serializer.Serialize(value, "PlayerConfig");
		string str = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Dictionary<string, object> output = (Dictionary<string, object>)serializer.Deserialize(stream);
		Dictionary<string, int> outputSequel = output.ToDictionary(key => key.Key, value => (int)value.Value);
		Assert.IsTrue(value.Zip(value).Count(pair => pair.First.Key == pair.Second.Key && pair.First.Value.Equals(pair.Second.Value)) == value.Count);
	}
	[TestMethod("Color Format Test")]
	public void ColorFormatSerialization()
	{
		ColorFormat colorFormat = new()
		{
			FormatName = "Default",
		};
		XmlSerializer<ColorFormat> formatter = new();
		var stream = formatter.Serialize(colorFormat);
		string str = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		ColorFormat output = formatter.Deserialize(stream);
	}
	[TestMethod("Changeable Value Test")]
	public void ChangableValueSerialization()
	{
		Storer storer = new();
		XmlSerializer<Storer> formatter = new();
		var stream = formatter.Serialize(storer);
		string str = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Storer output = formatter.Deserialize(stream);
	}
	[TestMethod("Player Config Test")]
	public void PlayerConfigSerialization()
	{
		PlayerConfig storer = new();
		XmlSerializer<PlayerConfig> formatter = new();
		var stream = formatter.Serialize(storer);
		string str = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		PlayerConfig output = formatter.Deserialize(stream);
	}
	internal class ColorFormat
	{
		/// <summary>
		/// Converts a byte to a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static float ToPercent(byte value)
		{
			return (float)value / byte.MaxValue;
		}
		/// <summary>
		/// Converts a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static byte ToByte(float percent)
		{
			return Convert.ToByte(percent * byte.MaxValue);
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("name")]
		public string FormatName;
		/// <summary>
		/// The color format version of the color format.
		/// </summary>
		[XmlAttribute("ver")]
		public int Version = 1;
		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		[XmlNamedAs("Primary")]
		public ColorTuple primaryUI = new ColorTuple(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		[XmlNamedAs("Secondary")]
		public ColorTuple SecondaryUI = new ColorTuple(ToPercent(47), ToPercent(206), ToPercent(172));
		/// <summary>
		/// 
		/// </summary>
		[XmlNamedAs("Extras")]
		public Dictionary<string, Color> ExtraUIColors = new Dictionary<string, Color>();

		/// <summary>
		/// 
		/// </summary>
		public ColorFormat()
		{

		}
	}
	public sealed class ChangableValue<T>
	{
		public static implicit operator T(ChangableValue<T> input)
		{
			return input.Value;
		}
		[XmlText]
		private T value;
		[field: XmlIgnore]
		public event Action<T> ValueChanged;
		public T Value
		{
			get => value;
			set
			{
				this.value = value;
				ValueChanged?.Invoke(value);
			}
		}
		private ChangableValue()
		{

		}
		public ChangableValue(T @default)
		{
			value = @default;
		}
		public void AttachValue(Action<T> input)
		{
			input.Invoke(Value);
			ValueChanged += input;
		}
	}
	public class Storer
	{
		public ChangableValue<float> bruh = new(4f);
		public ChangableValue<float> sex = new(12f);
		public string no = "brih";
	}
	public class PlayerConfig
	{
		public Gameplay gameplay = new();
		public Video video = new();
		public Input input = new();
		public Audio audio = new();




		[Serializable]
		public sealed class Audio
		{
			public float master = 1f;
			public float SFX = 1f;
			public float musicMain = 1f;
			public float passiveMusic = 1f;
			public float activeMusic = 1f;
			public float theVoices = 1f;
		}
		[Serializable]
		public sealed class Gameplay
		{

		}

		[Serializable]
		public sealed class Video
		{

			public float cameraBobLookMultiplier = 1f;
			[XmlNamedAs("enabled")]
			public bool cameraBobLookEnabled = true;
			public float movementBobMultiplier = 1f;
			[XmlNamedAs("bobEnabled")]
			public bool movementBobEnabled = true;
			[XmlNamedAs("maxMultiplier")]
			public float cameraBobLookMaxMultiplier = 5f;
			[XmlNamedAs("bobCap")]
			public float movementBobCap = 3f;
			public ChangableValue<float> fieldOfView = new(78f);
		}
		[Serializable]
		public sealed class Input
		{
			public KeyboardMouse keyboardMouse = new();
			public Controller controller = new();




			[Serializable]
			public sealed class KeyboardMouse
			{
				public ChangableValue<float> mouseSensitivity = new(25f);
			}
			[Serializable]
			public sealed class Controller
			{

			}
		}
	}
}
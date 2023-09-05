namespace OVSTester;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OVSXmlSerializer;
using OVSXmlSerializer.Extras;
using System.Xml;
using System.Numerics;
using System.Drawing;
using ColorTuple = System.ValueTuple<float, float, float>;


/*
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
	[TestMethod("Collections Serialization")]
	public void CollectionSerialization()
	{
		Collection<string> storer = new();
		storer.Add("bruh", "hot");
		storer.Add("haha", () => "no");
		XmlSerializer<Collection<string>> formatter = new();
		var stream = formatter.Serialize(storer);
		string str = new StreamReader(stream).ReadToEnd();
		stream.Position = 0;
		Collection<string> output = formatter.Deserialize(stream);
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
/// <summary>
/// A delegate that is treated as an event, sending out the new value and 
/// its source.
/// </summary>
public delegate void UpdatedConstantValue<T>(string key, T oldValue, T newValue, Collection<T> source);

[Serializable, XMLIgnoreEnumerable]
public sealed class Collection<T> : IDictionary<string, T>
{
	/// <summary>
	/// Sends out an event that happens when a value is changed.
	/// </summary>
	[field: NonSerialized, XmlIgnore]
	public event UpdatedConstantValue<T> UpdatedValue;
	private Dictionary<string, T> constants = new Dictionary<string, T>();
	[field: NonSerialized, XmlIgnore]
	private Dictionary<string, Func<T>> pointers = new Dictionary<string, Func<T>>();

	public T this[string key]
	{
		get
		{
			if (constants.TryGetValue(key, out T value))
				return value;
			if (pointers.TryGetValue(key, out Func<T> dele))
				return dele.Invoke();
			throw new KeyNotFoundException(key);
		}
		set
		{
			if (pointers.TryGetValue(key, out Func<T> func))
			{
				T funcOldValue = func.Invoke();
				pointers.Remove(key);
				constants.Add(key, value);
				UpdatedValue?.Invoke(key, funcOldValue, constants[key], this);
				return;
			}
			constants.TryGetValue(key, out T oldValue);
			constants[key] = value;
			UpdatedValue?.Invoke(key, oldValue, constants[key], this);
		}
	}

	/// <summary>
	/// Determines if the key has a point or not.
	/// </summary>
	/// <returns> 
	/// <see langword="true"/> if it is a pointer, <see langword="false"/> 
	/// if its a constant, <see langword="null"/> if it is not located 
	/// in the collection.
	/// </returns>
	public bool? IsPointer(string key)
	{
		if (constants.ContainsKey(key))
			return false;
		if (pointers.ContainsKey(key))
			return true;
		return null;
	}

	public ICollection<string> Keys
	{
		get
		{
			List<string> keys = new List<string>(constants.Keys);
			keys.AddRange(pointers.Keys);
			return keys;
		}
	}

	public ICollection<T> Values
	{
		get
		{
			List<T> values = new List<T>(constants.Values);
			values.AddRange(pointers.Values.Select(item => item.Invoke()));
			return values;
		}
	}
	public int Count => constants.Count + pointers.Count;

	bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

	public void Add(string key, T value)
	{
		if (pointers.ContainsKey(key))
			throw new InvalidOperationException(nameof(key));
		constants.Add(key, value);
	}

	public void Add(KeyValuePair<string, T> item)
	{
		if (pointers.ContainsKey(item.Key))
			throw new InvalidOperationException(nameof(item.Key));
		constants.Add(item.Key, item.Value);
	}

	public void Add(string key, Func<T> value)
	{
		if (constants.ContainsKey(key))
			throw new InvalidOperationException(nameof(key));
		pointers.Add(key, value);
	}


	public void Clear()
	{
		pointers.Clear();
		constants.Clear();
	}

	public bool Contains(KeyValuePair<string, T> item)
	{
		if (constants.TryGetValue(item.Key, out T value))
			if (value.Equals(item.Value))
				return true;
		return false;
	}

	public bool ContainsKey(string key)
	{
		return constants.ContainsKey(key) || pointers.ContainsKey(key);
	}

	void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
	{
		using (var enumerator = constants.AsEnumerable().GetEnumerator())
			while (enumerator.MoveNext())
				yield return enumerator.Current;
		// XML parser doesn't like it
		//using (var enumerator = pointers.AsEnumerable().GetEnumerator())
		//	while (enumerator.MoveNext())
		//		yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		using (var enumerator = constants.AsEnumerable().GetEnumerator())
			while (enumerator.MoveNext())
				yield return enumerator.Current;
		// XML parser doesn't like it
		//using (var enumerator = pointers.AsEnumerable().GetEnumerator())
		//	while (enumerator.MoveNext())
		//		yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
	}

	public bool Remove(string key)
	{
		if (constants.ContainsKey(key))
			return constants.Remove(key);
		if (pointers.ContainsKey(key))
			return pointers.Remove(key);
		return false;
	}

	bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
	{
		throw new NotImplementedException();
	}

	public bool TryGetValue(string key, out T value)
	{
		if (constants.TryGetValue(key, out value))
			return true;
		if (pointers.TryGetValue(key, out Func<T> pointer))
		{
			value = pointer.Invoke();
			return true;
		}
		return false;
	}
}
*/
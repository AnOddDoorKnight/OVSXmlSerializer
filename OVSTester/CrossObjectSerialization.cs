namespace OVSTester;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OVSSerializer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class CrossObjectSerialization
{
	[TestMethod("One to Two")]
	public void OneToTwoColors()
	{
		using MemoryStream memory = OVSXmlSerializer<One>.Shared.Serialize(new One { Color = Color.Red });
		Two two = OVSXmlSerializer<Two>.Shared.Deserialize(memory);
	}
	[TestMethod("Two to One")]
	public void TwoToOneColors()
	{
		using MemoryStream memory = OVSXmlSerializer<Two>.Shared.Serialize(new Two { Color = Color.Red, Color2 = Color.Green });
		One one = OVSXmlSerializer<One>.Shared.Deserialize(memory);
	}
	struct One
	{
		public Color Color { get; set; }
	}
	struct Two
	{
		public Color Color { get; set; }
		public Color Color2 { get; set; }
	}
	[TestMethod("Referenced One To Two")]
	public void ReferencedOneToTwo()
	{
		OneClass one = new OneClass { Color = Color.Red }; one.This = one;
		using MemoryStream memory = OVSXmlSerializer<OneClass>.Shared.Serialize(one);
		TwoClass two = OVSXmlSerializer<TwoClass>.Shared.Deserialize(memory);
	}
	class OneClass
	{
		public Color Color { get; set; }
		public OneClass This { get; set; }
	}
	class TwoClass
	{
		public Color Color { get; set; }
		public Color Color2 { get; set; }
	}
}
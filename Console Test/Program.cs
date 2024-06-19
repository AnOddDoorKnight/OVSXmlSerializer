using OVS.XmlSerialization;

internal class Program
{
	public Program() { }
	public IEnumerator<string> enumerator = EnumeratorTest();
	private static void Main(string[] args)
	{
		Program value = new();
		value.enumerator.MoveNext();

		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		Program output = (Program)serializer.Deserialize(stream);
	}

	private static IEnumerator<string> EnumeratorTest()
	{
		yield return "1";
		yield return "2";
		yield return "3";
	}
}
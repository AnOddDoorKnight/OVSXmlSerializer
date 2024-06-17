using OVSSerializer;

internal class Program
{
	public Program() { }
	public IEnumerator<string> enumerator = new List<string>() { "br" }.GetEnumerator();
	private static void Main(string[] args)
	{
		Program value = new();

		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		Program output = (Program)serializer.Deserialize(stream);
	}
}
using OVSSerializer;

internal class Program
{
	public Program(int sex) { }
	private static void Main(string[] args)
	{
		const string value = "bruh";

		OVSXmlSerializer serializer = new() { TypeHandling = IncludeTypes.SmartTypes };
		var stream = serializer.Serialize(value);
		string output = (string)serializer.Deserialize(stream);
		Console.WriteLine(output == value);
	}
}
class H : List<object> { }
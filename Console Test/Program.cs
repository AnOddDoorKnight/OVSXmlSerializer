using OVSSerializer;

internal class Program
{
	public Program(int sex) { }
	private static void Main(string[] args)
	{
		Console.WriteLine($"{typeof(H).Namespace}");
	}
}
class H : List<object> { }
using OVSXmlSerializer;

internal class Program
{
	public Program(int sex) { }
	private static void Main(string[] args)
	{
		OVSXmlSerializer<Program>.Shared.Config.HandleReadonlyFields = ReadonlyFieldHandle.ThrowError;
		try { OVSXmlSerializer<Program>.Shared.Serialize(new Program(5)); }
		catch { }
	}
}
using OVSXmlSerializer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

Dictionary<int, string> program = new Dictionary<int, string>();
for (int i = 0; i < 10; i++)
	program.Add(Random.Shared.Next(int.MinValue, int.MaxValue), "bruh");
using FileStream stream = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Test.xml").Open(FileMode.Create, FileAccess.ReadWrite);
XmlSerializer<Dictionary<int, string>> serializer = new(new XmlSerializerConfig() { includeTypes = true });
serializer.Serialize(program, "List").CopyTo(stream);
stream.Position = 0;
Console.Write(new StreamReader(stream).ReadToEnd());
stream.Position = 0;
Console.WriteLine(serializer.Deserialize(stream).ToString());
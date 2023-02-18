using OVSXmlSerializer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

List<KeyValuePair<string, int>> program = Enumerable.Repeat(new KeyValuePair<string, int>("test", Random.Shared.Next(int.MinValue, int.MaxValue)), 10).ToList();
using FileStream stream = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Test.xml").Open(FileMode.Create, FileAccess.ReadWrite);
XmlSerializer<List<KeyValuePair<string, int>>> serializer = new(new XmlSerializerConfig() { includeTypes = true });
serializer.Serialize(program, "List").CopyTo(stream);
stream.Position = 0;
Console.Write(new StreamReader(stream).ReadToEnd());
stream.Position = 0;
Console.WriteLine(serializer.Deserialize(stream).ToString());
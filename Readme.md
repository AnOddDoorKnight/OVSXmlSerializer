# Odd's Very Special Xml Serializer

..Or OVSXmlSerializer for short.



The system itself works very similar to how the XML serializer works normally,
but is meant to be worked with the `object` or more 'undefined' data that the 
ordinary XML serializer have difficulty handling. You can turn this off for more
traditional formatting with the config class, but it does reveal the issue once 
again.

Unlike the traditional XML Serializer, this will use the type parameters in the
class to automatically differentiate enums and arrays, which will remove the need
to mark fields as `[XmlArray]` or `[XmlEnum]`.

## Explicit Types

The unique function of this XML serializer over others is its *explicit type 
handling.* What this does is that it allows it to reserve its derived types when
considering base types, down to the level of `object`. This feature can be turned
on or off.

This feature is enabled by default, but creating `XmlSerializerConfig`, this can
be turned off. It will also implicitly convert `System.Xml.XmlWriterSettings` to
the config to ensure smooth transtion between systems.

There are 3 options you can enable:
1. **Always Included** - Regardless of the situation, it will write the type.
2. **Smart Types** - When defined out of the base field, it will write the type.
3. **Ignore** - It will never write the type, which may show incorrect behaviour.

## Serializing

Like the default system XML serializer, they pose the same requirements such as:
1. Requiring to have a public or private parameterless constructor.
2. Having all fields follow the same constraint as above.

There are two serializers, the generic and the non-generic. The non-generic derives
from the generic as object for performance reasons, and is created as instances
due to configurations.

The method returns a `MemoryStream`, a stream that can be used for overriding specific
file systems as such, but can be converted to a string if needed by other systems.

The system provides its own interface called `IXmlSerializable`, which is not
the same as the traditional system. Luckily, instead of using `XmlReader`, it uses
a `XmlNode` instead. `XmlWriter` is here to stay, though.

## Deserializing

Requires the stream or XML file to retrieve the object mentioned by the XML.
It is relatively easy enough, get a file that is generated by the OVSXmlSerializer
and it will de-parse it easily, assuming that you know the derived types.

## Attributes

Here are a few attributes that uses in its own `OVSXmlSerializer` namespace.
Note that `System.Xml.Serialization` will be converted automatically if needed.

1. `[XmlIgnore]` Ignores the field completely
2. `[XmlAttribute]` Adds *primitive value* as a single attribute to the class
    being serialized
3. `[XmlNamedAs(string name)]` Changes the field name or object name to something 
   else on XML serialization.
4. `[XmlText]` Assuming that all other fields are attributes or ignored, this
   will write the primitive value 

Arrays are automatically serialized as if it is an ordinary list or dictionary.
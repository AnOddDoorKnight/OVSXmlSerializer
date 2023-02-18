# Odd's Very Special Xml Serializer

..Or OVSXmlSerializer for short.



This is a specialized Xml Serializer that is mainly designed with use in B1NARY,
a visual novel about porn on the internet or whatever idfk not important. This
is used as a way to encapsulate and update the module over time when needed, 
typically as dll file or using the Nuget system.

The system itself works very similar to how the XML serializer works normally,
but is mean't to be worked with the `object` or more 'undefined' data that the 
ordinary XML serializer have difficulty handling. You can turn this off for more
traditional formatting with the config class, but it does reveal the issue once 
again.

This tool is meant to be handled with specific classes within just simply `object`
parameters. This feature can be turned off and handled normally, but may cause
issues that would not be present.

The wiki serves as *Documentation*, and this will be reserved on how to use the
tool.

Note that this currently does not support XML attributes for the time being.

## Explicit Types

The unique function of this XML serializer over others is its *explicit type 
handling.* What this does is that it allows it to reserve its derived types when
considering base types, down to the level of `object`. This feature can be turned
on or off.

This feature is enabled by default, but creating `XmlSerializerConfig`, this can
be turned off. It will also implicitly convert `System.Xml.XmlWriterSettings` to
the config to ensure smooth transtion between systems.

## Serializing

Like the default system XML serializer, they pose the same requirements such as:
1. Requiring to have a public or private parameterless constructor.
2. Having all fields follow the same constraint as above.

There are two serializers, the generic and the non-generic. The non-generic derives
from the generic as object for performance reasons, and is created as instances
due to configs.

The method returns a `MemoryStream`, a stream that can be used for overriding specific
file systems as such, but can be converted to a string if needed by other systems.

## Deserializing

Requires the stream or XML file to retrieve the object mentioned by the XML.
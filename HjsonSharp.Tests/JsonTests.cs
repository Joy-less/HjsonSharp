using System.Text.Json;

namespace HjsonSharp.Tests;

public class JsonTests {
    [Fact]
    public void ParseExampleTest() {
        // Example from https://www.json.org/example.html

        string Text = """
            {
                "glossary": {
                    "title": "example glossary",
            		"GlossDiv": {
                        "title": "S",
            			"GlossList": {
                            "GlossEntry": {
                                "ID": "SGML",
            					"SortAs": "SGML",
            					"GlossTerm": "Standard Generalized Markup Language",
            					"Acronym": "SGML",
            					"Abbrev": "ISO 8879:1986",
            					"GlossDef": {
                                    "para": "A meta-markup language, used to create markup languages such as DocBook.",
            						"GlossSeeAlso": ["GML", "XML"]
                                },
            					"GlossSee": "markup"
                            }
                        }
                    }
                }
            }
            """;
        var AnonymousObject = new {
            glossary = new {
                title = "example glossary",
                GlossDiv = new {
                    title = "S",
                    GlossList = new {
                        GlossEntry = new {
                            ID = "SGML",
                            SortAs = "SGML",
                            GlossTerm = "Standard Generalized Markup Language",
                            Acronym = "SGML",
                            Abbrev = "ISO 8879:1986",
                            GlossDef = new {
                                para = "A meta-markup language, used to create markup languages such as DocBook.",
                                GlossSeeAlso = new[] { "GML", "XML" }
                            },
                            GlossSee = "markup"
                        }
                    }
                }
            }
        };

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).Value;
        Assert.Equal(JsonSerializer.Serialize(AnonymousObject), JsonSerializer.Serialize(Element));
    }
    [Fact]
    public void ParseBasicObjectTest() {
        string Text = """
            {
              "first": 1,
              "second": 2
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).Value;
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void ParseBasicArrayTest() {
        string Text = """
            [
                1,
                2, 3
            ]
            """;

        int[] Array = CustomJsonReader.ParseElement<int[]>(Text, CustomJsonReaderOptions.Json).Value!;
        Assert.Equal([1, 2, 3], Array);
    }
    [Fact]
    public void ParseNestedArrayTest() {
        string Text = """
            {
              "first": [
                1,
                2,
                3
              ],
              "second": 2
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).Value;
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal([1, 2, 3], Element.GetProperty("first").Deserialize<int[]>(GlobalJsonOptions.Mini)!);
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void FindPropertyNameTest() {
        string Text = """
            {
              "first": 1,
              "second": {
                "third": 5
              }
            }
            """;

        using CustomJsonReader HjsonReader = new(Text, CustomJsonReaderOptions.Json);
        Assert.True(HjsonReader.FindPropertyValue("second"));
        JsonElement Element = HjsonReader.ParseElement().Value;
        Assert.Equal(5, Element.GetProperty("third").Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void FindArrayIndexTest() {
        string Text = """
            [
              1,
              4,
              5
            ]
            """;

        using CustomJsonReader HjsonReader = new(Text, CustomJsonReaderOptions.Json);
        Assert.True(HjsonReader.FindArrayIndex(2));
        JsonElement Element = HjsonReader.ParseElement().Value;
        Assert.Equal(5, Element.Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void TrailingCommasTest() {
        string Text = """
            {
              "a": 1,
            }
            """;

        Assert.True(CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).IsError);
    }
    [Fact]
    public void ExponentTest() {
        string Text = """
            {
              "1000": 10e3,
              "2000": 2.0E-3,
              "-3500": -35e3
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).Value;
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal(10e3, Element.GetProperty("1000").Deserialize<double>(GlobalJsonOptions.Mini));
        Assert.Equal(2.0E-3, Element.GetProperty("2000").Deserialize<double>(GlobalJsonOptions.Mini));
        Assert.Equal(-35e3, Element.GetProperty("-3500").Deserialize<double>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void ErroneousNumberTest() {
        Assert.True(CustomJsonReader.ParseElement<double>("-", CustomJsonReaderOptions.Json).IsError);
        Assert.True(CustomJsonReader.ParseElement<double>("+", CustomJsonReaderOptions.Json).IsError);
        Assert.True(CustomJsonReader.ParseElement<double>("-.", CustomJsonReaderOptions.Json).IsError);
        Assert.True(CustomJsonReader.ParseElement<double>(".", CustomJsonReaderOptions.Json).IsError);
    }
    [Fact]
    public void LeadingZeroTest() {
        Assert.Equal(0, CustomJsonReader.ParseElement<int>("0", CustomJsonReaderOptions.Json));
        Assert.True(CustomJsonReader.ParseElement<int>("01", CustomJsonReaderOptions.Json).IsError);
        Assert.True(CustomJsonReader.ParseElement<int>("001", CustomJsonReaderOptions.Json).IsError);
        Assert.Equal(0e0, CustomJsonReader.ParseElement<double>("0e0", CustomJsonReaderOptions.Json));
    }
    [Fact]
    public void StringEscapedHexSequences() {
        Assert.Equal("ç", CustomJsonReader.ParseElement<string>("""
            "\u00E7"
            """, CustomJsonReaderOptions.Json));
    }
    [Fact]
    public void ElementLengthTest() {
        using CustomJsonReader Reader1 = new("\"abcde\"");
        Assert.Equal(7, Reader1.ReadElementLength());

        using CustomJsonReader Reader2 = new("xyz\"abcde\"xyz", 3, "xyz\"abcde\"xyz".Length - 3);
        Assert.Equal(7, Reader2.ReadElementLength());
    }
}
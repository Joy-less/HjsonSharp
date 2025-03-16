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
        JsonSerializer.Serialize(Element).ShouldBe(JsonSerializer.Serialize(AnonymousObject));
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
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(1);
        Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(2);
    }
    [Fact]
    public void ParseBasicArrayTest() {
        string Text = """
            [
                1,
                2, 3
            ]
            """;

        int[]? Array = CustomJsonReader.ParseElement<int[]>(Text, CustomJsonReaderOptions.Json).Value;
        Array.ShouldBe([1, 2, 3]);
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
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("first").Deserialize<int[]>(GlobalJsonOptions.Mini).ShouldBe([1, 2, 3]);
        Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(2);
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
        HjsonReader.FindPropertyValue("second").ShouldBeTrue();
        JsonElement Element = HjsonReader.ParseElement().Value;
        Element.GetProperty("third").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(5);
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
        HjsonReader.FindArrayIndex(2).ShouldBeTrue();
        JsonElement Element = HjsonReader.ParseElement().Value;
        Element.Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(5);
    }
    [Fact]
    public void TrailingCommasTest() {
        string Text = """
            {
              "a": 1,
            }
            """;

        CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
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
        Element.GetPropertyCount().ShouldBe(3);
        Element.GetProperty("1000").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(10e3);
        Element.GetProperty("2000").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(2.0E-3);
        Element.GetProperty("-3500").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(-35e3);
    }
    [Fact]
    public void ErroneousNumberTest() {
        CustomJsonReader.ParseElement<double>("-", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
        CustomJsonReader.ParseElement<double>("+", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
        CustomJsonReader.ParseElement<double>("-.", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
        CustomJsonReader.ParseElement<double>(".", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
    }
    [Fact]
    public void LeadingZeroTest() {
        CustomJsonReader.ParseElement<int>("0", CustomJsonReaderOptions.Json).ShouldBe(0);
        CustomJsonReader.ParseElement<int>("01", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
        CustomJsonReader.ParseElement<int>("001", CustomJsonReaderOptions.Json).IsError.ShouldBeTrue();
        CustomJsonReader.ParseElement<double>("0e0", CustomJsonReaderOptions.Json).ShouldBe(0e0);
    }
    [Fact]
    public void StringEscapedHexSequences() {
        CustomJsonReader.ParseElement<string>("""
            "\u00E7"
            """, CustomJsonReaderOptions.Json).ShouldBe("ç");
    }
    [Fact]
    public void ElementLengthTest() {
        using CustomJsonReader Reader1 = new("\"abcde\"");
        Reader1.ReadElementLength().ShouldBe(7);

        using CustomJsonReader Reader2 = new("xyz\"abcde\"xyz", 3, "xyz\"abcde\"xyz".Length - 3);
        Reader2.ReadElementLength().ShouldBe(7);
    }
}
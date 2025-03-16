using System.Text.Json;

namespace HjsonSharp.Tests;

public class HjsonTests {
    [Fact]
    public void ParseExampleTest() {
        // Example from https://hjson.github.io

        string Text = """
            {
              // use #, // or /**/ comments,
              // omit quotes for keys
              key: 1
              // omit quotes for strings
              contains: everything on this line
              // omit commas at the end of a line
              cool: {
                foo: 1
                bar: 2
              }
              // allow trailing commas
              list: [
                1,
                2,
              ]
              // and use multiline strings
              realist:
                '''
                My half empty glass,
                I will fill your empty half.
                Now you are half full.
                '''
            }
            """;
        var AnonymousObject = new {
            // use #, // or /**/ comments,
            // omit quotes for keys
            key = "1", // TODO: Replace with number
            // omit quotes for strings
            contains = "everything on this line",
            // omit commas at the end of a line
            cool = new {
                foo = "1", // TODO: Replace with number
                bar = "2" // TODO: Replace with number
            },
            // allow trailing commas
            list = new[] {
                "1", // TODO: Replace with number
                "2", // TODO: Replace with number
            },
            // and use multiline strings
            realist = """
                My half empty glass,
                I will fill your empty half.
                Now you are half full.
                """
        };

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        JsonSerializer.Serialize(Element).ShouldBe(JsonSerializer.Serialize(AnonymousObject));
    }

    [Fact]
    public void HashStyleCommentsTest() {
        string Text = """
            {
              "first": 1, # This is a comment
              "second": 2 # This is another comment
            } # This is a final comment
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(1);
        Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(2);
    }
    [Fact]
    public void UnquotedPropertyNamesTest() {
        string Text = """
            {
              abcdef12345_!$%: "b",
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(1);
        Element.GetProperty("abcdef12345_!$%").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("b");
    }
    [Fact]
    public void SingleQuotedStringsTest() {
        string Text = """
            {
              'a': 'b',
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(1);
        Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("b");
    }
    [Fact]
    public void UnquotedStringsTest() {
        string Text = """
            {
              "a": b,
              "c": d{}e
              "f":g h  i
              "j":
              k
              "l": 123m
              "m": 12/*3*/
              "n": .a
              "o": 5.
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(8);
        Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("b,");
        Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("d{}e");
        Element.GetProperty("f").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("g h  i");
        Element.GetProperty("j").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("k");
        Element.GetProperty("l").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("123m");
        Element.GetProperty("m").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(12);
        Element.GetProperty("n").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe(".a");
        Element.GetProperty("o").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("5.");
    }
    [Fact]
    public void TripleQuotedStringsTest() {
        string Text = """
            {
              "a": '''
                qwerty
                ''',
              "b":
                '''
                qwerty
                ''',
              "c":
                '''
                 qwerty
                '''
              "d": '''qwerty'''
              "e": '''  qwerty  '''
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(5);
        Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("qwerty");
        Element.GetProperty("b").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("qwerty");
        Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe(" qwerty");
        Element.GetProperty("d").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("qwerty");
        Element.GetProperty("e").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("  qwerty  ");
    }
    [Fact]
    public void OneLineOmittedCommasTest() {
        string Text = """
            {
              "a":"b" c: de: f
              "g": [
                1 2
              ]
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(3);
        Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("b");
        Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("de: f");
        Element.GetProperty("g").Deserialize<string[]>(GlobalJsonOptions.Mini).ShouldBe(["1 2"]);
    }
    [Fact]
    public void UnquotedLinks() {
        string Code = """
            {
                "Example": https://example.com/sub-example
            }
            """;
        JsonElement Element = CustomJsonReader.ParseElement(Code, CustomJsonReaderOptions.Hjson).Value;
        Element.GetPropertyCount().ShouldBe(1);
        Element.GetProperty("Example").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("https://example.com/sub-example");
    }
    [Fact]
    public void OmittedRootBracesTest() {
        string Text1 = """
            {
              "a": "b",
              "c": "d"
            }
            """;
        string Text2 = """
            "a": "b",
            "c": "d"
            """;
        JsonElement Element1 = CustomJsonReader.ParseElement(Text1, CustomJsonReaderOptions.Hjson).Value;
        JsonElement Element2 = CustomJsonReader.ParseElement(Text2, CustomJsonReaderOptions.Hjson).Value;
        Element1.ToString().ShouldBe(Element2.ToString());

        string Text3 = """
            null
            """;
        JsonElement Element3 = CustomJsonReader.ParseElement(Text3, CustomJsonReaderOptions.Hjson).Value;
        Element3.Deserialize<string>(GlobalJsonOptions.Mini).ShouldBeNull();

        string Text4 = """
            null: 5
            """;
        JsonElement Element4 = CustomJsonReader.ParseElement(Text4, CustomJsonReaderOptions.Hjson).Value;
        Element4.GetPropertyCount().ShouldBe(1);
        Element4.GetProperty("null").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(5);
    }
    [Fact]
    public void SurrogatePairsTest() {
        string GrinningFaceEmojiHjson = "\"\\uD83D\\uDE04\"";
        string GrinningFaceEmojiString = "\uD83D\uDE04"; // 😀

        JsonElement Element = CustomJsonReader.ParseElement(GrinningFaceEmojiHjson, CustomJsonReaderOptions.Hjson).Value;
        Element.Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe(GrinningFaceEmojiString);
    }
}
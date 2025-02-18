﻿using System.Text.Json;

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
        Assert.Equal(JsonSerializer.Serialize(AnonymousObject), JsonSerializer.Serialize(Element));
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
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void UnquotedPropertyNamesTest() {
        string Text = """
            {
              abcdef12345_!$%: "b",
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("abcdef12345_!$%").Deserialize<string>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void SingleQuotedStringsTest() {
        string Text = """
            {
              'a': 'b',
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Hjson).Value;
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini));
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
        Assert.Equal(8, Element.GetPropertyCount());
        Assert.Equal("b,", Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("d{}e", Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("g h  i", Element.GetProperty("f").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("k", Element.GetProperty("j").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("123m", Element.GetProperty("l").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal(12, Element.GetProperty("m").Deserialize<int>(GlobalJsonOptions.Mini));
        Assert.Equal(".a", Element.GetProperty("n").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("5.", Element.GetProperty("o").Deserialize<string>(GlobalJsonOptions.Mini));
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
        Assert.Equal(5, Element.GetPropertyCount());
        Assert.Equal("qwerty", Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("qwerty", Element.GetProperty("b").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal(" qwerty", Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("qwerty", Element.GetProperty("d").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("  qwerty  ", Element.GetProperty("e").Deserialize<string>(GlobalJsonOptions.Mini));
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
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal("de: f", Element.GetProperty("c").Deserialize<string>(GlobalJsonOptions.Mini));
        Assert.Equal(["1 2"], Element.GetProperty("g").Deserialize<string[]>(GlobalJsonOptions.Mini)!);
    }
    [Fact]
    public void UnquotedLinks() {
        string Code = """
            {
                "Example": https://example.com/sub-example
            }
            """;
        JsonElement Element = CustomJsonReader.ParseElement(Code, CustomJsonReaderOptions.Hjson).Value;
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("https://example.com/sub-example", Element.GetProperty("Example").Deserialize<string>(GlobalJsonOptions.Mini));
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
        Assert.Equal(Element1.ToString(), Element2.ToString());

        string Text3 = """
            null
            """;
        JsonElement Element3 = CustomJsonReader.ParseElement(Text3, CustomJsonReaderOptions.Hjson).Value;
        Assert.Null(Element3.Deserialize<string>(GlobalJsonOptions.Mini));

        string Text4 = """
            null: 5
            """;
        JsonElement Element4 = CustomJsonReader.ParseElement(Text4, CustomJsonReaderOptions.Hjson).Value;
        Assert.Equal(1, Element4.GetPropertyCount());
        Assert.Equal(5, Element4.GetProperty("null").Deserialize<int>(GlobalJsonOptions.Mini));
    }
    [Fact]
    public void SurrogatePairsTest() {
        string GrinningFaceEmojiHjson = "\"\\uD83D\\uDE04\"";
        string GrinningFaceEmojiString = "\uD83D\uDE04"; // 😀

        JsonElement Element = CustomJsonReader.ParseElement(GrinningFaceEmojiHjson, CustomJsonReaderOptions.Hjson).Value;
        Assert.Equal(GrinningFaceEmojiString, Element.Deserialize<string>(GlobalJsonOptions.Mini));
    }
}
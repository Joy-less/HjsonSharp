using System.Text.Json;

namespace HjsonSharp.Tests;

public class HjsonTests {
    [Fact]
    public void HashStyleCommentsTest() {
        string Text = """
            {
              "first": 1, # This is a comment
              "second": 2 # This is another comment
            } # This is a final comment
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void UnquotedPropertyNamesTest() {
        string Text = """
            {
              abcdef12345_!$%: "b",
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("abcdef12345_!$%").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void SingleQuotedStringsTest() {
        string Text = """
            {
              'a': 'b',
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a").Deserialize<string>(JsonOptions.Mini));
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

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(8, Element.GetPropertyCount());
        Assert.Equal("b,", Element.GetProperty("a").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("d{}e", Element.GetProperty("c").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("g h  i", Element.GetProperty("f").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("k", Element.GetProperty("j").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("123m", Element.GetProperty("l").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal(12, Element.GetProperty("m").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(".a", Element.GetProperty("n").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("5.", Element.GetProperty("o").Deserialize<string>(JsonOptions.Mini));
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

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(5, Element.GetPropertyCount());
        Assert.Equal("qwerty", Element.GetProperty("a").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("qwerty", Element.GetProperty("b").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal(" qwerty", Element.GetProperty("c").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("qwerty", Element.GetProperty("d").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("  qwerty  ", Element.GetProperty("e").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void OneLineOmittedCommasTest() {
        string Text = """
            {
              "a":"b"c:de:f
              "g": [
                1 2
              ]
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Hjson);
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("de:f", Element.GetProperty("c").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal(["1 2"], Element.GetProperty("g").Deserialize<string[]>(JsonOptions.Mini)!);
    }
}
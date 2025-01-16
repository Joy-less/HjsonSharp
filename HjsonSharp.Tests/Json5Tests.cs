using System.Globalization;
using System.Text.Json;

namespace HjsonSharp.Tests;

public class Json5Tests {
    [Fact]
    public void UnicodeWhitespaceTest() {
        string HairSpace = "\u200A";
        string ParagraphSeparator = "\u2029";

        string Text = $$"""
            {
            {{HairSpace}}"first":{{HairSpace}}1,
            {{ParagraphSeparator}}"second": 2
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void LeadingDecimalPointTest() {
        string Text = ".3";

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(0.3, Element.Deserialize<double>(JsonOptions.Mini));
    }
    [Fact]
    public void TrailingDecimalPointTest() {
        string Text = "3.";

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(3.0, Element.Deserialize<double>(JsonOptions.Mini));
    }
    [Fact]
    public void LineAndBlockStyleCommentsTest() {
        string Text = """
            {
              /*hmmm*/"first": 1, // This is a comment
              "second": 2 /* This is another comment */
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void TrailingCommasTest() {
        string Text = """
            {
              "a": 1,
              "b": [
                2,
              ],
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("a").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal([2], Element.GetProperty("b").Deserialize<int[]>(JsonOptions.Mini)!);
    }
    [Fact]
    public void EcmaScriptPropertyNamesTest() {
        string Text = """
            {
              a$_b\u0065: "b",
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a$_b\u0065").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void NamedFloatingPointLiteralsTest() {
        string Text = """
            {
              "a": Infinity,
              "b": -Infinity,
              "c": NaN
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal(double.PositiveInfinity, Element.GetProperty("a").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NegativeInfinity, Element.GetProperty("b").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NaN, Element.GetProperty("c").Deserialize<double>(JsonOptions.Mini));
    }
    [Fact]
    public void HexadecimalNumbers() {
        string Text = """
            {
              "a": 0X50,
              "b": 0xDECAF,
              "c": -0xC0FFEE
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json5);
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal("0X50", Element.GetProperty("a").ToString());
        Assert.Equal("0xDECAF", Element.GetProperty("b").ToString());
        Assert.Equal("-0xC0FFEE", Element.GetProperty("c").ToString());
    }
    [Fact]
    public void StringEscapedShortHexSequences() {
        Assert.Equal("ç", HjsonReader.ParseElement<string>("""
            "\xE7"
            """, HjsonReaderOptions.Json5));
        Assert.Equal("ç00", HjsonReader.ParseElement<string>("""
            "\xE700"
            """, HjsonReaderOptions.Json5));
    }
}
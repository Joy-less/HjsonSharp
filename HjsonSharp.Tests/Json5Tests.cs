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

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void LeadingDecimalPointTest() {
        string Text = ".3";

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
        Assert.Equal(0.3, Element.Deserialize<double>(JsonOptions.Mini));
    }
    [Fact]
    public void TrailingDecimalPointTest() {
        string Text = "3.";

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
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

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
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

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
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

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("b", Element.GetProperty("a$_b\u0065").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void NamedFloatingPointLiteralsTest() {
        string Text = """
            {
              "a": Infinity,
              "b": -Infinity,
              "c": NaN,
              "d": -NaN
            }
            """;

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json5);
        Assert.Equal(4, Element.GetPropertyCount());
        Assert.Equal(double.PositiveInfinity, Element.GetProperty("a").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NegativeInfinity, Element.GetProperty("b").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NaN, Element.GetProperty("c").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NaN, Element.GetProperty("d").Deserialize<double>(JsonOptions.Mini));
    }
}
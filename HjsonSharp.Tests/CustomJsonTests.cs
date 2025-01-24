using System.Text.Json;

namespace HjsonSharp.Tests;

public class CustomJsonTests {
    [Fact]
    public void NamedFloatingPointLiteralsWithUnquotedStringsTest() {
        string Text = """
            {
              "a": Infinity
              "b": -Infinity
              "c": NaN
              "d": Infinit5
              "e": -Infinit5
              "f": Na5
            }
            """;

        JsonElement Element = HjsonReader.ParseElement(Text, HjsonReaderOptions.Json with {
            NamedFloatingPointLiterals = true,
            QuotelessStrings = true,
            OmittedCommas = true,
        }).Value;
        Assert.Equal(6, Element.GetPropertyCount());
        Assert.Equal(double.PositiveInfinity, Element.GetProperty("a").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NegativeInfinity, Element.GetProperty("b").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NaN, Element.GetProperty("c").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal("Infinit5", Element.GetProperty("d").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("-Infinit5", Element.GetProperty("e").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("Na5", Element.GetProperty("f").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void BasicIncompleteInputsTest() {
        string Text = """
            {
              "key": "val
            """;

        JsonElement Element = HjsonReader.ParseElement(Text, HjsonReaderOptions.Json with {
            IncompleteInputs = true,
        }).Value;
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("val", Element.GetProperty("key").Deserialize<string>(JsonOptions.Mini));
    }
    [Fact]
    public void ComplexIncompleteInputsTest() {
        string Text = """
            {
              "items": [
                "apple",
                "orange",
                10
            """;

        JsonElement Element = HjsonReader.ParseElement(Text, HjsonReaderOptions.Json with {
            IncompleteInputs = true,
        }).Value;
        Assert.Equal(1, Element.GetPropertyCount());
        Assert.Equal("apple,orange,10", string.Join(',', Element.GetProperty("items").EnumerateArray()));
    }
}
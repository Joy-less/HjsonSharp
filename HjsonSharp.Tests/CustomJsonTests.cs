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

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json with {
            NamedFloatingPointLiterals = true,
            UnquotedStrings = true,
            OmittedCommas = true,
        });
        Assert.Equal(6, Element.GetPropertyCount());
        Assert.Equal(double.PositiveInfinity, Element.GetProperty("a").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NegativeInfinity, Element.GetProperty("b").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(double.NaN, Element.GetProperty("c").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal("Infinit5", Element.GetProperty("d").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("-Infinit5", Element.GetProperty("e").Deserialize<string>(JsonOptions.Mini));
        Assert.Equal("Na5", Element.GetProperty("f").Deserialize<string>(JsonOptions.Mini));
    }
}
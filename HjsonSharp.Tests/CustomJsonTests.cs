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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json with {
            NamedFloatingPointLiterals = true,
            QuotelessStrings = true,
            OmittedCommas = true,
        }).Value;
        Element.GetPropertyCount().ShouldBe(6);
        Element.GetProperty("a").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.PositiveInfinity);
        Element.GetProperty("b").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.NegativeInfinity);
        Element.GetProperty("c").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.NaN);
        Element.GetProperty("d").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("Infinit5");
        Element.GetProperty("e").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("-Infinit5");
        Element.GetProperty("f").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("Na5");
    }
    [Fact]
    public void BasicIncompleteInputsTest() {
        string Text = """
            {
              "key": "val
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json with {
            IncompleteInputs = true,
        }).Value;
        Element.GetPropertyCount().ShouldBe(1);
        Element.GetProperty("key").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("val");
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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json with {
            IncompleteInputs = true,
        }).Value;
        Element.GetPropertyCount().ShouldBe(1);
        string.Join(',', Element.GetProperty("items").EnumerateArray()).ShouldBe("apple,orange,10");
    }
}
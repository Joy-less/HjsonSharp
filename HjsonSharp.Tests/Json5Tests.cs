using System.Text.Json;

namespace HjsonSharp.Tests;

public class Json5Tests {
    [Fact]
    public void ParseExampleTest() {
        // Example from https://json5.org

        string Text = """
            {
              // comments
              unquoted: 'and you can quote me on that',
              singleQuotes: 'I can use "double quotes" here',
              lineBreaks: "Look, Mom! \
            No \\n's!",
              hexadecimal: 0xdecaf,
              leadingDecimalPoint: .8675309, andTrailing: 8675309.,
              positiveSign: +1,
              trailingComma: 'in objects', andIn: ['arrays',],
              "backwardsCompatible": "with JSON",
            }
            """;
        var AnonymousObject = new {
            // comments
            unquoted = "and you can quote me on that",
            singleQuotes = "I can use \"double quotes\" here",
            lineBreaks = "Look, Mom! No \\n's!",
            hexadecimal = "0xdecaf", // TODO: Replace with number
            leadingDecimalPoint = ".8675309", andTrailing = "8675309.", // TODO: Replace with numbers
            positiveSign = "+1", // TODO: Replace with number
            trailingComma = "in objects", andIn = new[] { "arrays" },
            backwardsCompatible = "with JSON",
        };

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        JsonSerializer.Serialize(Element).ShouldBe(JsonSerializer.Serialize(AnonymousObject));
    }
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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(1);
        Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(2);
    }
    [Fact]
    public void LeadingDecimalPointTest() {
        string Text = ".3";

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(0.3);
    }
    [Fact]
    public void TrailingDecimalPointTest() {
        string Text = "3.";

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(3.0);
    }
    [Fact]
    public void LineAndBlockStyleCommentsTest() {
        string Text = """
            {
              /*hmmm*/"first": 1, // This is a comment
              "second": 2 /* This is another comment */
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("first").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(1);
        Element.GetProperty("second").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(2);
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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(2);
        Element.GetProperty("a").Deserialize<int>(GlobalJsonOptions.Mini).ShouldBe(1);
        Element.GetProperty("b").Deserialize<int[]>(GlobalJsonOptions.Mini).ShouldBe([2]);
    }
    [Fact]
    public void EcmaScriptPropertyNamesTest() {
        string Text = """
            {
              a$_b\u0065: "b",
            }
            """;

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(1);
        Element.GetProperty("a$_b\u0065").Deserialize<string>(GlobalJsonOptions.Mini).ShouldBe("b");
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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(3);
        Element.GetProperty("a").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.PositiveInfinity);
        Element.GetProperty("b").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.NegativeInfinity);
        Element.GetProperty("c").Deserialize<double>(GlobalJsonOptions.Mini).ShouldBe(double.NaN);
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

        JsonElement Element = CustomJsonReader.ParseElement(Text, CustomJsonReaderOptions.Json5).Value;
        Element.GetPropertyCount().ShouldBe(3);
        Element.GetProperty("a").ToString().ShouldBe("0X50");
        Element.GetProperty("b").ToString().ShouldBe("0xDECAF");
        Element.GetProperty("c").ToString().ShouldBe("-0xC0FFEE");
    }
    [Fact]
    public void EscapedStringShortHexSequences() {
        string Text1 = """
            "\xE7"
            """;
        CustomJsonReader.ParseElement<string>(Text1, CustomJsonReaderOptions.Json5).ShouldBe("ç");

        string Text2 = """
            "\xE700"
            """;
        CustomJsonReader.ParseElement<string>(Text2, CustomJsonReaderOptions.Json5).ShouldBe("ç00");
    }
    [Fact]
    public void EscapedStringNewlines() {
        string Text1 = """
                "a\
                b"
            """;
        CustomJsonReader.ParseElement<string>(Text1, CustomJsonReaderOptions.Json5).ShouldBe("a    b");

        string Text2 = "\"a\\\r\\\nb\"";
        CustomJsonReader.ParseElement<string>(Text2, CustomJsonReaderOptions.Json5).ShouldBe("ab");
    }
}
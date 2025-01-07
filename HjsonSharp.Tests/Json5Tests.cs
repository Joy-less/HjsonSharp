using System.Text;
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

        using HjsonStream HjsonStream = new(Text, new HjsonStreamOptions() {
            Syntax = JsonSyntaxOptions.Json5,
        });
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
}
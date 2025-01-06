using System.Text.Json;

namespace HjsonSharp.Tests;

public class UnitTest1 {
    [Fact]
    public void JsonTest() {
        string Text = """
            {
              "first": 1,
              "second": 2
            }
            """;

        using HjsonStream HjsonStream = new(Text);
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void FindTest() {
        string Text = """
            {
              "first": 1,
              "second": {
                "third": 5
              }
            }
            """;

        using HjsonStream HjsonStream = new(Text);
        Assert.True(HjsonStream.FindPath(["second"]));
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(5, Element.GetProperty("third").Deserialize<int>(JsonOptions.Mini));
    }
}
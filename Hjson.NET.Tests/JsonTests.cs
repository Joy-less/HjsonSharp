using System.Text.Json;

namespace Hjson.NET.Tests;

public class JsonTests {
    [Fact]
    public void ParseBasicObjectTest() {
        string Text = """
            {
              "first": 1,
              "second": 2
            }
            """;

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, new HjsonStreamOptions() {
            Syntax = JsonSyntaxOptions.Json5,
        });
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void FindPathTest() {
        string Text = """
            {
              "first": 1,
              "second": {
                "third": 5
              }
            }
            """;

        using HjsonStream HjsonStream = new(Text, new HjsonStreamOptions() {
            Syntax = JsonSyntaxOptions.Json,
        });
        Assert.True(HjsonStream.FindPath(["second"]));
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(5, Element.GetProperty("third").Deserialize<int>(JsonOptions.Mini));
    }
}
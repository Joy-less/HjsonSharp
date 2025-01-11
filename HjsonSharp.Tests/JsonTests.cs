using System.Text.Json;

namespace HjsonSharp.Tests;

public class JsonTests {
    [Fact]
    public void ParseBasicObjectTest() {
        string Text = """
            {
              "first": 1,
              "second": 2
            }
            """;

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void ParseBasicArrayTest() {
        string Text = """
            [
                1,
                2, 3
            ]
            """;

        int[] Array = HjsonStream.ParseElement<int[]>(Text, HjsonStreamOptions.Json)!;
        Assert.Equal([1, 2, 3], Array);
    }
    [Fact]
    public void FindPropertyNameTest() {
        string Text = """
            {
              "first": 1,
              "second": {
                "third": 5
              }
            }
            """;

        using HjsonStream HjsonStream = new(Text, HjsonStreamOptions.Json);
        Assert.True(HjsonStream.FindPath("second"));
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(5, Element.GetProperty("third").Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void FindArrayIndexTest() {
        string Text = """
            [
              1,
              4,
              5
            ]
            """;

        using HjsonStream HjsonStream = new(Text, HjsonStreamOptions.Json);
        Assert.True(HjsonStream.FindPath(2));
        JsonElement Element = HjsonStream.ParseElement<JsonElement>();
        Assert.Equal(5, Element.Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void TrailingCommasTest() {
        string Text = """
            {
              "a": 1,
            }
            """;

        Assert.Throws<HjsonException>(() => HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Json));
    }
}
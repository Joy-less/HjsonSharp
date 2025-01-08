using System.Text.Json;

namespace Hjson.NET.Tests;

public class HjsonTests {
    [Fact]
    public void HashStyleCommentsTest() {
        string Text = """
            {
              "first": 1, # This is a comment
              "second": 2 # This is another comment
            } # This is a final comment
            """;

        JsonElement Element = HjsonStream.ParseElement<JsonElement>(Text, HjsonStreamOptions.Hjson);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal(1, Element.GetProperty("first").Deserialize<int>(JsonOptions.Mini));
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
    }
}
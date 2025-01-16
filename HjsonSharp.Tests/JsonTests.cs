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

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json);
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

        int[] Array = HjsonReader.ParseElement<int[]>(Text, HjsonReaderOptions.Json)!;
        Assert.Equal([1, 2, 3], Array);
    }
    [Fact]
    public void ParseNestedArrayTest() {
        string Text = """
            {
              "first": [
                1,
                2,
                3
              ],
              "second": 2
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json);
        Assert.Equal(2, Element.GetPropertyCount());
        Assert.Equal([1, 2, 3], Element.GetProperty("first").Deserialize<int[]>(JsonOptions.Mini)!);
        Assert.Equal(2, Element.GetProperty("second").Deserialize<int>(JsonOptions.Mini));
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

        using HjsonReader HjsonReader = new(Text, HjsonReaderOptions.Json);
        Assert.True(HjsonReader.FindPath("second"));
        JsonElement Element = HjsonReader.ParseElement<JsonElement>();
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

        using HjsonReader HjsonReader = new(Text, HjsonReaderOptions.Json);
        Assert.True(HjsonReader.FindPath(2));
        JsonElement Element = HjsonReader.ParseElement<JsonElement>();
        Assert.Equal(5, Element.Deserialize<int>(JsonOptions.Mini));
    }
    [Fact]
    public void TrailingCommasTest() {
        string Text = """
            {
              "a": 1,
            }
            """;

        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json));
    }
    [Fact]
    public void ExponentTest() {
        string Text = """
            {
              "1000": 10e3,
              "2000": 2.0E-3,
              "-3500": -35e3
            }
            """;

        JsonElement Element = HjsonReader.ParseElement<JsonElement>(Text, HjsonReaderOptions.Json);
        Assert.Equal(3, Element.GetPropertyCount());
        Assert.Equal(10e3, Element.GetProperty("1000").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(2.0E-3, Element.GetProperty("2000").Deserialize<double>(JsonOptions.Mini));
        Assert.Equal(-35e3, Element.GetProperty("-3500").Deserialize<double>(JsonOptions.Mini));
    }
    [Fact]
    public void ErroneousNumberTest() {
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<double>("-", HjsonReaderOptions.Json));
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<double>("+", HjsonReaderOptions.Json));
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<double>("-.", HjsonReaderOptions.Json));
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<double>(".", HjsonReaderOptions.Json));
    }
    [Fact]
    public void LeadingZeroTest() {
        Assert.Equal(0, HjsonReader.ParseElement<int>("0", HjsonReaderOptions.Json));
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<int>("01", HjsonReaderOptions.Json));
        Assert.Throws<HjsonException>(() => HjsonReader.ParseElement<int>("001", HjsonReaderOptions.Json));
        Assert.Equal(0e0, HjsonReader.ParseElement<double>("0e0", HjsonReaderOptions.Json));
    }
    [Fact]
    public void StringEscapedHexSequences() {
        Assert.Equal("ç", HjsonReader.ParseElement<string>("""
            "\u00E7"
            """, HjsonReaderOptions.Json));
    }
}
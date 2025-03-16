using System.Text;

namespace HjsonSharp.Tests;

public class ApiTests {
    [Fact]
    public void ReadToEndStringTest() {
        CustomJsonReader HjsonReader = new("abcdef");
        HjsonReader.ReadToEnd().ShouldBe("abcdef");
    }
    [Fact]
    public void ReadToEndBytesTest() {
        CustomJsonReader HjsonReader = new(Encoding.UTF8.GetBytes("abcdef"));
        HjsonReader.ReadToEnd().ShouldBe("abcdef");
    }
    [Fact]
    public void ReadToEndStreamTest() {
        using MemoryStream MemoryStream = new(Encoding.UTF8.GetBytes("abcdef"));
        CustomJsonReader HjsonReader = new(MemoryStream);
        HjsonReader.ReadToEnd().ShouldBe("abcdef");
    }
    [Fact]
    public void ReadToEndListTest() {
        CustomJsonReader HjsonReader = new([new Rune('a'), new Rune('b'), new Rune('c'), new Rune('d'), new Rune('e'), new Rune('f')]);
        HjsonReader.ReadToEnd().ShouldBe("abcdef");
    }
}
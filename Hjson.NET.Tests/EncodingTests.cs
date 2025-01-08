using System.Text;

namespace Hjson.NET.Tests;

public class EncodingTests {
    [Fact]
    public void Utf8Test() {
        BaseTest(Encoding.UTF8);
    }
    [Fact]
    public void Utf16Test() {
        BaseTest(Encoding.Unicode);
    }
    [Fact]
    public void BigEndianUtf16Test() {
        BaseTest(Encoding.BigEndianUnicode);
    }
    [Fact]
    public void Utf32Test() {
        BaseTest(Encoding.UTF32);
    }
    [Fact]
    public void AsciiTest() {
        BaseTest(Encoding.ASCII, ShouldFail: true);
    }

    private static void BaseTest(Encoding Encoding, bool ShouldFail = false) {
        const string InputString = "こんにちは😀";
        string? Result = HjsonStream.ParseElement<string>('"' + InputString + '"', HjsonStreamOptions.Hjson with { StreamEncoding = Encoding });
        if (ShouldFail) {
            Assert.NotEqual(InputString, Result);
        }
        else {
            Assert.Equal(InputString, Result);
        }
    }
}
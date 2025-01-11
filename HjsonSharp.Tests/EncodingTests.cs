using System.Text;

namespace HjsonSharp.Tests;

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
    [Fact]
    public void Utf8PreambleTest() {
        BasePreambleTest(Encoding.UTF8);
    }
    [Fact]
    public void Utf16PreambleTest() {
        BasePreambleTest(Encoding.Unicode);
    }
    [Fact]
    public void BigEndianUtf16PreambleTest() {
        BasePreambleTest(Encoding.BigEndianUnicode);
    }
    [Fact]
    public void Utf32PreambleTest() {
        BasePreambleTest(Encoding.UTF32);
    }
    [Fact]
    public void AsciiPreambleTest() {
        BasePreambleTest(Encoding.ASCII, ShouldFail: true);
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
    private static void BasePreambleTest(Encoding Encoding, bool ShouldFail = false) {
        const string InputString = "私";
        string? Result = HjsonStream.ParseElement<string>([.. Encoding.Preamble, .. Encoding.GetBytes($"\"{InputString}\"")], HjsonStreamOptions.Hjson with { StreamEncoding = null });
        if (ShouldFail) {
            Assert.NotEqual(InputString, Result);
        }
        else {
            Assert.Equal(InputString, Result);
        }
    }
}
namespace HjsonSharp.Tests;

public class UnitTest1 {
    [Fact]
    public void JsonTest() {
        string Text = """
            {
              "first": 1,
              "second": 2,
            }
            """;
        using HjsonStream HjsonStream = new(Text);
        HjsonStream.ReadObject();
    }
}

public class Player {
    public required string Name;
}
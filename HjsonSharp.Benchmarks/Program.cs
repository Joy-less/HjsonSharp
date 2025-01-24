using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Hjson;
using System.Text.Json;

namespace HjsonSharp.Benchmarks;

public class Program {
    public static void Main() {
        BenchmarkRunner.Run<HjsonSharpVsHjsonCsBenchmarks>();
        //ProfilePerformance();
    }
    public static void ProfilePerformance() {
        HjsonSharpVsHjsonCsBenchmarks Benchmarks = new();
        while (true) {
            Benchmarks.LongStringHjsonSharp();
            Benchmarks.ShortIntegerHjsonSharp();
            Benchmarks.PersonHjsonSharp();
        }
    }
}

[MemoryDiagnoser]
public class HjsonSharpVsHjsonCsBenchmarks {
    #region Long String

    private static readonly string LongString = new('a', 1_000_000);
    private static readonly string LongStringJson = '"' + LongString + '"';

    [Benchmark]
    public void LongStringHjsonCs() {
        string Result = (string)HjsonValue.Parse(LongStringJson);
        if (Result != LongString) {
            throw new Exception();
        }
    }
    [Benchmark]
    public void LongStringHjsonSharp() {
        string Result = JsonReader.ParseElement<string>(LongStringJson).Value!;
        if (Result != LongString) {
            throw new Exception();
        }
    }

    #endregion

    #region Short Integer

    private static readonly int ShortInteger = 12345;
    private static readonly string ShortIntegerJson = ShortInteger.ToString();

    [Benchmark]
    public void ShortIntegerHjsonCs() {
        int Result = (int)HjsonValue.Parse(ShortIntegerJson);
        if (Result != ShortInteger) {
            throw new Exception();
        }
    }
    [Benchmark]
    public void ShortIntegerHjsonSharp() {
        int Result = JsonReader.ParseElement<int>(ShortIntegerJson).Value!;
        if (Result != ShortInteger) {
            throw new Exception();
        }
    }

    #endregion

    #region Person

    private static readonly Person Person = new() {
        Name = "John Doe",
        Age = 30,
        Gender = "M",
        Email = "johndoe1234567890!!!@example.com",
        HeightMeters = 1.8,
    };
    private static readonly string PersonJson = JsonSerializer.Serialize(Person);

    [Benchmark]
    public void PersonHjsonCs() {
        Person Result = JsonSerializer.Deserialize<Person>(HjsonValue.Parse(PersonJson).ToString(Stringify.Plain))!;
        if (Result != Person) {
            throw new Exception();
        }
    }
    [Benchmark]
    public void PersonHjsonSharp() {
        Person Result = JsonReader.ParseElement<Person>(PersonJson).Value!;
        if (Result != Person) {
            throw new Exception();
        }
    }

    #endregion
}

public record Person {
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required string Gender { get; set; }
    public required string Email { get; set; }
    public required double HeightMeters { get; set; }
}
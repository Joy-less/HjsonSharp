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
            Benchmarks.ParseLongStringHjsonSharp();
            Benchmarks.ParseShortIntegerRepeatedHjsonSharp();
            Benchmarks.ParsePersonHjsonSharp();
        }
    }
}

[MemoryDiagnoser]
public class HjsonSharpVsHjsonCsBenchmarks {
    private static readonly string LongString = new string('a', 1_000_000);
    private static readonly string LongStringJson = '"' + LongString + '"';

    [Benchmark]
    public void LongStringHjsonCs() {
        string Result = (string)HjsonValue.Parse(LongStringJson);
        if (Result != LongString) {
            throw new Exception();
        }
    }
    [Benchmark]
    public void ParseLongStringHjsonSharp() {
        string Result = HjsonStream.ParseElement<string>(LongStringJson)!;
        if (Result != LongString) {
            throw new Exception();
        }
    }

    private static readonly int ShortInteger = 123;
    private static readonly string ShortIntegerJson = ShortInteger.ToString();
    private static readonly int ShortIntegerIterations = 1_000_000;

    [Benchmark]
    public void ParseShortIntegerRepeatedHjsonCs() {
        for (int Counter = 0; Counter < ShortIntegerIterations; Counter++) {
            int Result = (int)HjsonValue.Parse(ShortIntegerJson);
            if (Result != ShortInteger) {
                throw new Exception();
            }
        }
    }
    [Benchmark]
    public void ParseShortIntegerRepeatedHjsonSharp() {
        for (int Counter = 0; Counter < ShortIntegerIterations; Counter++) {
            int Result = HjsonStream.ParseElement<int>(ShortIntegerJson)!;
            if (Result != ShortInteger) {
                throw new Exception();
            }
        }
    }

    private static readonly Person Person = new() {
        Name = "John Doe",
        Age = 30,
        Gender = "M",
    };
    private static readonly string PersonJson = JsonSerializer.Serialize(Person);

    [Benchmark]
    public void ParsePersonHjsonCs() {
        Person Result = JsonSerializer.Deserialize<Person>(HjsonValue.Parse(PersonJson).ToString(Stringify.Plain))!;
        if (Result != Person) {
            throw new Exception();
        }
    }
    [Benchmark]
    public void ParsePersonHjsonSharp() {
        Person Result = HjsonStream.ParseElement<Person>(PersonJson)!;
        if (Result != Person) {
            throw new Exception();
        }
    }
}

public record Person {
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required string Gender { get; set; }
}
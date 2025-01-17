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

    private const int LongStringIterations = 1;

    private static readonly string LongString = new('a', 1_000_000);
    private static readonly string LongStringJson = '"' + LongString + '"';

    [Benchmark(OperationsPerInvoke = LongStringIterations)]
    public void LongStringHjsonCs() {
        for (int Counter = 0; Counter < LongStringIterations; Counter++) {
            string Result = (string)HjsonValue.Parse(LongStringJson);
            if (Result != LongString) {
                throw new Exception();
            }
        }
    }
    [Benchmark(OperationsPerInvoke = LongStringIterations)]
    public void LongStringHjsonSharp() {
        for (int Counter = 0; Counter < LongStringIterations; Counter++) {
            string Result = HjsonReader.ParseElement<string>(LongStringJson).Value!;
            if (Result != LongString) {
                throw new Exception();
            }
        }
    }

    #endregion

    #region Short Integer

    private const int ShortIntegerIterations = 1_000_000;

    private static readonly int ShortInteger = 12345;
    private static readonly string ShortIntegerJson = ShortInteger.ToString();

    [Benchmark(OperationsPerInvoke = ShortIntegerIterations)]
    public void ShortIntegerHjsonCs() {
        for (int Counter = 0; Counter < ShortIntegerIterations; Counter++) {
            int Result = (int)HjsonValue.Parse(ShortIntegerJson);
            if (Result != ShortInteger) {
                throw new Exception();
            }
        }
    }
    [Benchmark(OperationsPerInvoke = ShortIntegerIterations)]
    public void ShortIntegerHjsonSharp() {
        for (int Counter = 0; Counter < ShortIntegerIterations; Counter++) {
            int Result = HjsonReader.ParseElement<int>(ShortIntegerJson).Value!;
            if (Result != ShortInteger) {
                throw new Exception();
            }
        }
    }

    #endregion

    #region Person

    private const int PersonIterations = 100_000;

    private static readonly Person Person = new() {
        Name = "John Doe",
        Age = 30,
        Gender = "M",
        Email = "johndoe1234567890!!!@example.com",
        HeightMeters = 1.8,
    };
    private static readonly string PersonJson = JsonSerializer.Serialize(Person);

    [Benchmark(OperationsPerInvoke = PersonIterations)]
    public void PersonHjsonCs() {
        for (int Counter = 0; Counter < PersonIterations; Counter++) {
            Person Result = JsonSerializer.Deserialize<Person>(HjsonValue.Parse(PersonJson).ToString(Stringify.Plain))!;
            if (Result != Person) {
                throw new Exception();
            }
        }
    }
    [Benchmark(OperationsPerInvoke = PersonIterations)]
    public void PersonHjsonSharp() {
        for (int Counter = 0; Counter < PersonIterations; Counter++) {
            Person Result = HjsonReader.ParseElement<Person>(PersonJson).Value!;
            if (Result != Person) {
                throw new Exception();
            }
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
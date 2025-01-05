using HjsonSharp;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Stopwatch Stopwatch = Stopwatch.StartNew();

// Write a 1GB string
using MemoryStream Stream = new();
//await Stream.WriteAsync(Encoding.UTF8.GetBytes("私は"));
await Stream.WriteAsync(Encoding.UTF8.GetBytes("\"わ𓅡𓅡𓅡𓅡𓅡𓅡𓅡"));
await Stream.WriteAsync(Encoding.UTF8.GetBytes(new string('4', 1_000_000_000)));
await Stream.WriteAsync(Encoding.UTF8.GetBytes("𓅡232𓅡\""));
Stream.Position = 0;
Console.WriteLine($"Written string in {Stopwatch.Elapsed}");
Stopwatch.Restart();

/*// 
StreamTextReader r = new(Stream, Encoding.UTF8);
r.Read();
char c = (char)r.Read();
_ = c;
long p = r.StreamPosition;
_ = p;
r.StreamPosition = 0;
c = (char)r.Peek();
_ = c;
p = r.StreamPosition;
_ = p;*/

/*// Read each character of the stream
Stream.Position = 0;
using StreamReader StreamReader2 = new(Stream);
while (StreamReader2.Read() >= 0) ;
Console.WriteLine($"Read each character of stream in {Stopwatch.Elapsed}");
Stopwatch.Restart();

// Read the stream with a buffer
Stream.Position = 0;
using StreamReader StreamReader3 = new(Stream);
char[] Buffer = new char[4096];
while (StreamReader3.Read(Buffer) > 0) ;
Console.WriteLine($"Read the stream with a buffer in {Stopwatch.Elapsed}");
Stopwatch.Restart();
}*/

/*// Read the stream with StreamTextReader
StreamTextReader StreamTextReader2 = new(Stream, Encoding.UTF8);
while (StreamTextReader2.Read() >= 0) if (StreamTextReader2.StreamPosition % 10_000_000 == 0) Console.WriteLine(StreamTextReader2.StreamPosition);
_ = StreamTextReader2.StreamPosition;
_ = StreamTextReader2.Stream.Position;
Console.WriteLine($"Read the stream with StreamTextReader in {Stopwatch.Elapsed}");
Stopwatch.Restart();*/

// Parse the string using IntegrityDB
Stream.Position = 0;
using HjsonStream JsonStream = new(Stream);
JsonStreamToken Token = JsonStream.ReadString();
Console.WriteLine($"IntegrityDB parsed string in {Stopwatch.Elapsed}");
Stopwatch.Restart();

// Parse the string using System.Text.Json
Stream.Position = 0;
JsonSerializer.Deserialize<JsonElement>(Stream);
Console.WriteLine($"System.Text.Json parsed string in {Stopwatch.Elapsed}");
Stopwatch.Restart();

Console.ReadLine();
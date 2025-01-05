namespace HjsonSharp;

public record struct HjsonStreamOptions {
    public int BufferSize { get; set; }

    public static HjsonStreamOptions Json => new() {
        BufferSize = 4096,
    };
    public static HjsonStreamOptions Jsonc => Json with {

    };
    public static HjsonStreamOptions Json5 => Jsonc with {

    };
    public static HjsonStreamOptions Hjson => Json5 with {

    };
}
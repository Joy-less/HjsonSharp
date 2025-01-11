namespace HjsonSharp;

/// <summary>
/// An error occurred when reading or writing HJSON.
/// </summary>
public class HjsonException(string? Message = null) : Exception(Message);
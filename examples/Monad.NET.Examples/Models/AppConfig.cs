namespace Monad.NET.Examples.Models;

/// <summary>
/// Application configuration for demonstrating Reader monad.
/// </summary>
public record AppConfig(
    string AppName,
    string ConnectionString,
    int MaxRetries,
    TimeSpan Timeout);

/// <summary>
/// Parsed configuration from text files.
/// </summary>
public record ParsedConfig(int Timeout, int MaxRetries, int BufferSize);


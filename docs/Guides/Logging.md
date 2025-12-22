# Logging Guidance

- **Writer for local aggregation**: Use `Writer<string, T>` for simple concatenated logs, `Writer<List<TLog>, T>` for structured entries.
- **Bridging to structured loggers**: After computing, flush logs to Serilog/NLog by iterating `Writer.Log`; keep `Writer` logs short-lived to avoid large allocations.
- **RemoteData/Result to ProblemDetails**: Prefer the ASP.NET Core extension methods to map errors; supply a policy function if you need custom error shapes.
- **Failure payloads**: Keep `Result.Err`/`RemoteData.Failure` payloads small and serializable; avoid embedding exceptions directly in HTTP responses.


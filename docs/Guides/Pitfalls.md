# Pitfalls & Gotchas

- **Validation vs Result**: LINQ query syntax (`from ... select`) uses `AndThen` under the hood, which **short-circuits on first error**. To accumulate all errors, use `Apply`, `Zip`, or `Combine` instead.
- **RemoteData guards**: `Unwrap()` throws for `NotAsked`/`Loading` states. Prefer `Match()` for exhaustive handling or `ToResult(notAskedError, loadingError)` to convert safely.
- **Try wrapping**: `Try.Of()` captures exceptions as values. If you need to propagate/rethrow, call `GetOrThrow()` explicitly.
- **Option.Some(null)**: Passing `null` to `Option.Some()` throws `ArgumentNullException`. Use `Option.None()` or the implicit conversion (which handles null safely).
- **Writer logs**: `Writer<string, T>` uses string concatenation (O(n²) for many writes). For structured logs or better performance, use `Writer<List<TLog>, T>` with list concatenation.


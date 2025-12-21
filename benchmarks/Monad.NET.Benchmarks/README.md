# Monad.NET Benchmarks

This project contains performance benchmarks for Monad.NET using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Running Benchmarks

### Run all benchmarks

```bash
dotnet run -c Release
```

### Run specific benchmark class

```bash
dotnet run -c Release -- --filter "*OptionBenchmarks*"
dotnet run -c Release -- --filter "*ResultBenchmarks*"
dotnet run -c Release -- --filter "*ComparisonBenchmarks*"
```

### Quick test run (fewer iterations)

```bash
dotnet run -c Release -- --job short
```

### Export results

```bash
dotnet run -c Release -- --exporters json markdown html
```

## Benchmark Classes

| Class | Description |
|-------|-------------|
| `OptionBenchmarks` | Benchmarks for `Option<T>` operations |
| `ResultBenchmarks` | Benchmarks for `Result<T, E>` operations |
| `EitherBenchmarks` | Benchmarks for `Either<L, R>` operations |
| `TryBenchmarks` | Benchmarks for `Try<T>` operations |
| `NonEmptyListBenchmarks` | Benchmarks for `NonEmptyList<T>` operations |
| `ComparisonBenchmarks` | Compares Monad.NET with nullable types and exceptions |

## Interpreting Results

- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation
- **Gen0/Gen1/Gen2**: GC collections per 1000 operations
- **Allocated**: Allocated memory per operation

## Key Metrics to Watch

1. **Zero allocations** - Struct-based monads should not allocate on heap
2. **Consistent performance** - Success and failure paths should have similar performance
3. **Competitive with alternatives** - Should be comparable to nullable/exceptions

## Sample Output

```
|        Method |      Mean |     Error |    StdDev | Allocated |
|-------------- |----------:|----------:|----------:|----------:|
|    CreateSome |  1.234 ns | 0.0123 ns | 0.0115 ns |         - |
|    CreateNone |  0.123 ns | 0.0012 ns | 0.0011 ns |         - |
|      Map_Some |  2.345 ns | 0.0234 ns | 0.0219 ns |         - |
```

The `-` in Allocated means zero heap allocations (good!).


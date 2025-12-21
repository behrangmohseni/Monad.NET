using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Monad.NET.Benchmarks;

/// <summary>
/// Benchmarks comparing Monad.NET with nullable reference types and exceptions.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ComparisonBenchmarks
{
    private const int Iterations = 1000;

    #region Option vs Nullable

    [Benchmark(Baseline = true)]
    public int Option_Pipeline()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var result = Option<int>.Some(i)
                .Map(x => x * 2)
                .Filter(x => x % 4 == 0)
                .Map(x => x + 1)
                .UnwrapOr(0);
            sum += result;
        }
        return sum;
    }

    [Benchmark]
    public int Nullable_Pipeline()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            int? value = i;
            value = value * 2;
            if (value % 4 != 0) value = null;
            value = value + 1;
            sum += value ?? 0;
        }
        return sum;
    }

    #endregion

    #region Result vs Exceptions

    [Benchmark]
    public int Result_HappyPath()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var result = Divide(100, i + 1)
                .Map(x => x * 2)
                .AndThen(x => Divide(x, 2))
                .UnwrapOr(0);
            sum += result;
        }
        return sum;
    }

    [Benchmark]
    public int Exception_HappyPath()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            try
            {
                var x = DivideThrows(100, i + 1);
                x = x * 2;
                x = DivideThrows(x, 2);
                sum += x;
            }
            catch
            {
                sum += 0;
            }
        }
        return sum;
    }

    [Benchmark]
    public int Result_WithErrors()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var divisor = i % 10 == 0 ? 0 : i + 1;
            var result = Divide(100, divisor)
                .Map(x => x * 2)
                .UnwrapOr(0);
            sum += result;
        }
        return sum;
    }

    [Benchmark]
    public int Exception_WithErrors()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            try
            {
                var divisor = i % 10 == 0 ? 0 : i + 1;
                var x = DivideThrows(100, divisor);
                x = x * 2;
                sum += x;
            }
            catch
            {
                sum += 0;
            }
        }
        return sum;
    }

    #endregion

    #region Try vs Try-Catch

    [Benchmark]
    public int Try_Of_NoError()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var result = Try<int>.Of(() => i * 2)
                .Map(x => x + 1)
                .GetOrElse(0);
            sum += result;
        }
        return sum;
    }

    [Benchmark]
    public int TryCatch_NoError()
    {
        var sum = 0;
        for (var i = 0; i < Iterations; i++)
        {
            try
            {
                var x = i * 2;
                x = x + 1;
                sum += x;
            }
            catch
            {
                sum += 0;
            }
        }
        return sum;
    }

    #endregion

    #region Helper Methods

    private static Result<int, string> Divide(int a, int b)
    {
        return b == 0
            ? Result<int, string>.Err("Division by zero")
            : Result<int, string>.Ok(a / b);
    }

    private static int DivideThrows(int a, int b)
    {
        if (b == 0) throw new DivideByZeroException();
        return a / b;
    }

    #endregion
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Monad.NET.SourceGenerators;
using Xunit;

namespace Monad.NET.SourceGenerators.Tests;

public class UnionGeneratorTests
{
    [Fact]
    public void Generator_WithValidUnion_GeneratesMatchMethods()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Shape
            {
                public partial record Circle(double Radius) : Shape;
                public partial record Rectangle(double Width, double Height) : Shape;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Match<TResult>", output);
        Assert.Contains("public void Match(", output);
    }

    [Fact]
    public void Generator_WithThreeCases_GeneratesAllParameters()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record TrafficLight
            {
                public partial record Red() : TrafficLight;
                public partial record Yellow() : TrafficLight;
                public partial record Green() : TrafficLight;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("red", output);
        Assert.Contains("yellow", output);
        Assert.Contains("green", output);
    }

    [Fact]
    public void Generator_WithClass_GeneratesMatchMethods()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial class Expression
            {
                public partial class Literal : Expression
                {
                    public int Value { get; }
                    public Literal(int value) => Value = value;
                }
                
                public partial class Add : Expression
                {
                    public Expression Left { get; }
                    public Expression Right { get; }
                    public Add(Expression left, Expression right) 
                    { 
                        Left = left; 
                        Right = right; 
                    }
                }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Match<TResult>", output);
        Assert.Contains("literal", output);
        Assert.Contains("add", output);
    }

    [Fact]
    public void Generator_WithGlobalNamespace_GeneratesWithoutNamespace()
    {
        var source = """
            using Monad.NET;

            [Union]
            public abstract partial record SimpleUnion
            {
                public partial record CaseA() : SimpleUnion;
                public partial record CaseB() : SimpleUnion;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Match<TResult>", output);
        Assert.DoesNotContain("namespace", output.Split('\n').Where(l => !l.TrimStart().StartsWith("//")).First(l => l.Contains("Match") || l.Contains("partial")));
    }

    [Fact]
    public void Generator_WithNonAbstractType_DoesNotGenerate()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public partial record NotAbstract
            {
                public partial record CaseA() : NotAbstract;
            }
            """;

        var (_, output) = RunGenerator(source);

        // Should only contain the attribute, not the generated Match methods
        Assert.DoesNotContain("Match<TResult>", output);
    }

    [Fact]
    public void Generator_WithNonPartialType_DoesNotGenerate()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract record NotPartial
            {
                public record CaseA() : NotPartial;
            }
            """;

        var (_, output) = RunGenerator(source);

        // Should only contain the attribute
        Assert.DoesNotContain("Match<TResult>", output);
    }

    [Fact]
    public void Generator_WithNoCases_DoesNotGenerate()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record EmptyUnion
            {
                // No cases
            }
            """;

        var (_, output) = RunGenerator(source);

        Assert.DoesNotContain("Match<TResult>", output);
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new UnionGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();
        var generatedOutput = string.Join("\n", runResult.GeneratedTrees.Select(t => t.ToString()));

        return (diagnostics, generatedOutput);
    }
}


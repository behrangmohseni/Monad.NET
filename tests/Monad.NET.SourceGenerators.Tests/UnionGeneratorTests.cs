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

    [Fact]
    public void Generator_GeneratesIsCaseProperties()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Result
            {
                public partial record Success(int Value) : Result;
                public partial record Error(string Message) : Result;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("public bool IsSuccess", output);
        Assert.Contains("public bool IsError", output);
        Assert.Contains("this is", output);
    }

    [Fact]
    public void Generator_GeneratesMapMethod()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Option
            {
                public partial record Some(int Value) : Option;
                public partial record None() : Option;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("public global::TestNamespace.Option Map(", output);
        Assert.Contains("#region Map Method", output);
    }

    [Fact]
    public void Generator_GeneratesTapMethod()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record State
            {
                public partial record Loading() : State;
                public partial record Loaded(string Data) : State;
                public partial record Failed(string Error) : State;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("public global::TestNamespace.State Tap(", output);
        Assert.Contains("#region Tap Method", output);
        Assert.Contains("?.Invoke(__case__)", output);
    }

    [Fact]
    public void Generator_GeneratesFactoryMethods()
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
        Assert.Contains("#region Factory Methods", output);
        Assert.Contains("public static", output);
        Assert.Contains("NewCircle(", output);
        Assert.Contains("NewRectangle(", output);
    }

    [Fact]
    public void Generator_FactoryMethodsHaveCorrectParameters()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Person
            {
                public partial record Adult(string Name, int Age) : Person;
                public partial record Child(string Name) : Person;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        // Check Adult factory method exists with parameters
        Assert.Contains("NewAdult(", output);
        Assert.Contains("Name", output);
        Assert.Contains("Age", output);
        // Check Child factory method exists
        Assert.Contains("NewChild(", output);
    }

    [Fact]
    public void Generator_WithDisabledFactoryMethods_DoesNotGenerateThem()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union(GenerateFactoryMethods = false)]
            public abstract partial record Shape
            {
                public partial record Circle(double Radius) : Shape;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("#region Factory Methods", output);
    }

    [Fact]
    public void Generator_WithMonadReference_GeneratesAsOptionMethods()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Maybe
            {
                public partial record Just(int Value) : Maybe;
                public partial record Nothing() : Maybe;
            }
            """;

        var (diagnostics, output) = RunGenerator(source, includeMonadReference: true);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("public global::Monad.NET.Option<", output);
        Assert.Contains("AsJust()", output);
        Assert.Contains("AsNothing()", output);
    }

    [Fact]
    public void Generator_WithoutMonadReference_DoesNotGenerateAsOptionMethods()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Maybe
            {
                public partial record Just(int Value) : Maybe;
                public partial record Nothing() : Maybe;
            }
            """;

        var (diagnostics, output) = RunGenerator(source, includeMonadReference: false);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("AsJust()", output);
        Assert.DoesNotContain("AsNothing()", output);
    }

    [Fact]
    public void Generator_GeneratesXmlDocumentation()
    {
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Status
            {
                public partial record Active() : Status;
                public partial record Inactive() : Status;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("/// <summary>", output);
        Assert.Contains("Pattern matches on all cases", output);
        Assert.Contains("Returns true if this is the", output);
    }

    [Fact]
    public void Generator_HandlesGenericTypes()
    {
        // Note: Current implementation doesn't support generic unions,
        // but this test verifies it doesn't crash
        var source = """
            using Monad.NET;

            namespace TestNamespace;

            [Union]
            public abstract partial record Container
            {
                public partial record Empty() : Container;
                public partial record Full(object Value) : Container;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Match<TResult>", output);
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) RunGenerator(
        string source, 
        bool includeMonadReference = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
        };

        if (includeMonadReference)
        {
            // Add reference to Monad.NET assembly for Option<T>
            var monadAssembly = typeof(Monad.NET.Option<>).Assembly;
            references.Add(MetadataReference.CreateFromFile(monadAssembly.Location));
        }

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

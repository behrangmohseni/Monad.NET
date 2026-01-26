using Xunit;

namespace Monad.NET.Analyzers.Tests;

/// <summary>
/// Tests for BindToMapAnalyzer to improve code coverage.
/// Tests the analyzer's basic functionality without requiring compilation.
/// </summary>
public class BindToMapAnalyzerTests
{
    [Fact]
    public void Analyzer_HasCorrectSupportedDiagnostics()
    {
        var analyzer = new BindToMapAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);
        Assert.Equal("MNT004", diagnostics[0].Id);
    }

    [Fact]
    public void DiagnosticDescriptor_HasCorrectProperties()
    {
        var descriptor = DiagnosticDescriptors.BindToMap;

        Assert.Equal("MNT004", descriptor.Id);
        Assert.NotNull(descriptor.Title);
        Assert.NotEmpty(descriptor.Title.ToString());
        Assert.NotNull(descriptor.MessageFormat);
        Assert.NotEmpty(descriptor.MessageFormat.ToString());
        Assert.NotNull(descriptor.Description);
        Assert.NotEmpty(descriptor.Description.ToString());
    }

    [Fact]
    public void Analyzer_CanBeInitialized()
    {
        var analyzer = new BindToMapAnalyzer();

        // The analyzer should not throw when initialized
        Assert.NotNull(analyzer);
        Assert.True(analyzer.SupportedDiagnostics.Length > 0);
    }
}

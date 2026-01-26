using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Monad.NET.Analyzers.Tests;

/// <summary>
/// Integration tests that verify analyzers can be loaded and run.
/// </summary>
public class AnalyzerIntegrationTests
{
    [Fact]
    public void AllAnalyzers_AreLoadable()
    {
        var analyzers = new DiagnosticAnalyzer[]
        {
            new UncheckedUnwrapAnalyzer(),
            new RedundantMapChainAnalyzer(),
            new MapIdentityAnalyzer(),
            new FilterConstantAnalyzer(),
            new DoubleNegationAnalyzer(),
            new OptionNullComparisonAnalyzer(),
            new MapGetOrElseAnalyzer(),
            new NullableToSomeAnalyzer(),
            new BindToMapAnalyzer(),
            new DiscardedMonadAnalyzer(),
            new ThrowInMatchAnalyzer()
        };

        Assert.Equal(11, analyzers.Length);
        Assert.All(analyzers, a => Assert.NotEmpty(a.SupportedDiagnostics));
    }

    [Fact]
    public void DiagnosticDescriptors_AreConfiguredCorrectly()
    {
        var descriptors = new[]
        {
            DiagnosticDescriptors.UncheckedUnwrap,
            DiagnosticDescriptors.RedundantMapChain,
            DiagnosticDescriptors.MapGetOrElseToMatch,
            DiagnosticDescriptors.BindToMap,
            DiagnosticDescriptors.DiscardedMonad,
            DiagnosticDescriptors.ThrowInMatch,
            DiagnosticDescriptors.NullableToSome,
            DiagnosticDescriptors.FilterConstant,
            DiagnosticDescriptors.MapIdentity,
            DiagnosticDescriptors.OptionNullComparison,
            DiagnosticDescriptors.DoubleNegation
        };

        Assert.All(descriptors, d =>
        {
            Assert.StartsWith("MNT", d.Id);
            Assert.NotEmpty(d.Title.ToString());
            Assert.NotEmpty(d.MessageFormat.ToString());
            Assert.NotEmpty(d.Description.ToString());
        });
    }

    [Fact]
    public void UncheckedUnwrapAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new UncheckedUnwrapAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT001");
    }

    [Fact]
    public void RedundantMapChainAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new RedundantMapChainAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT002");
    }

    [Fact]
    public void MapGetOrElseAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new MapGetOrElseAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT003");
    }

    [Fact]
    public void BindToMapAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new BindToMapAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT004");
    }

    [Fact]
    public void DiscardedMonadAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new DiscardedMonadAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT005");
    }

    [Fact]
    public void ThrowInMatchAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new ThrowInMatchAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT006");
    }

    [Fact]
    public void NullableToSomeAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new NullableToSomeAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT007");
    }

    [Fact]
    public void FilterConstantAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new FilterConstantAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT008");
    }

    [Fact]
    public void MapIdentityAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new MapIdentityAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT009");
    }

    [Fact]
    public void OptionNullComparisonAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new OptionNullComparisonAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT010");
    }

    [Fact]
    public void DoubleNegationAnalyzer_SupportsExpectedDiagnostic()
    {
        var analyzer = new DoubleNegationAnalyzer();
        Assert.Contains(analyzer.SupportedDiagnostics, d => d.Id == "MNT012");
    }
}

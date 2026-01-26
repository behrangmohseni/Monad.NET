using Microsoft.EntityFrameworkCore;
using Monad.NET;
using Monad.NET.EntityFrameworkCore;
using Xunit;

namespace Monad.NET.EntityFrameworkCore.Tests;

public class QueryableExtensionsTests : IDisposable
{
    private readonly QueryTestDbContext _context;

    public QueryableExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<QueryTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QueryTestDbContext(options);
        _context.Database.EnsureCreated();

        // Seed data
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Widget", Price = 10.00m },
            new Product { Id = 2, Name = "Gadget", Price = 25.00m },
            new Product { Id = 3, Name = "Doohickey", Price = 15.00m }
        );
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void FirstOrNone_WithElements_ReturnsSome()
    {
        var result = _context.Products.FirstOrNone();

        Assert.True(result.IsSome);
    }

    [Fact]
    public void FirstOrNone_WithEmptySequence_ReturnsNone()
    {
        var result = _context.Products.Where(p => p.Price > 1000).FirstOrNone();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void FirstOrNone_WithPredicate_ReturnsSome()
    {
        var result = _context.Products.FirstOrNone(p => p.Price > 20);

        Assert.True(result.IsSome);
        Assert.Equal("Gadget", result.GetValue().Name);
    }

    [Fact]
    public void FirstOrNone_WithPredicateNoMatch_ReturnsNone()
    {
        var result = _context.Products.FirstOrNone(p => p.Price > 1000);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void SingleOrNone_WithSingleElement_ReturnsSome()
    {
        var result = _context.Products.Where(p => p.Name == "Widget").SingleOrNone();

        Assert.True(result.IsSome);
        Assert.Equal("Widget", result.GetValue().Name);
    }

    [Fact]
    public void SingleOrNone_WithNoElements_ReturnsNone()
    {
        var result = _context.Products.Where(p => p.Name == "NonExistent").SingleOrNone();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void SingleOrNone_WithMultipleElements_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _context.Products.SingleOrNone());
    }

    [Fact]
    public void ElementAtOrNone_ValidIndex_ReturnsSome()
    {
        var result = _context.Products.OrderBy(p => p.Id).ElementAtOrNone(1);

        Assert.True(result.IsSome);
        Assert.Equal("Gadget", result.GetValue().Name);
    }

    [Fact]
    public void ElementAtOrNone_InvalidIndex_ReturnsNone()
    {
        var result = _context.Products.ElementAtOrNone(100);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void ElementAtOrNone_NegativeIndex_ReturnsNone()
    {
        var result = _context.Products.ElementAtOrNone(-1);

        Assert.True(result.IsNone);
    }

    [Fact]
    public void LastOrNone_WithElements_ReturnsSome()
    {
        var result = _context.Products.OrderBy(p => p.Id).LastOrNone();

        Assert.True(result.IsSome);
        Assert.Equal("Doohickey", result.GetValue().Name);
    }

    [Fact]
    public void LastOrNone_WithEmptySequence_ReturnsNone()
    {
        var result = _context.Products.Where(p => p.Price > 1000).LastOrNone();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task FirstOrNoneAsync_WithElements_ReturnsSome()
    {
        var result = await _context.Products.FirstOrNoneAsync();

        Assert.True(result.IsSome);
    }

    [Fact]
    public async Task FirstOrNoneAsync_WithEmptySequence_ReturnsNone()
    {
        var result = await _context.Products.Where(p => p.Price > 1000).FirstOrNoneAsync();

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task SingleOrNoneAsync_WithSingleElement_ReturnsSome()
    {
        var result = await _context.Products
            .Where(p => p.Name == "Widget")
            .SingleOrNoneAsync();

        Assert.True(result.IsSome);
        Assert.Equal("Widget", result.GetValue().Name);
    }

    [Fact]
    public async Task ElementAtOrNoneAsync_ValidIndex_ReturnsSome()
    {
        var result = await _context.Products
            .OrderBy(p => p.Id)
            .ElementAtOrNoneAsync(0);

        Assert.True(result.IsSome);
        Assert.Equal("Widget", result.GetValue().Name);
    }

    [Fact]
    public async Task LastOrNoneAsync_WithElements_ReturnsSome()
    {
        var result = await _context.Products
            .OrderBy(p => p.Id)
            .LastOrNoneAsync();

        Assert.True(result.IsSome);
        Assert.Equal("Doohickey", result.GetValue().Name);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public class QueryTestDbContext : DbContext
{
    public QueryTestDbContext(DbContextOptions<QueryTestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}


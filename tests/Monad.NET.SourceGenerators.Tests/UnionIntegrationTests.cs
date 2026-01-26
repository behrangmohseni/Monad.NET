using Monad.NET;
using Xunit;

namespace Monad.NET.SourceGenerators.Tests;

// These tests verify that the generated code actually works at runtime
public class UnionIntegrationTests
{
    [Fact]
    public void Match_WithCircle_ReturnsCircleArea()
    {
        Shape shape = new Shape.Circle(5.0);

        var area = shape.Match(
            circle: c => Math.PI * c.Radius * c.Radius,
            rectangle: r => r.Width * r.Height,
            triangle: t => 0.5 * t.Base * t.Height);

        Assert.Equal(Math.PI * 25, area);
    }

    [Fact]
    public void Match_WithRectangle_ReturnsRectangleArea()
    {
        Shape shape = new Shape.Rectangle(4.0, 5.0);

        var area = shape.Match(
            circle: c => Math.PI * c.Radius * c.Radius,
            rectangle: r => r.Width * r.Height,
            triangle: t => 0.5 * t.Base * t.Height);

        Assert.Equal(20.0, area);
    }

    [Fact]
    public void Match_WithTriangle_ReturnsTriangleArea()
    {
        Shape shape = new Shape.Triangle(6.0, 4.0);

        var area = shape.Match(
            circle: c => Math.PI * c.Radius * c.Radius,
            rectangle: r => r.Width * r.Height,
            triangle: t => 0.5 * t.Base * t.Height);

        Assert.Equal(12.0, area);
    }

    [Fact]
    public void MatchVoid_ExecutesCorrectAction()
    {
        Shape shape = new Shape.Circle(5.0);
        string result = "";

        shape.Match(
            circle: c => result = $"Circle with radius {c.Radius}",
            rectangle: r => result = $"Rectangle {r.Width}x{r.Height}",
            triangle: t => result = $"Triangle base={t.Base} height={t.Height}");

        Assert.Equal("Circle with radius 5", result);
    }

    [Fact]
    public void Match_WithPaymentMethod_WorksCorrectly()
    {
        PaymentMethod payment = new PaymentMethod.CreditCard("1234-5678-9012-3456", "12/25");

        var description = payment.Match(
            creditCard: cc => $"Credit card ending in {cc.Number[^4..]}",
            payPal: pp => $"PayPal: {pp.Email}",
            bankTransfer: bt => $"Bank: {bt.BankName} Account: {bt.AccountNumber}");

        Assert.Equal("Credit card ending in 3456", description);
    }

    [Fact]
    public void Match_WithHttpResponse_WorksCorrectly()
    {
        HttpResponse response = new HttpResponse.Ok("Success!");

        var message = response.Match(
            ok: o => $"200: {o.Body}",
            notFound: _ => "404: Not Found",
            serverError: e => $"500: {e.Message}");

        Assert.Equal("200: Success!", message);
    }

    [Fact]
    public void Match_WithExpression_EvaluatesCorrectly()
    {
        Expr expr = new Expr.Add(
            new Expr.Literal(10),
            new Expr.Multiply(
                new Expr.Literal(5),
                new Expr.Literal(3)));

        var result = Evaluate(expr);

        Assert.Equal(25, result); // 10 + (5 * 3)
    }

    private static int Evaluate(Expr expr)
    {
        return expr.Match(
            literal: l => l.Value,
            add: a => Evaluate(a.Left) + Evaluate(a.Right),
            multiply: m => Evaluate(m.Left) * Evaluate(m.Right));
    }

    #region Is{Case} Property Tests

    [Fact]
    public void IsCase_WithCircle_ReturnsCorrectValues()
    {
        Shape shape = new Shape.Circle(5.0);

        Assert.True(shape.IsCircle);
        Assert.False(shape.IsRectangle);
        Assert.False(shape.IsTriangle);
    }

    [Fact]
    public void IsCase_WithRectangle_ReturnsCorrectValues()
    {
        Shape shape = new Shape.Rectangle(4.0, 5.0);

        Assert.False(shape.IsCircle);
        Assert.True(shape.IsRectangle);
        Assert.False(shape.IsTriangle);
    }

    #endregion

    #region As{Case}() Method Tests

    [Fact]
    public void AsCase_WithMatchingCase_ReturnsSome()
    {
        Shape shape = new Shape.Circle(5.0);

        var result = shape.AsCircle();

        Assert.True(result.IsSome);
        Assert.Equal(5.0, result.GetValue().Radius);
    }

    [Fact]
    public void AsCase_WithNonMatchingCase_ReturnsNone()
    {
        Shape shape = new Shape.Circle(5.0);

        var result = shape.AsRectangle();

        Assert.True(result.IsNone);
    }

    [Fact]
    public void AsCase_CanBeUsedInPipeline()
    {
        Shape shape = new Shape.Circle(5.0);

        var area = shape.AsCircle()
            .Map(c => Math.PI * c.Radius * c.Radius)
            .GetValueOr(0);

        Assert.Equal(Math.PI * 25, area);
    }

    #endregion

    #region Map Method Tests

    [Fact]
    public void Map_TransformsCase()
    {
        Shape shape = new Shape.Circle(5.0);

        var doubled = shape.Map(
            circle: c => new Shape.Circle(c.Radius * 2),
            rectangle: r => new Shape.Rectangle(r.Width * 2, r.Height * 2),
            triangle: t => new Shape.Triangle(t.Base * 2, t.Height * 2));

        Assert.True(doubled.IsCircle);
        Assert.Equal(10.0, doubled.AsCircle().GetValue().Radius);
    }

    #endregion

    #region Tap Method Tests

    [Fact]
    public void Tap_ExecutesSideEffect()
    {
        Shape shape = new Shape.Circle(5.0);
        double? capturedRadius = null;

        var result = shape.Tap(
            circle: c => capturedRadius = c.Radius);

        Assert.Same(shape, result);
        Assert.Equal(5.0, capturedRadius);
    }

    [Fact]
    public void Tap_OnlyExecutesMatchingCase()
    {
        Shape shape = new Shape.Circle(5.0);
        bool rectangleExecuted = false;

        shape.Tap(
            rectangle: _ => rectangleExecuted = true);

        Assert.False(rectangleExecuted);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void NewFactoryMethod_CreatesInstance()
    {
        var circle = Shape.NewCircle(5.0);
        var rectangle = Shape.NewRectangle(4.0, 5.0);
        var triangle = Shape.NewTriangle(6.0, 4.0);

        Assert.IsType<Shape.Circle>(circle);
        Assert.Equal(5.0, circle.Radius);

        Assert.IsType<Shape.Rectangle>(rectangle);
        Assert.Equal(4.0, rectangle.Width);
        Assert.Equal(5.0, rectangle.Height);

        Assert.IsType<Shape.Triangle>(triangle);
        Assert.Equal(6.0, triangle.Base);
        Assert.Equal(4.0, triangle.Height);
    }

    #endregion
}

// Test union types - these get Match methods generated
[Union]
public abstract partial record Shape
{
    public partial record Circle(double Radius) : Shape;
    public partial record Rectangle(double Width, double Height) : Shape;
    public partial record Triangle(double Base, double Height) : Shape;
}

[Union]
public abstract partial record PaymentMethod
{
    public partial record CreditCard(string Number, string Expiry) : PaymentMethod;
    public partial record PayPal(string Email) : PaymentMethod;
    public partial record BankTransfer(string BankName, string AccountNumber) : PaymentMethod;
}

[Union]
public abstract partial record HttpResponse
{
    public partial record Ok(string Body) : HttpResponse;
    public partial record NotFound() : HttpResponse;
    public partial record ServerError(string Message) : HttpResponse;
}

[Union]
public abstract partial record Expr
{
    public partial record Literal(int Value) : Expr;
    public partial record Add(Expr Left, Expr Right) : Expr;
    public partial record Multiply(Expr Left, Expr Right) : Expr;
}


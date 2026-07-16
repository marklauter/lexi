using Math.Parsing;
using Math.Parsing.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Math.Parsing.Tests;

// All Console-capturing tests live in this single class so they run sequentially (xUnit serializes
// tests within a class), avoiding races on the process-global Console.Out. No other test class
// redirects Console output.
[ExcludeFromCodeCoverage]
public sealed class MathPrintTests
{
    private static string Capture(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }

        return writer.ToString();
    }

    [Theory]
    [InlineData(TokenIds.ADD, "+")]
    [InlineData(TokenIds.SUBTRACT, "-")]
    [InlineData(TokenIds.MULTIPLY, "*")]
    [InlineData(TokenIds.DIVIDE, "/")]
    [InlineData(TokenIds.MODULUS, "%")]
    public void BinaryOperation_Print_WritesOperatorAndOperands(uint operatorId, string glyph)
    {
        var expression = new BinaryOperation(
            new Number(NumericTypes.Integer, 1),
            new Number(NumericTypes.Integer, 2),
            operatorId);

        var output = Capture(() => expression.Print());

        Assert.Contains(nameof(BinaryOperation), output, StringComparison.Ordinal);
        Assert.Contains($"Op {glyph}", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Group_Print_WritesGroupAndInner()
    {
        var group = new Group(new Number(NumericTypes.Integer, 7));

        var output = Capture(() => group.Print());

        Assert.Contains(nameof(Group), output, StringComparison.Ordinal);
        Assert.Contains("Integer", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(NumericTypes.Integer, 42, "Integer: 42")]
    [InlineData(NumericTypes.FloatingPoint, 1.5, "FloatingPoint: 1.5")]
    [InlineData(NumericTypes.ScientificNotation, 0.0001, "ScientificNotation: 0.0001")]
    public void Number_Print_WritesTypeAndValue(NumericTypes type, double value, string expected)
    {
        var number = new Number(type, value);

        var output = Capture(() => number.Print());

        Assert.Contains(expected, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Number_Print_NotANumber_WritesNaN()
    {
        var number = new Number(NumericTypes.NotANumber, 0);

        var output = Capture(() => number.Print());

        Assert.Contains("NaN", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Number_Print_UnknownType_Throws() =>
        Assert.Throws<NotSupportedException>(() =>
            Capture(() => new Number((NumericTypes)99, 0).Print()));
}

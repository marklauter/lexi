using Math.Parsing.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Math.Parsing.Tests;

[ExcludeFromCodeCoverage]
public sealed class MathExpressionTests(Parser parser)
{
    private readonly Parser parser = parser ?? throw new ArgumentNullException(nameof(parser));

    [Theory]
    [InlineData("5 % 3", 2)]
    [InlineData("10 % 4", 2)]
    [InlineData("9 % 3", 0)]
    public void Parse_Modulus_Evaluates(string source, double expected)
    {
        var expression = parser.Parse(source);

        Assert.Equal(expected, expression.Evaluate());
    }

    [Fact]
    public void BinaryOperation_Evaluate_UnknownOperator_Throws()
    {
        var expression = new BinaryOperation(
            new Number(NumericTypes.Integer, 1),
            new Number(NumericTypes.Integer, 1),
            uint.MaxValue);

        _ = Assert.Throws<NotSupportedException>(() => expression.Evaluate());
    }

    [Fact]
    public void BinaryOperation_Children_YieldsLeftThenRight()
    {
        var left = new Number(NumericTypes.Integer, 1);
        var right = new Number(NumericTypes.Integer, 2);
        var expression = new BinaryOperation(left, right, TokenIds.ADD);

        Assert.Equal([left, right], expression.Children());
    }

    [Fact]
    public void Group_Children_YieldsInnerExpression()
    {
        var inner = new Number(NumericTypes.Integer, 1);
        var group = new Group(inner);

        Assert.Equal([inner], group.Children());
    }

    [Fact]
    public void Group_Evaluate_DelegatesToInner()
    {
        var group = new Group(new Number(NumericTypes.Integer, 42));

        Assert.Equal(42, group.Evaluate());
    }

    [Fact]
    public void Number_Children_IsEmpty()
    {
        var number = new Number(NumericTypes.Integer, 1);

        Assert.Empty(number.Children());
    }

    [Theory]
    [InlineData(NumericTypes.NotANumber, true)]
    [InlineData(NumericTypes.Integer, false)]
    [InlineData(NumericTypes.FloatingPoint, false)]
    [InlineData(NumericTypes.ScientificNotation, false)]
    public void Number_IsNaN_ReflectsType(NumericTypes type, bool expected)
    {
        var number = new Number(type, 0);

        Assert.Equal(expected, number.IsNaN());
    }
}

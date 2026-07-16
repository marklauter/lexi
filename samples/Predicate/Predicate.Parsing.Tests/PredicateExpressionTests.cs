using Predicate.Parsing.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Predicate.Parsing.Tests;

[ExcludeFromCodeCoverage]
public sealed class PredicateExpressionTests
{
    [Fact]
    public void Identifier_FromString_RoundTripsThroughToString()
    {
        var identifier = Identifier.FromString("Street");

        Assert.Equal("Street", identifier.Value);
        Assert.Equal("Street", identifier.ToString());
    }

    [Fact]
    public void Identifier_NullValue_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new Identifier(null!));

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"\"", "")]
    [InlineData("unquoted", "unquoted")]
    public void StringLiteral_FromString_StripsSurroundingQuotes(string input, string expected)
    {
        var literal = StringLiteral.FromString(input);

        Assert.Equal(expected, literal.Value);
        Assert.Equal(expected, literal.ToString());
    }

    [Fact]
    public void StringLiteral_NullInput_Throws() =>
        Assert.Throws<ArgumentNullException>(() => StringLiteral.FromString(null!));

    [Fact]
    public void StringLiteral_NullValue_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new StringLiteral(null!));

    [Theory]
    [InlineData("'a'", "a")]
    [InlineData("unquoted", "unquoted")]
    public void CharacterLiteral_FromString_StripsSurroundingApostrophes(string input, string expected)
    {
        var literal = CharacterLiteral.FromString(input);

        Assert.Equal(expected, literal.Value);
        Assert.Equal(expected, literal.ToString());
    }

    [Fact]
    public void CharacterLiteral_NullInput_Throws() =>
        Assert.Throws<ArgumentNullException>(() => CharacterLiteral.FromString(null!));

    [Fact]
    public void CharacterLiteral_NullValue_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new CharacterLiteral(null!));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BooleanLiteral_FromBoolean_RoundTrips(bool value)
    {
        var literal = BooleanLiteral.FromBoolean(value);

        Assert.Equal(value, literal.ToBoolean());
        Assert.Equal(value.ToString(), literal.ToString());
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    public void BooleanLiteral_FromString_ParsesBoolean(string input, bool expected) =>
        Assert.Equal(expected, BooleanLiteral.FromString(input).ToBoolean());

    [Fact]
    public void BooleanLiteral_FromString_NonBoolean_Throws() =>
        Assert.Throws<InvalidOperationException>(() => BooleanLiteral.FromString("maybe"));

    [Fact]
    public void BooleanLiteral_FromString_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => BooleanLiteral.FromString(null!));

    [Fact]
    public void NumericLiteral_FromInt32_IsIntegerTyped()
    {
        var literal = NumericLiteral.FromInt32(42);

        Assert.Equal(NumericTypes.Integer, literal.Type);
        Assert.Equal(42, literal.ToInt32());
        Assert.Equal("42", literal.ToString());
        Assert.False(literal.IsNaN());
    }

    [Fact]
    public void NumericLiteral_FromDouble_IsFloatingPointTyped()
    {
        var literal = NumericLiteral.FromDouble(1.5);

        Assert.Equal(NumericTypes.FloatingPoint, literal.Type);
        Assert.Equal(1.5, literal.ToDouble());
        Assert.Equal("1.5", literal.ToString());
    }

    [Theory]
    [InlineData(NumericTypes.Integer, 5, "5")]
    [InlineData(NumericTypes.FloatingPoint, 1.25, "1.25")]
    [InlineData(NumericTypes.ScientificNotation, 0.0001, "0.0001")]
    [InlineData(NumericTypes.NotANumber, 0, "NaN")]
    public void NumericLiteral_ToString_RendersPerType(NumericTypes type, double value, string expected) =>
        Assert.Equal(expected, new NumericLiteral(type, value).ToString());

    [Fact]
    public void NumericLiteral_NotANumber_IsNaN() =>
        Assert.True(new NumericLiteral(NumericTypes.NotANumber, 0).IsNaN());

    [Fact]
    public void NumericLiteral_UndefinedType_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new NumericLiteral((NumericTypes)999, 0));

    [Theory]
    [InlineData(TokenIds.FROM, Keywords.From, "from")]
    [InlineData(TokenIds.WHERE, Keywords.Where, "where")]
    [InlineData(TokenIds.SKIP, Keywords.Skip, "skip")]
    [InlineData(TokenIds.TAKE, Keywords.Take, "take")]
    public void Keyword_FromUInt32_MapsTokenToKeyword(uint tokenId, Keywords expected, string text)
    {
        var keyword = Keyword.FromUInt32(tokenId);

        Assert.Equal(expected, keyword.Value);
        Assert.Equal(expected, keyword.ToKeywords());
        Assert.Equal(text, keyword.ToString());
    }

    [Fact]
    public void Keyword_FromUInt32_UnknownToken_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Keyword.FromUInt32(uint.MaxValue));

    [Fact]
    public void Keyword_FromKeywords_RoundTrips() =>
        Assert.Equal(Keywords.Where, Keyword.FromKeywords(Keywords.Where).Value);

    [Fact]
    public void Keyword_UndefinedValue_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Keyword((Keywords)999));

    [Theory]
    [InlineData(TokenIds.EQUAL, ComparisonOperators.Equal)]
    [InlineData(TokenIds.NOT_EQUAL, ComparisonOperators.NotEqual)]
    [InlineData(TokenIds.LESS_THAN, ComparisonOperators.LessThan)]
    [InlineData(TokenIds.GREATER_THAN, ComparisonOperators.GreaterThan)]
    [InlineData(TokenIds.LESS_THAN_OR_EQUAL, ComparisonOperators.LessThanOrEqual)]
    [InlineData(TokenIds.GREATER_THAN_OR_EQUAL, ComparisonOperators.GreaterThanOrEqual)]
    [InlineData(TokenIds.STARTS_WITH, ComparisonOperators.StartsWith)]
    [InlineData(TokenIds.ENDS_WITH, ComparisonOperators.EndsWith)]
    [InlineData(TokenIds.CONTAINS, ComparisonOperators.Contains)]
    public void ComparisonOperator_FromUInt32_MapsTokenToOperator(uint tokenId, ComparisonOperators expected)
    {
        var op = ComparisonOperator.FromUInt32(tokenId);

        Assert.Equal(expected, op.Value);
        Assert.Equal(expected, op.ToComparisonOperators());
        Assert.Equal(expected.ToString(), op.ToString());
    }

    [Fact]
    public void ComparisonOperator_FromUInt32_UnknownToken_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ComparisonOperator.FromUInt32(uint.MaxValue));

    [Fact]
    public void ComparisonOperator_FromComparisonOperators_RoundTrips() =>
        Assert.Equal(
            ComparisonOperators.Contains,
            ComparisonOperator.FromComparisonOperators(ComparisonOperators.Contains).Value);

    [Fact]
    public void ComparisonOperator_UndefinedValue_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ComparisonOperator((ComparisonOperators)999));

    [Theory]
    [InlineData(ComparisonOperators.Equal)]
    [InlineData(ComparisonOperators.NotEqual)]
    [InlineData(ComparisonOperators.GreaterThan)]
    [InlineData(ComparisonOperators.GreaterThanOrEqual)]
    [InlineData(ComparisonOperators.LessThan)]
    [InlineData(ComparisonOperators.LessThanOrEqual)]
    [InlineData(ComparisonOperators.StartsWith)]
    [InlineData(ComparisonOperators.EndsWith)]
    [InlineData(ComparisonOperators.Contains)]
    public void ComparisonExpression_ToString_NamesOperator(ComparisonOperators op)
    {
        var expression = new ComparisonExpression(
            Identifier.FromString("x"),
            op,
            NumericLiteral.FromInt32(1));

        Assert.Equal(op.ToString(), expression.ToString());
    }

    [Fact]
    public void ComparisonExpression_UndefinedOperator_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ComparisonExpression(
            Identifier.FromString("x"),
            (ComparisonOperators)999,
            NumericLiteral.FromInt32(1)));

    [Theory]
    [InlineData(LogicalOperators.And)]
    [InlineData(LogicalOperators.Or)]
    public void LogicalExpression_ToString_NamesOperator(LogicalOperators op)
    {
        var expression = new LogicalExpression(
            Identifier.FromString("a"),
            op,
            Identifier.FromString("b"));

        Assert.Equal(op.ToString(), expression.ToString());
    }

    [Fact]
    public void LogicalExpression_UndefinedOperator_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogicalExpression(
            Identifier.FromString("a"),
            (LogicalOperators)999,
            Identifier.FromString("b")));

    [Fact]
    public void ParentheticalExpression_WrapsInner()
    {
        var inner = Identifier.FromString("x");
        var parenthetical = new ParentheticalExpression(inner);

        Assert.Equal(inner, parenthetical.Expression);
    }

    [Fact]
    public void Statement_HoldsItsParts()
    {
        var from = Identifier.FromString("T");
        var predicate = new ComparisonExpression(
            Identifier.FromString("x"),
            ComparisonOperators.Equal,
            NumericLiteral.FromInt32(1));
        var skip = NumericLiteral.FromInt32(1);
        var take = NumericLiteral.FromInt32(2);

        var statement = new Statement(from, predicate, skip, take);

        Assert.Equal(from, statement.From);
        Assert.Equal(predicate, statement.Predicate);
        Assert.Equal(skip, statement.Skip);
        Assert.Equal(take, statement.Take);
    }

    [Fact]
    public void NullLiteral_InstancesAreEqual() =>
        Assert.Equal(new NullLiteral(), new NullLiteral());
}

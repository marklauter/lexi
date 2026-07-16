using Predicate.Parsing.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Predicate.Parsing.Tests;

[ExcludeFromCodeCoverage]
public sealed class PredicateParserTests(Parser parser)
{
    private readonly Parser parser = parser ?? throw new ArgumentNullException(nameof(parser));

    private ComparisonExpression ParseComparison(string source)
    {
        var statement = parser.Parse(source);
        return Assert.IsType<ComparisonExpression>(statement.Predicate);
    }

    [Theory]
    [InlineData("from T where x = 1", ComparisonOperators.Equal)]
    [InlineData("from T where x != 1", ComparisonOperators.NotEqual)]
    [InlineData("from T where x < 1", ComparisonOperators.LessThan)]
    [InlineData("from T where x <= 1", ComparisonOperators.LessThanOrEqual)]
    [InlineData("from T where x > 1", ComparisonOperators.GreaterThan)]
    [InlineData("from T where x >= 1", ComparisonOperators.GreaterThanOrEqual)]
    [InlineData("from T where x startswith \"a\"", ComparisonOperators.StartsWith)]
    [InlineData("from T where x endswith \"a\"", ComparisonOperators.EndsWith)]
    [InlineData("from T where x contains \"a\"", ComparisonOperators.Contains)]
    public void Parse_ComparisonOperators(string source, ComparisonOperators expected) =>
        Assert.Equal(expected, ParseComparison(source).Operator);

    [Fact]
    public void Parse_IntegerLiteral()
    {
        var right = Assert.IsType<NumericLiteral>(ParseComparison("from T where x = 42").Right);

        Assert.Equal(NumericTypes.Integer, right.Type);
        Assert.Equal(42, right.ToInt32());
    }

    [Fact]
    public void Parse_NegativeIntegerLiteral()
    {
        var right = Assert.IsType<NumericLiteral>(ParseComparison("from T where x = -7").Right);

        Assert.Equal(-7, right.ToInt32());
    }

    [Fact]
    public void Parse_FloatingPointLiteral()
    {
        var right = Assert.IsType<NumericLiteral>(ParseComparison("from T where x = 1.5").Right);

        Assert.Equal(NumericTypes.FloatingPoint, right.Type);
        Assert.Equal(1.5, right.ToDouble());
    }

    [Fact]
    public void Parse_ScientificNotationLiteral()
    {
        var right = Assert.IsType<NumericLiteral>(ParseComparison("from T where x = 1.0E5").Right);

        Assert.Equal(NumericTypes.ScientificNotation, right.Type);
        Assert.Equal(100000, right.ToDouble());
    }

    [Fact]
    public void Parse_StringLiteral()
    {
        var right = Assert.IsType<StringLiteral>(ParseComparison("from T where x = \"hello\"").Right);

        Assert.Equal("hello", right.Value);
    }

    [Fact]
    public void Parse_CharacterLiteral()
    {
        var right = Assert.IsType<CharacterLiteral>(ParseComparison("from T where x = 'a'").Right);

        Assert.Equal("a", right.Value);
    }

    [Fact]
    public void Parse_NullLiteral() =>
        Assert.IsType<NullLiteral>(ParseComparison("from T where x = null").Right);

    [Theory]
    [InlineData("from T where x = true", true)]
    [InlineData("from T where x = false", false)]
    public void Parse_BooleanLiteral(string source, bool expected)
    {
        var right = Assert.IsType<BooleanLiteral>(ParseComparison(source).Right);

        Assert.Equal(expected, right.ToBoolean());
    }

    [Fact]
    public void Parse_LogicalOr_BuildsOrExpression()
    {
        var statement = parser.Parse("from T where a = 1 or b = 2");

        var logical = Assert.IsType<LogicalExpression>(statement.Predicate);
        Assert.Equal(LogicalOperators.Or, logical.Operator);
        _ = Assert.IsType<ComparisonExpression>(logical.Left);
        _ = Assert.IsType<ComparisonExpression>(logical.Right);
    }

    [Fact]
    public void Parse_LogicalAnd_BuildsAndExpression()
    {
        var statement = parser.Parse("from T where a = 1 and b = 2");

        var logical = Assert.IsType<LogicalExpression>(statement.Predicate);
        Assert.Equal(LogicalOperators.And, logical.Operator);
    }

    [Fact]
    public void Parse_AndBindsTighterThanOr()
    {
        var statement = parser.Parse("from T where a = 1 or b = 2 and c = 3");

        var or = Assert.IsType<LogicalExpression>(statement.Predicate);
        Assert.Equal(LogicalOperators.Or, or.Operator);
        _ = Assert.IsType<ComparisonExpression>(or.Left);
        var and = Assert.IsType<LogicalExpression>(or.Right);
        Assert.Equal(LogicalOperators.And, and.Operator);
    }

    [Fact]
    public void Parse_ParentheticalExpression()
    {
        var statement = parser.Parse("from T where (a = 1)");

        var parenthetical = Assert.IsType<ParentheticalExpression>(statement.Predicate);
        _ = Assert.IsType<ComparisonExpression>(parenthetical.Expression);
    }

    [Fact]
    public void Parse_NoSkipOrTake_LeavesBothNull()
    {
        var statement = parser.Parse("from T where x = 1");

        Assert.Null(statement.Skip);
        Assert.Null(statement.Take);
    }

    [Fact]
    public void Parse_SkipOnly_SetsSkipLeavesTakeNull()
    {
        var statement = parser.Parse("from T where x = 1 skip 5");

        Assert.Equal(5, statement.Skip?.ToInt32());
        Assert.Null(statement.Take);
    }

    [Fact]
    public void Parse_TakeOnly_SetsTakeLeavesSkipNull()
    {
        var statement = parser.Parse("from T where x = 1 take 9");

        Assert.Null(statement.Skip);
        Assert.Equal(9, statement.Take?.ToInt32());
    }

    [Fact]
    public void Parse_SkipAndTake_SetsBoth()
    {
        var statement = parser.Parse("from T where x = 1 skip 5 take 9");

        Assert.Equal(5, statement.Skip?.ToInt32());
        Assert.Equal(9, statement.Take?.ToInt32());
    }

    [Fact]
    public void Parse_SkipAndTake_AcceptNonIntegerNumerics()
    {
        var statement = parser.Parse("from T where x = 1 skip 1.5 take 2.0E1");

        Assert.Equal(NumericTypes.FloatingPoint, statement.Skip?.Type);
        Assert.Equal(NumericTypes.ScientificNotation, statement.Take?.Type);
    }

    [Fact]
    public void Parse_CapturesFromIdentifier() =>
        Assert.Equal("Address", parser.Parse("from Address where x = 1").From.Value);
}

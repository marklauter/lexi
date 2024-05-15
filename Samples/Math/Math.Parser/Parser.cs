using Lexi;
using Math.Parser.Exceptions;
using Math.Parser.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Math.Parser;

public sealed class Parser(Lexer lexer)
{
    private readonly Lexer lexer = lexer
        ?? throw new ArgumentNullException(nameof(lexer));

    private readonly ref struct ParseResult(
        Expression expression,
        MatchResult matchResult)
    {
        public readonly Expression Expression = expression;
        public readonly MatchResult MatchResult = matchResult;
    }

    public Expression Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseTerm(new Source(source))
            .Expression;
    }

    private ParseResult ParseTerm(Source script)
    {
        var left = ParseFactor(script);

        var matchResult = left.MatchResult;
        matchResult = lexer.NextMatch(matchResult.Source);

        while (!matchResult.Source.IsEndOfSource
            && matchResult.Symbol.IsOperator()
            && matchResult.Symbol.TokenId.IsTerm())
        {
            var right = ParseFactor(matchResult.Source);

            left = new(
                new BinaryOperation(
                left.Expression,
                right.Expression,
                matchResult.Symbol.TokenId),
                right.MatchResult);

            matchResult = lexer.NextMatch(right.MatchResult.Source);
        }

        return left;
    }

    private ParseResult ParseFactor(Source script)
    {
        var left = ParseValue(script);

        var matchResult = left.MatchResult;
        matchResult = lexer.NextMatch(matchResult.Source);

        while (!matchResult.Source.IsEndOfSource
            && matchResult.Symbol.IsOperator()
            && matchResult.Symbol.TokenId.IsFactor())
        {
            var right = ParseValue(matchResult.Source);

            left = new(
                new BinaryOperation(
                left.Expression,
                right.Expression,
                matchResult.Symbol.TokenId),
                right.MatchResult);

            matchResult = lexer.NextMatch(right.MatchResult.Source);
        }

        return left;
    }

    private ParseResult ParseValue(Source script)
    {
        if (script.IsEndOfSource)
        {
            throw new UnexpectedEndOfSourceException("Unexpected end of source");
        }

        string value;
        var matchResult = lexer.NextMatch(script);

        if (matchResult.Symbol.IsNumericLiteral())
        {
            return new(ParseNumber(in matchResult), matchResult);
        }
        else if (matchResult.Symbol.IsOpenCircumfixDelimiter())
        {
            var term = ParseTerm(matchResult.Source);

            matchResult = lexer.NextMatch(term.MatchResult.Source);
            if (matchResult.Symbol.IsCloseCircumfixDelimiter())
            {
                return new(new Group(term.Expression), matchResult);
            }

            value = matchResult.Symbol.Token == Tokens.Undefined
                ? script.Text[script.Offset..]
                : matchResult
                    .Source
                    .ReadSymbol(in matchResult.Symbol);
            throw new UnexpectedTokenException($"unexpected token '{value}' at {matchResult.Symbol.Offset}. expected close parenthesis.");
        }

        value = matchResult.Symbol.Token == Tokens.Undefined
            ? script.Text[script.Offset..]
            : matchResult
                .Source
                .ReadSymbol(in matchResult.Symbol);
        throw new UnexpectedTokenException($"unexpected token '{value}' at {matchResult.Symbol.Offset}. expected number or open parenthesis.");
    }

    [SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "switch is complete")]
    private static Number ParseNumber(ref readonly MatchResult matchResult)
    {
        var value = matchResult
            .Source
            .ReadSymbol(in matchResult.Symbol);

        // todo: use TryParse and add error msg on false
        return matchResult.Symbol.Token switch
        {
            Tokens.IntegerLiteral => new Number(
                NumericTypes.Integer,
                Int32.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)),
            Tokens.FloatingPointLiteral => new Number(
                NumericTypes.FloatingPoint,
                Double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture)),
            Tokens.ScientificNotationLiteral => new Number(
                NumericTypes.ScientificNotation,
                Double.Parse(value, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)),
            _ => new Number(NumericTypes.NotANumber, 0)
        };
    }
}

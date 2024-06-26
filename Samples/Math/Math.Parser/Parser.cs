﻿using Lexi;
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
        matchResult = lexer.NextMatch(matchResult);

        while (!matchResult.Source.IsEndOfSource
            && matchResult.Symbol.IsOperator()
            && matchResult.Symbol.IsTerm())
        {
            var right = ParseFactor(matchResult.Source);

            left = new(
                new BinaryOperation(
                left.Expression,
                right.Expression,
                matchResult.Symbol.TokenId),
                right.MatchResult);

            matchResult = lexer.NextMatch(right.MatchResult);
        }

        return left;
    }

    private ParseResult ParseFactor(Source script)
    {
        var left = ParseValue(script);

        var matchResult = left.MatchResult;
        matchResult = lexer.NextMatch(matchResult);

        while (!matchResult.Source.IsEndOfSource
            && matchResult.Symbol.IsOperator()
            && matchResult.Symbol.IsFactor())
        {
            var right = ParseValue(matchResult.Source);

            left = new(
                new BinaryOperation(
                left.Expression,
                right.Expression,
                matchResult.Symbol.TokenId),
                right.MatchResult);

            matchResult = lexer.NextMatch(right.MatchResult);
        }

        return left;
    }

    private ParseResult ParseValue(Source source)
    {
        if (source.IsEndOfSource)
        {
            throw new UnexpectedEndOfSourceException("Unexpected end of source");
        }

        var matchResult = lexer.NextMatch(source);

        if (matchResult.Symbol.IsNumericLiteral())
        {
            return new(ParseNumber(in matchResult), matchResult);
        }
        else if (matchResult.Symbol.IsOpenCircumfixDelimiter())
        {
            var term = ParseTerm(matchResult.Source);

            matchResult = lexer.NextMatch(term.MatchResult);
            if (matchResult.Symbol.IsCloseCircumfixDelimiter())
            {
                return new(new Group(term.Expression), matchResult);
            }

            if (matchResult.Symbol.IsMatch)
            {
                throw new UnexpectedTokenException($"unexpected token '{matchResult.Source.ReadSymbol(in matchResult.Symbol)}' at {matchResult.Symbol.Offset}. expected close parenthesis.");
            }

            throw new UnexpectedEndOfSourceException($"unexpected token '{source.Remaining()}' at {matchResult.Symbol.Offset}. expected close parenthesis.");
        }

        if (matchResult.Symbol.IsMatch)
        {
            throw new UnexpectedTokenException($"unexpected token '{matchResult.Source.ReadSymbol(in matchResult.Symbol)}' at {matchResult.Symbol.Offset}. expected number or open parenthesis.");
        }

        throw new UnexpectedTokenException($"unexpected token '{source.Remaining()}' at {matchResult.Symbol.Offset}. expected number or open parenthesis.");
    }

    [SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "switch is complete")]
    private static Number ParseNumber(ref readonly MatchResult matchResult)
    {
        var value = matchResult
            .Source
            .ReadSymbol(in matchResult.Symbol);

        // todo: use TryParse and add error msg on false
        return matchResult.Symbol.TokenId switch
        {
            TokenIds.INTEGER_LITERAL => new Number(
                NumericTypes.Integer,
                Int32.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)),
            TokenIds.FLOATING_POINT_LITERAL => new Number(
                NumericTypes.FloatingPoint,
                Double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture)),
            TokenIds.SCIENTIFIC_NOTATION_LITERAL => new Number(
                NumericTypes.ScientificNotation,
                Double.Parse(value, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)),
            _ => new Number(NumericTypes.NotANumber, 0)
        };
    }
}

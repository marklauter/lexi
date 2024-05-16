## Build Status
[![.NET Tests](https://github.com/marklauter/lexi/actions/workflows/dotnet.tests.yml/badge.svg)](https://github.com/marklauter/lexi/actions/workflows/dotnet.tests.yml)
[![.NET Publish](https://github.com/marklauter/lexi/actions/workflows/dotnet.publish.yml/badge.svg)](https://github.com/marklauter/lexi/actions/workflows/dotnet.publish.yml)

##
![lexi logo](https://raw.githubusercontent.com/marklauter/lexi/main/images/lexi.png)

# lexi
A regex based lexer for dotnet. The lexer supports simple L1 recursive descent parsers.

## Nuget Package
https://www.nuget.org/packages/MSL.Lexi/
```console
dotnet add package MSL.Lexi
```

## Sample Projects
There are two sample projects that demonstrate how to use the lexer within a recursive descent parser. One is a simple math parser and the other is a predicate expression parser.
Each project includes a parser library, a set of tests for the parser, and a REPL console application that allows you to interact with the parser.

See [Math.Parser](https://github.com/marklauter/lexi/tree/main/Samples/Math) and [Predicate.Parser](https://github.com/marklauter/lexi/tree/main/Samples/Predicate) for working samples.

### Sample Math.REPL Output
```console
math:> (1 + 1) / 2 * 3
BinaryOperation
Left Expression
   BinaryOperation
   Left Expression
      Group Expression
         BinaryOperation
         Left Expression
            Number: 1
         Op Add
         Right Expression
            Number: 1
   Op Divide
   Right Expression
      Number: 2
Op Multiply
Right Expression
   Number: 3
-------------
result:> 3

math:>
```

### Sample Predicate.REPL Output
```yaml
predicate:> from Address where Street startswith "Cypress" and (City = "Tampa" or City = "Miami")
From: Address
LogicalExpression:
|-- L: ComparisonExpression:
|-- L: |-- L: Identifier: Street
|-- L: |-- Operator: StartsWith
|-- L: |-- R: StringLiteral: Cypress
|-- Operator: And
|-- R: ParentheticalExpression:
|-- R: |-- (: LogicalExpression:
|-- R: |-- (: |-- L: ComparisonExpression:
|-- R: |-- (: |-- L: |-- L: Identifier: City
|-- R: |-- (: |-- L: |-- Operator: Equal
|-- R: |-- (: |-- L: |-- R: StringLiteral: Tampa
|-- R: |-- (: |-- Operator: Or
|-- R: |-- (: |-- R: ComparisonExpression:
|-- R: |-- (: |-- R: |-- L: Identifier: City
|-- R: |-- (: |-- R: |-- Operator: Equal
|-- R: |-- (: |-- R: |-- R: StringLiteral: Miami
predicate:>
```

## VocabularyBuilder Examples
 You specify the vocabulary with the `VocabularyBuilder` which returns a lexer from the `Build` method.
 
 Here's a sample from the `Math.Parser` project:
```csharp
public static IServiceCollection AddParser(this IServiceCollection services)
{
    var builder = VocabularyBuilder
        .Create(RegexOptions.CultureInvariant)
        .Match("false", TokenIds.FALSE)
        .Match("true", TokenIds.TRUE)
        .Match(CommonPatterns.IntegerLiteral(), TokenIds.INTEGER_LITERAL)
        .Match(CommonPatterns.FloatingPointLiteral(), TokenIds.FLOATING_POINT_LITERAL)
        .Match(CommonPatterns.ScientificNotationLiteral(), TokenIds.SCIENTIFIC_NOTATION_LITERAL)
        .Match(@"\+", TokenIds.ADD)
        .Match("-", TokenIds.SUBTRACT)
        .Match(@"\*", TokenIds.MULTIPLY)
        .Match("/", TokenIds.DIVIDE)
        .Match("%", TokenIds.MODULUS)
        .Match(@"\(", TokenIds.OPEN_PARENTHESIS)
        .Match(@"\)", TokenIds.CLOSE_PARENTHESIS);

    // register the lexer with the service collection
    services.TryAddSingleton(serviceProvider => builder.Build());

    // lexer is injected into Parser constructor:
    // public sealed class Parser(Lexer lexer)
    services.TryAddTransient<Parser>();

    return services;
}
```

The `Predicate.Parser` project works the same way:
```csharp
public static IServiceCollection AddParser(this IServiceCollection services)
{
    var builder = VocabularyBuilder
        .Create(RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
        .Match($"{nameof(TokenIds.FROM)}", TokenIds.FROM)
        .Match($"{nameof(TokenIds.WHERE)}", TokenIds.WHERE)
        .Match($"{nameof(TokenIds.SKIP)}", TokenIds.SKIP)
        .Match($"{nameof(TokenIds.TAKE)}", TokenIds.TAKE)
        .Match($"{nameof(TokenIds.CONTAINS)}", TokenIds.CONTAINS)
        .Match("startswith|sw", TokenIds.STARTS_WITH)
        .Match("endswith|ew", TokenIds.ENDS_WITH)
        .Match(@"and|&&", TokenIds.LOGICAL_AND)
        .Match(@"or|\|\|", TokenIds.LOGICAL_OR)
        .Match("null|NULL", TokenIds.NULL_LITERAL)
        .Match(CommonPatterns.Identifier(), TokenIds.IDENTIFIER)
        .Match("true", TokenIds.TRUE)
        .Match("false", TokenIds.FALSE)
        .Match(CommonPatterns.IntegerLiteral(), TokenIds.INTEGER_LITERAL)
        .Match(CommonPatterns.FloatingPointLiteral(), TokenIds.FLOATING_POINT_LITERAL)
        .Match(CommonPatterns.ScientificNotationLiteral(), TokenIds.SCIENTIFIC_NOTATION_LITERAL)
        .Match(CommonPatterns.QuotedStringLiteral(), TokenIds.STRING_LITERAL)
        .Match(CommonPatterns.CharacterLiteral(), TokenIds.CHAR_LITERAL)
        .Match(@"\(", TokenIds.OPEN_PARENTHESIS)
        .Match(@"\)", TokenIds.CLOSE_PARENTHESIS)
        .Match("=|==", TokenIds.EQUAL)
        .Match(">", TokenIds.GREATER_THAN)
        .Match(">=", TokenIds.GREATER_THAN_OR_EQUAL)
        .Match("<", TokenIds.LESS_THAN)
        .Match("<=", TokenIds.LESS_THAN_OR_EQUAL)
        .Match("!=", TokenIds.NOT_EQUAL);

    // register the lexer with the service collection
    services.TryAddSingleton(serviceProvider => builder.Build());

    // lexer is injected into Parser constructor:
    // public sealed class Parser(Lexer lexer)
    services.TryAddTransient<Parser>();

    return services;
}
```

## Practical Parser Example
The `Math.Parser` implements a classic term/factor recusive descent parser. The parser returns an expression tree that can be evaluated to get the result.
We use the lexer to get the next token with calls to one of the `Lexer.NextMatch` overloads as required. 

```csharp
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

    // Parse the source string into an expression tree.
    public Expression Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseTerm(new Source(source))
            .Expression;
    }

    private readonly ref struct ParseResult(
        Expression expression,
        MatchResult matchResult)
    {
        public readonly Expression Expression = expression;
        public readonly MatchResult MatchResult = matchResult;
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
```

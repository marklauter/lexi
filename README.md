[![.NET Tests](https://github.com/marklauter/lexi/actions/workflows/dotnet.tests.yml/badge.svg)](https://github.com/marklauter/lexi/actions/workflows/dotnet.tests.yml)
[![.NET Publish](https://github.com/marklauter/lexi/actions/workflows/dotnet.publish.yml/badge.svg)](https://github.com/marklauter/lexi/actions/workflows/dotnet.publish.yml)
[![NuGet](https://img.shields.io/nuget/v/MSL.Lexi?logo=nuget)](https://www.nuget.org/packages/MSL.Lexi/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/)

# lexi
A regex-based lexer for dotnet. The lexer supports simple L1 recursive descent parsers.

## Nuget Package
https://www.nuget.org/packages/MSL.Lexi/
```console
dotnet add package MSL.Lexi
```

## Upgrading from v2 to v3
v3.0.0 is a breaking release. The headline change is the **span-first redesign**: a `Symbol` is now a plain, collectible value — the matched text lives in `Source`, not in the token — so you can stream tokens into `List<Symbol>`, `IEnumerable<Symbol>`, fields, and across `await`, which the old `ref struct` `Symbol` made impossible.

If you only do two things: **retarget to net10.0** and **replace implicit `Source`⇄`string` conversions**. Those cover most call sites.

**1. Target framework: net6/7/8 → net10.0.** Every consumer must retarget; this is what forces the major bump.
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. `Symbol` is now a `readonly record struct` (was `readonly ref struct`).** Tokens are now collectible (`List<Symbol>`, fields, across `await`) and gain value equality. `Offset`/`Length`/`TokenId` are now properties rather than public fields — source-compatible for reads; recompile for the binary change.

**3. Token text comes from `Source`, never from `Symbol`.** The span lives only in `Source`; read a token's text from the `Source` that produced it. `MatchResult` is still a `ref struct` (it carries a `Source`), so keep the `Symbol`s plus one `Source` to resolve their text on demand.
```csharp
ReadOnlySpan<char> text = result.Source.ReadSymbol(in result.Symbol);
```

**4. `Source` implicit operators removed.**
```csharp
// v2 — implicit conversions
Source src = "1 + 2";       // string -> Source
string s   = src;           // Source -> string
// v3 — explicit
Source src = new("1 + 2");  // or Source.FromString("1 + 2")
string s   = src.ToString();
```
`lexer.NextMatch("…")` still works — v3 removes the implicit operator but adds an explicit `NextMatch(string)` overload. Only sites that relied on the conversion elsewhere need the explicit form.

**5. `Source.ReadSymbol` returns `ReadOnlySpan<char>` (was `string`).** No substring is allocated now; call `.ToString()` if you need a `string`.

**6. `Source.ToString()` returns the source text** (was the default `"Lexi.Source"`).

**7. Sealing / static.** `CommonPatterns` is now `static` (`new CommonPatterns()` and subclassing no longer compile; calling the pattern members is unchanged). `VocabularyBuilder` is now `sealed`.

**8. Behavioral & grammar changes (same API, different output).**
- Trailing ignorable content now returns `EndOfSource`, not a spurious `NoMatch` error token.
- Interleaved runs of different ignore patterns are now fully skipped (v2 made a single pass and could strand the offset).
- A `NoMatch` symbol now spans the single offending character (`Length 1`, was `0`); `ReadSymbol` names it — `"lexer error at offset 14: unexpected '@'"`. The offset still does not advance; recovery is the caller's choice.
- `ScientificNotationLiteral` now accepts a signed positive exponent (`1e+5`, `-1.5E+10`).
- `CharacterLiteral` now supports escape sequences (`\b \t \n \r \f \' \" \\` and `\uXXXX`) and therefore **requires a backslash to be escaped** — v2's `'\'` (a lone backslash char) is now invalid; write `'\\'`.

**9. Packaging.** v3 ships a `.snupkg` symbols package alongside the main package for the first time. Additive — no action required.

### Quick reference
| v2 | v3 |
| --- | --- |
| `net6.0;net7.0;net8.0` | `net10.0` |
| `Symbol` is a `ref struct` (not collectible) | `readonly record struct` (collectible, value equality) |
| `Source src = "text";` | `new Source("text")` / `Source.FromString("text")` |
| `string s = source;` | `source.ToString()` |
| `string t = source.ReadSymbol(in sym);` | `string t = source.ReadSymbol(in sym).ToString();` |
| `Source.ToString()` → `"Lexi.Source"` | → the source text |
| `new CommonPatterns()` / subclass | not allowed (`static`) — call members directly |
| subclass `VocabularyBuilder` | not allowed (`sealed`) |
| `'\'` matches a backslash char | invalid — use `'\\'` |

## Sample Projects
I've included two sample projects in the repo to demonstrate the lexer within a recursive descent parser. One is a simple math parser and the other is a predicate expression parser.
Each project includes a parser library, a set of tests for the parser, and a REPL console application that allows you to interact with the parser.

See [Math.Parsing](https://github.com/marklauter/lexi/tree/main/samples/Math) and [Predicate.Parsing](https://github.com/marklauter/lexi/tree/main/samples/Predicate) for working samples.

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
 
 Here's a sample from the `Math.Parsing` project:
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
        .Match(@"\)", TokenIds.CLOSE_PARENTHESIS)
        .Ignore(CommonPatterns.Whitespace(), TokenIds.WHITE_SPACE)
        .Ignore(CommonPatterns.NewLine(), TokenIds.WHITE_SPACE);

    // register the lexer with the service collection
    services.TryAddSingleton(serviceProvider => builder.Build());

    // lexer is injected into Parser constructor:
    // public sealed class Parser(Lexer lexer)
    services.TryAddTransient<Parser>();

    return services;
}
```

The `Predicate.Parsing` project works the same way:
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
        .Match("!=", TokenIds.NOT_EQUAL)
        .Ignore(CommonPatterns.Whitespace(), TokenIds.WHITE_SPACE)
        .Ignore(CommonPatterns.NewLine(), TokenIds.WHITE_SPACE);

    // register the lexer with the service collection
    services.TryAddSingleton(serviceProvider => builder.Build());

    // lexer is injected into Parser constructor:
    // public sealed class Parser(Lexer lexer)
    services.TryAddTransient<Parser>();

    return services;
}
```

## Practical Parser Example
The `Math.Parsing` implements a classic term/factor recursive descent parser. The parser returns an expression tree that can be evaluated to get the result.
We use the lexer to get the next token with calls to one of the `Lexer.NextMatch` overloads as required. 

```csharp
using Lexi;
using Math.Parsing.Exceptions;
using Math.Parsing.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Math.Parsing;

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
                int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)),
            TokenIds.FLOATING_POINT_LITERAL => new Number(
                NumericTypes.FloatingPoint,
                double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture)),
            TokenIds.SCIENTIFIC_NOTATION_LITERAL => new Number(
                NumericTypes.ScientificNotation,
                double.Parse(value, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)),
            _ => new Number(NumericTypes.NotANumber, 0)
        };
    }
}
```

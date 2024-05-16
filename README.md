![lexi logo](https://raw.githubusercontent.com/marklauter/lexi/main/images/lexi.png)
# lexi
A regex based lexer for dotnet.

## Samples
See [Math.Parser](https://github.com/marklauter/lexi/tree/main/Samples/Math) and [Predicate.Parser](https://github.com/marklauter/lexi/tree/main/Samples/Predicate) for working samples.

## VocabularyBuilder Sample
```csharp
    var lexer = VocabularyBuilder
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
        .Build();
```
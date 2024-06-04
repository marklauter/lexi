using System.Text.RegularExpressions;

namespace Lexi;

/// <summary>
/// Common <see cref="Regex"/> patterns.
/// </summary>
public partial class CommonPatterns
{
    private const RegexOptions PatternOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline |
        RegexOptions.CultureInvariant;

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"\G(\r\n|[\r\n])", PatternOptions)]
    public static partial Regex NewLine();

    [GeneratedRegex(@"\G\s+", PatternOptions)]
    public static partial Regex Whitespace();

    [GeneratedRegex(@"\G\-?\d+", PatternOptions)]
    public static partial Regex IntegerLiteral();

    [GeneratedRegex(@"\G\-?\d+\.\d+", PatternOptions)]
    public static partial Regex FloatingPointLiteral();

    [GeneratedRegex(@"\G\-?\d+(?:\.\d+)?[eE]\-?\d+", PatternOptions)]
    public static partial Regex ScientificNotationLiteral();

    [GeneratedRegex(@"\G""(?:[^""\\\n\r]|\\.)*""", PatternOptions)]
    public static partial Regex QuotedStringLiteral();

    // todo: need to add char literal pattern for escape codes like \b, \t, \n, \r, \f, \', \", \\, \u0000, \uFFFF
    [GeneratedRegex(@"\G'[^']'", PatternOptions)]
    public static partial Regex CharacterLiteral();

    [GeneratedRegex(@"\G[a-zA-Z_]\w*", PatternOptions)]
    public static partial Regex Identifier();
#elif NET6_0_OR_GREATER
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static Regex NewLine() => new(@"\G(\r\n|[\r\n])", PatternOptions);

    public static Regex Whitespace() => new(@"\G\s+", PatternOptions);

    public static Regex IntegerLiteral() => new(@"\G\-?\d+", PatternOptions);

    public static Regex FloatingPointLiteral() => new(@"\G\-?\d+\.\d+", PatternOptions);

    public static Regex ScientificNotationLiteral() => new(@"\G\-?\d+(?:\.\d+)?[eE]\-?\d+", PatternOptions);

    public static Regex QuotedStringLiteral() => new(@"\G""(?:[^""\\\n\r]|\\.)*""", PatternOptions);

    public static Regex CharacterLiteral() => new(@"\G'[^']'", PatternOptions);

    public static Regex Identifier() => new(@"\G[a-zA-Z_]\w*", PatternOptions);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif
}

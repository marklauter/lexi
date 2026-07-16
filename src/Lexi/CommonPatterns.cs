using System.Text.RegularExpressions;

namespace Lexi;

/// <summary>
/// Common <see cref="Regex"/> patterns.
/// </summary>
public static partial class CommonPatterns
{
    private const RegexOptions PatternOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline |
        RegexOptions.CultureInvariant;

    /// <summary>
    /// Matches a single line break: a CR-LF pair, a lone carriage return, or a lone line feed.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G(\r\n|[\r\n])", PatternOptions)]
    public static partial Regex NewLine();

    /// <summary>
    /// Matches a run of one or more whitespace characters.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G\s+", PatternOptions)]
    public static partial Regex Whitespace();

    /// <summary>
    /// Matches an integer literal: an optional leading minus sign followed by one or more decimal digits.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G\-?\d+", PatternOptions)]
    public static partial Regex IntegerLiteral();

    /// <summary>
    /// Matches a floating-point literal: an optional minus sign, one or more digits, a decimal point, and one or more fractional digits.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G\-?\d+\.\d+", PatternOptions)]
    public static partial Regex FloatingPointLiteral();

    /// <summary>
    /// Matches a number in scientific notation: an optional minus sign, an integer part, an optional fractional part, an <c>e</c> or <c>E</c>, an optional exponent sign (<c>+</c> or <c>-</c>), and one or more exponent digits.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G\-?\d+(?:\.\d+)?[eE][+\-]?\d+", PatternOptions)]
    public static partial Regex ScientificNotationLiteral();

    /// <summary>
    /// Matches a double-quoted string literal. The body is any run of characters other than a quote, backslash, or line break, interspersed with backslash escapes (a backslash followed by any character).
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G""(?:[^""\\\n\r]|\\.)*""", PatternOptions)]
    public static partial Regex QuotedStringLiteral();

    /// <summary>
    /// Matches a character literal: a single character enclosed in single quotes. The character is one ordinary character (not a quote or backslash), a simple escape (<c>\b</c>, <c>\t</c>, <c>\n</c>, <c>\r</c>, <c>\f</c>, <c>\'</c>, <c>\"</c>, or <c>\\</c>), or a four-hex-digit Unicode escape (<c>\uXXXX</c>). A lone backslash is rejected.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    // A single character between single quotes: either one ordinary char (not a quote or backslash),
    // a simple escape (backslash + one of b t n r f ' " \), or a 4-hex-digit unicode escape (\uXXXX).
    [GeneratedRegex(@"\G'(?:[^'\\]|\\[btnrf'""\\]|\\u[0-9A-Fa-f]{4})'", PatternOptions)]
    public static partial Regex CharacterLiteral();

    /// <summary>
    /// Matches an identifier: a leading ASCII letter or underscore followed by any number of word characters.
    /// </summary>
    /// <returns>The <see cref="Regex"/>.</returns>
    [GeneratedRegex(@"\G[a-zA-Z_]\w*", PatternOptions)]
    public static partial Regex Identifier();
}

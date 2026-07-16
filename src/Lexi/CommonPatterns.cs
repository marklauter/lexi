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

    [GeneratedRegex(@"\G(\r\n|[\r\n])", PatternOptions)]
    public static partial Regex NewLine();

    [GeneratedRegex(@"\G\s+", PatternOptions)]
    public static partial Regex Whitespace();

    [GeneratedRegex(@"\G\-?\d+", PatternOptions)]
    public static partial Regex IntegerLiteral();

    [GeneratedRegex(@"\G\-?\d+\.\d+", PatternOptions)]
    public static partial Regex FloatingPointLiteral();

    [GeneratedRegex(@"\G\-?\d+(?:\.\d+)?[eE][+\-]?\d+", PatternOptions)]
    public static partial Regex ScientificNotationLiteral();

    [GeneratedRegex(@"\G""(?:[^""\\\n\r]|\\.)*""", PatternOptions)]
    public static partial Regex QuotedStringLiteral();

    // A single character between single quotes: either one ordinary char (not a quote or backslash),
    // a simple escape (backslash + one of b t n r f ' " \), or a 4-hex-digit unicode escape (\uXXXX).
    [GeneratedRegex(@"\G'(?:[^'\\]|\\[btnrf'""\\]|\\u[0-9A-Fa-f]{4})'", PatternOptions)]
    public static partial Regex CharacterLiteral();

    [GeneratedRegex(@"\G[a-zA-Z_]\w*", PatternOptions)]
    public static partial Regex Identifier();
}

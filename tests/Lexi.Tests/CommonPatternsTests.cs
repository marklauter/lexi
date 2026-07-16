using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class CommonPatternsTests
{
    private static void AssertConsumesWhole(Regex regex, string input)
    {
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(input);

        var match = regex.Match(input);

        Assert.True(match.Success);
        Assert.Equal(0, match.Index);
        Assert.Equal(input.Length, match.Length);
    }

    private static void AssertNoMatch(Regex regex, string input)
    {
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(input);

        Assert.DoesNotMatch(regex, input);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    public void NewLine_MatchesEachLineBreakForm(string input) =>
        AssertConsumesWhole(CommonPatterns.NewLine(), input);

    [Theory]
    [InlineData("1e5")]
    [InlineData("1E5")]
    [InlineData("1e-5")]
    [InlineData("1e+5")] // signed positive exponent
    [InlineData("1.5e3")]
    [InlineData("-1e5")]
    [InlineData("-1.5E+10")]
    public void ScientificNotation_MatchesSignedExponents(string input) =>
        AssertConsumesWhole(CommonPatterns.ScientificNotationLiteral(), input);

    [Theory]
    [InlineData("'a'")]
    [InlineData("'Z'")]
    [InlineData("'0'")]
    public void CharacterLiteral_MatchesASingleUnescapedCharacter(string input) =>
        AssertConsumesWhole(CommonPatterns.CharacterLiteral(), input);

    [Theory]
    [InlineData("''")]   // empty
    [InlineData("'ab'")] // two characters
    public void CharacterLiteral_RejectsNonSingleCharacters(string input) =>
        AssertNoMatch(CommonPatterns.CharacterLiteral(), input);

    [Theory]
    [InlineData(@"'\b'")]
    [InlineData(@"'\t'")]
    [InlineData(@"'\n'")]
    [InlineData(@"'\r'")]
    [InlineData(@"'\f'")]
    [InlineData(@"'\''")] // escaped single quote
    [InlineData("'\\\"'")] // escaped double quote
    [InlineData(@"'\\'")] // escaped backslash
    [InlineData(@"'A'")]
    [InlineData(@"'\u0041'")] // unicode escape
    [InlineData(@"'\uabcd'")] // lower-case hex unicode escape
    public void CharacterLiteral_MatchesEscapeSequences(string input) =>
        AssertConsumesWhole(CommonPatterns.CharacterLiteral(), input);

    [Theory]
    [InlineData(@"'\'")]      // lone backslash is not a complete escape
    [InlineData(@"'\x'")]     // unknown simple escape
    [InlineData(@"'\u041'")]  // unicode escape needs four hex digits
    [InlineData(@"'\u00G1'")] // non-hex digit
    public void CharacterLiteral_RejectsMalformedEscapes(string input) =>
        AssertNoMatch(CommonPatterns.CharacterLiteral(), input);
}

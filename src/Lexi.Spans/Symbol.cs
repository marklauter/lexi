namespace Lexi.Spans;

/// <summary>
/// A token's position and id — offset, length, token id. A <b>plain</b> value type (a
/// <c>readonly record struct</c>, not a <c>ref struct</c>): it holds no span, so tokens are freely
/// collectible and streamable (<c>List&lt;Symbol&gt;</c>, <c>IEnumerable&lt;Symbol&gt;</c>, across
/// <c>await</c>), and get value equality for free. This decoupling — positions here, span only in
/// <see cref="Source"/> — is what dissolves the ref-struct wall. The lexer only ever emits non-negative
/// offset/length, so no construction-time guard is carried here.
/// </summary>
public readonly record struct Symbol(
    int Offset,
    int Length,
    uint TokenId)
{
    /// <summary>Returns true if this is a real, non-empty, non-end-of-source match.</summary>
    public bool IsMatch => (TokenId & Pattern.NoMatch) == 0 && Length > 0 && !IsEndOfSource;

    /// <summary>Returns true if the token id carries the end-of-source flag.</summary>
    public bool IsEndOfSource => (TokenId & Pattern.EndOfSource) != 0;

    /// <summary>Returns true if the passed token id equals this symbol's token id.</summary>
    public bool Is(uint tokenId) => TokenId == tokenId;
}

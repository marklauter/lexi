using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// A token's position and id: offset, length, and token id. A <c>readonly record struct</c> that holds
/// no span, so tokens are collectible and streamable (<c>List&lt;Symbol&gt;</c>,
/// <c>IEnumerable&lt;Symbol&gt;</c>, across <c>await</c>) and carry value equality. The span lives only in
/// <see cref="Source"/>, which is a <c>ref struct</c> and cannot cross those boundaries.
/// </summary>
[DebuggerDisplay("{Offset}, {Length}, {TokenId}")]
public readonly record struct Symbol
{
    /// <summary>
    /// Initializes a symbol.
    /// </summary>
    /// <param name="offset">The offset of the symbol in the source.</param>
    /// <param name="length">The length of the symbol.</param>
    /// <param name="tokenId">The token id of the symbol.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Symbol(
        int offset,
        int length,
        uint tokenId)
    {
        Offset = offset < 0
            ? throw new ArgumentOutOfRangeException(nameof(offset))
            : offset;
        Length = length < 0
            ? throw new ArgumentOutOfRangeException(nameof(length))
            : length;
        TokenId = tokenId;
    }

    /// <summary>
    /// Gets the offset.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the token id.
    /// </summary>
    public uint TokenId { get; }

    /// <summary>
    /// Returns true when the symbol is a real match: not flagged <see cref="Pattern.NoMatch"/>, positive
    /// length, and not end of source.
    /// </summary>
    public bool IsMatch => (TokenId & Pattern.NoMatch) == 0 && Length > 0 && !IsEndOfSource;

    /// <summary>
    /// Returns true if the token id is EndOfSource.
    /// </summary>
    /// <remarks>
    /// Set by the lexer when the end of the source is reached.
    /// </remarks>
    public bool IsEndOfSource => (TokenId & Pattern.EndOfSource) != 0;

    /// <summary>
    /// Returns true if the passed token id equals the symbol token id.
    /// </summary>
    /// <param name="tokenId">The token id to compare against <see cref="TokenId"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="tokenId"/> equals <see cref="TokenId"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(uint tokenId) => TokenId == tokenId;
}

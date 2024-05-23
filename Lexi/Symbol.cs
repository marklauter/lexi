using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// A symbol retrieved from the source.
/// </summary>
/// <param name="offset">The offset of the symbol in the source.</param>
/// <param name="length">The length of the symbol.</param>
/// <param name="tokenId">The token id of the symbol.</param>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct Symbol(
    int offset,
    int length,
    uint tokenId)
{
    /// <summary>
    /// Gets the offset.
    /// </summary>
    public readonly int Offset = offset < 0
        ? throw new ArgumentOutOfRangeException(nameof(offset))
        : offset;

    /// <summary>
    /// Gets the length.
    /// </summary>
    public readonly int Length = length < 0
        ? throw new ArgumentOutOfRangeException(nameof(length))
        : length;

    /// <summary>
    /// Gets the token id.
    /// </summary>
    public readonly uint TokenId = tokenId;

    /// <summary>
    /// Returns true if length > 0.
    /// </summary>
    public bool IsMatch => Length > 0;

    /// <summary>
    /// Returns true if the token id is LexError.
    /// </summary>
    /// <remarks>
    /// Set by the lexer when it can't find any matchig patterns in the vocabulary.
    /// </remarks>
    public bool IsError => TokenId == Pattern.LexError;

    /// <summary>
    /// Returns true if the token id is EndOfSource.
    /// </summary>
    /// <remarks>
    /// Set by the lexer when the end of the source is reached.
    /// </remarks>
    public bool IsEndOfSource => TokenId == Pattern.EndOfSource;

    /// <summary>
    /// Returns true if the passed token id equals the symbol token id.
    /// </summary>
    /// <param name="tokenId"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(uint tokenId) => TokenId == tokenId;
}

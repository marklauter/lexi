using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// A match result.
/// </summary>
/// <param name="source"><see cref="Source"/></param>
/// <param name="symbol"><see cref="Symbol"/></param>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct MatchResult(
    Source source,
    Symbol symbol)
{
    /// <summary>
    /// The source with a new offset.
    /// </summary>
    public readonly Source Source = source;

    /// <summary>
    /// The symbol.
    /// </summary>
    public readonly Symbol Symbol = symbol;
}

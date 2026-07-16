using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// The result of one lex step: the <see cref="Symbol"/> read and the <see cref="Source"/> positioned to
/// continue lexing. A <c>readonly ref struct</c> because it carries a <see cref="Source"/>, which holds a span.
/// </summary>
/// <param name="source">The <see cref="Source"/> positioned to continue lexing after this step.</param>
/// <param name="symbol">The <see cref="Symbol"/> produced by this step.</param>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[DebuggerDisplay("{Source}, {Symbol}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct MatchResult(
    Source source,
    Symbol symbol)
{
    /// <summary>
    /// The source positioned to continue lexing: past the token on a match, at the offending character on a
    /// lexer error, at the end on end-of-source.
    /// </summary>
    public readonly Source Source = source;

    /// <summary>
    /// The symbol produced by this step.
    /// </summary>
    public readonly Symbol Symbol = symbol;
}

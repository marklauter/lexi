using System.Diagnostics.CodeAnalysis;

namespace Lexi;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
public readonly ref struct MatchResult(
    Source source,
    Symbol symbol)
{
    public readonly Source Source = source;
    public readonly Symbol Symbol = symbol;
}

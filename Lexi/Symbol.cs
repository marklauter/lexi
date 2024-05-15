using System.Diagnostics.CodeAnalysis;

namespace Lexi;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
public readonly ref struct Symbol(
    int offset,
    int length,
    int tokenId)
{
    public readonly int Offset = Math.Clamp(offset, 0, Int32.MaxValue);
    public readonly int Length = Math.Clamp(length, 0, Int32.MaxValue);
    public readonly int TokenId = tokenId;
    public bool Matched => Length > 0;
}

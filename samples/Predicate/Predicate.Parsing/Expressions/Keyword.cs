using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record Keyword(
    Keywords Value)
    : Expression
{
    /// <summary>
    /// The keyword. Rejected at construction unless it names a defined <see cref="Keywords"/> member,
    /// so every <see cref="Keyword"/> that exists holds a valid keyword and <see cref="ToString"/> cannot fail.
    /// </summary>
    public Keywords Value { get; } = Enum.IsDefined(Value)
        ? Value
        : throw new ArgumentOutOfRangeException(nameof(Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Keyword FromUInt32(uint tokenId) => tokenId switch
    {
        TokenIds.FROM => new Keyword(Keywords.From),
        TokenIds.WHERE => new Keyword(Keywords.Where),
        TokenIds.SKIP => new Keyword(Keywords.Skip),
        TokenIds.TAKE => new Keyword(Keywords.Take),
        _ => throw new ArgumentOutOfRangeException(nameof(tokenId)),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keywords ToKeywords() => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Keyword FromKeywords(Keywords value) => new(value);

    /// <summary>
    /// The canonical text form — the keyword as it appears in source.
    /// </summary>
    public override string ToString() => Value switch
    {
        Keywords.From => "from",
        Keywords.Where => "where",
        Keywords.Skip => "skip",
        Keywords.Take => "take",
        _ => Value.ToString(),
    };
}

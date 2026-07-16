using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record ComparisonOperator(
    ComparisonOperators Value)
    : Expression
{
    /// <summary>
    /// The comparison operator. Rejected at construction unless it names a defined
    /// <see cref="ComparisonOperators"/> member, so <see cref="ToString"/> cannot fail.
    /// </summary>
    public ComparisonOperators Value { get; } = Enum.IsDefined(Value)
        ? Value
        : throw new ArgumentOutOfRangeException(nameof(Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComparisonOperator FromUInt32(uint tokenId) => tokenId switch
    {
        TokenIds.EQUAL => new(ComparisonOperators.Equal),
        TokenIds.NOT_EQUAL => new(ComparisonOperators.NotEqual),
        TokenIds.LESS_THAN => new(ComparisonOperators.LessThan),
        TokenIds.GREATER_THAN => new(ComparisonOperators.GreaterThan),
        TokenIds.LESS_THAN_OR_EQUAL => new(ComparisonOperators.LessThanOrEqual),
        TokenIds.GREATER_THAN_OR_EQUAL => new(ComparisonOperators.GreaterThanOrEqual),
        TokenIds.STARTS_WITH => new(ComparisonOperators.StartsWith),
        TokenIds.ENDS_WITH => new(ComparisonOperators.EndsWith),
        TokenIds.CONTAINS => new(ComparisonOperators.Contains),
        _ => throw new ArgumentOutOfRangeException(nameof(tokenId)),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComparisonOperators ToComparisonOperators() => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComparisonOperator FromComparisonOperators(ComparisonOperators value) => new(value);

    /// <summary>
    /// The canonical text form — the operator's name.
    /// </summary>
    public override string ToString() => Value.ToString();
}

namespace Predicate.Parsing.Expressions;

public sealed record ComparisonExpression(
    Expression Left,
    ComparisonOperators Operator,
    Expression Right)
    : BinaryExpression(Left, Right)
{
    /// <summary>
    /// The comparison operator. Rejected at construction unless it names a defined
    /// <see cref="ComparisonOperators"/> member, so ToString cannot fail.
    /// </summary>
    public ComparisonOperators Operator { get; } = Enum.IsDefined(Operator)
        ? Operator
        : throw new ArgumentOutOfRangeException(nameof(Operator));

    public override string ToString() => Operator switch
    {
        ComparisonOperators.Equal => nameof(ComparisonOperators.Equal),
        ComparisonOperators.NotEqual => nameof(ComparisonOperators.NotEqual),
        ComparisonOperators.GreaterThan => nameof(ComparisonOperators.GreaterThan),
        ComparisonOperators.GreaterThanOrEqual => nameof(ComparisonOperators.GreaterThanOrEqual),
        ComparisonOperators.LessThan => nameof(ComparisonOperators.LessThan),
        ComparisonOperators.LessThanOrEqual => nameof(ComparisonOperators.LessThanOrEqual),
        ComparisonOperators.StartsWith => nameof(ComparisonOperators.StartsWith),
        ComparisonOperators.EndsWith => nameof(ComparisonOperators.EndsWith),
        ComparisonOperators.Contains => nameof(ComparisonOperators.Contains),
        _ => Operator.ToString(),
    };
}

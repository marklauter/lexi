namespace Predicate.Parsing.Expressions;

public sealed record LogicalExpression(
    Expression Left,
    LogicalOperators Operator,
    Expression Right)
    : BinaryExpression(Left, Right)
{
    /// <summary>
    /// The logical operator. Rejected at construction unless it names a defined
    /// <see cref="LogicalOperators"/> member, so ToString cannot fail.
    /// </summary>
    public LogicalOperators Operator { get; } = Enum.IsDefined(Operator)
        ? Operator
        : throw new ArgumentOutOfRangeException(nameof(Operator));

    public override string ToString() => Operator switch
    {
        LogicalOperators.And => nameof(LogicalOperators.And),
        LogicalOperators.Or => nameof(LogicalOperators.Or),
        _ => Operator.ToString(),
    };
}

namespace Predicate.Parsing.Expressions;

public abstract record BinaryExpression(
    Expression Left,
    Expression Right)
    : Expression;

namespace Predicate.Parsing.Expressions;

public sealed record ParentheticalExpression(
    Expression Expression)
    : Expression;

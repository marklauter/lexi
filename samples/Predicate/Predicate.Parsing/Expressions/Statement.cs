namespace Predicate.Parsing.Expressions;

public sealed record Statement(
    Identifier From,
    Expression Predicate,
    NumericLiteral? Skip,
    NumericLiteral? Take);

namespace Predicate.Parser.Expressions;

public enum LogicalOperators : uint
{
    Error = 0,
    And = TokenIds.LOGICAL_AND,
    Or = TokenIds.LOGICAL_OR,
}

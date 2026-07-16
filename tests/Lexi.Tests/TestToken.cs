namespace Lexi.Tests;

public enum TestToken : uint
{
    IntegerLiteral = 0,
    FloatingPointLiteral = 1,
    StringLiteral = 3,
    AdditionOperator = 4,
    SubtractionOperator = 5,
    MultiplicationOperator = 6,
    DivisionOperator = 7,
    ModulusOperator = 8,
    GreaterThanOperator = 9,
    GreaterThanOrEqualOperator = 10,
    WhiteSpace = 99,
}

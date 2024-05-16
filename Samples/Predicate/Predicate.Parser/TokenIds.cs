using Lexi;
using System.Runtime.CompilerServices;

namespace Predicate.Parser;

internal static class SymbolExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsComparisonOperator(this Symbol symbol) =>
        symbol.TokenId is TokenIds.EQUAL or TokenIds.NOT_EQUAL or TokenIds.LESS_THAN or TokenIds.GREATER_THAN or TokenIds.LESS_THAN_OR_EQUAL or TokenIds.GREATER_THAN_OR_EQUAL or TokenIds.STARTS_WITH or TokenIds.ENDS_WITH or TokenIds.CONTAINS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyword(this Symbol symbol) =>
        symbol.TokenId is TokenIds.FROM or TokenIds.WHERE or TokenIds.SKIP or TokenIds.TAKE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIdentifier(this Symbol symbol) =>
        symbol.TokenId == TokenIds.IDENTIFIER;
}

internal sealed class TokenIds
{
    // literals
    public const int FALSE = -1;
    public const int TRUE = -2;
    public const int FLOATING_POINT_LITERAL = -3;
    public const int INTEGER_LITERAL = -4;
    public const int SCIENTIFIC_NOTATION_LITERAL = -5;
    public const int STRING_LITERAL = -6;
    public const int CHAR_LITERAL = -7;
    public const int NULL_LITERAL = -8;
    public const int ARRAY_LITERAL = -9;
    public const int OBJECT_LITERAL = -10;

    // delimiters
    public const int OPEN_PARENTHESIS = '('; // 40
    public const int CLOSE_PARENTHESIS = ')'; // 41

    // comparison operators
    public const int EQUAL = '='; // 61
    public const int NOT_EQUAL = 400;
    public const int GREATER_THAN = '>'; // 62
    public const int GREATER_THAN_OR_EQUAL = 402;
    public const int LESS_THAN = '<'; // 60
    public const int LESS_THAN_OR_EQUAL = 401;
    public const int STARTS_WITH = 405;
    public const int ENDS_WITH = 406;
    public const int CONTAINS = 407;

    // logical operators
    public const int LOGICAL_AND = 403;
    public const int LOGICAL_OR = 404;

    // names
    public const int IDENTIFIER = 500;

    // keywords
    public const int FROM = 300;
    public const int WHERE = 301;
    public const int SKIP = 302;
    public const int TAKE = 303;
}

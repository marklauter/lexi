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
    public const uint WHITE_SPACE = 0;
    // literals
    public const uint FALSE = 1001;
    public const uint TRUE = 1002;
    public const uint FLOATING_POINT_LITERAL = 1003;
    public const uint INTEGER_LITERAL = 1004;
    public const uint SCIENTIFIC_NOTATION_LITERAL = 1005;
    public const uint STRING_LITERAL = 1006;
    public const uint CHAR_LITERAL = 1007;
    public const uint NULL_LITERAL = 1008;
    public const uint ARRAY_LITERAL = 1009;
    public const uint OBJECT_LITERAL = 1010;

    // delimiters
    public const uint OPEN_PARENTHESIS = '('; // 40
    public const uint CLOSE_PARENTHESIS = ')'; // 41

    // comparison operators
    public const uint EQUAL = '='; // 61
    public const uint NOT_EQUAL = 400;
    public const uint GREATER_THAN = '>'; // 62
    public const uint GREATER_THAN_OR_EQUAL = 402;
    public const uint LESS_THAN = '<'; // 60
    public const uint LESS_THAN_OR_EQUAL = 401;
    public const uint STARTS_WITH = 405;
    public const uint ENDS_WITH = 406;
    public const uint CONTAINS = 407;

    // logical operators
    public const uint LOGICAL_AND = 403;
    public const uint LOGICAL_OR = 404;

    // names
    public const int IDENTIFIER = 500;

    // keywords
    public const int FROM = 300;
    public const int WHERE = 301;
    public const int SKIP = 302;
    public const int TAKE = 303;
}

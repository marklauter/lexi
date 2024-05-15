using Lexi;
using System.Runtime.CompilerServices;

namespace Math.Parser;

internal static class SymbolExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOperator(this Symbol symbol) =>
        symbol.TokenId is TokenIds.ADD or TokenIds.SUBTRACT or TokenIds.MULTIPLY or TokenIds.DIVIDE or TokenIds.MODULUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumericLiteral(this Symbol symbol) =>
        symbol.TokenId is TokenIds.FLOATING_POINT_LITERAL or TokenIds.INTEGER_LITERAL or TokenIds.SCIENTIFIC_NOTATION_LITERAL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpenCircumfixDelimiter(this Symbol symbol) =>
        symbol.TokenId == TokenIds.OPEN_PARENTHESIS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCloseCircumfixDelimiter(this Symbol symbol) =>
        symbol.TokenId == TokenIds.CLOSE_PARENTHESIS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFactor(this Symbol symbol) =>
        symbol.TokenId is TokenIds.MULTIPLY or TokenIds.DIVIDE or TokenIds.MODULUS;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTerm(this Symbol symbol) =>
        symbol.TokenId is TokenIds.ADD or TokenIds.SUBTRACT;
}

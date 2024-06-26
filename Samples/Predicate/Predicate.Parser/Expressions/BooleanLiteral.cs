﻿using System.Runtime.CompilerServices;

namespace Predicate.Parser.Expressions;

public sealed record BooleanLiteral(
    bool Value)
    : Expression
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(BooleanLiteral literal) => literal.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BooleanLiteral(bool value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(BooleanLiteral literal) =>
        literal.Value.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BooleanLiteral(string value) =>
        Boolean.TryParse(value, out var result)
            ? new(result)
            : throw new InvalidOperationException($"value is not a bool '{value}'");
}

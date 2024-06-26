﻿using System.Runtime.CompilerServices;

namespace Predicate.Parser.Expressions;

public sealed record StringLiteral(
    string Value)
    : Expression
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator StringLiteral(string value)
    {
        var start = value.StartsWith('"')
            ? 1
            : 0;

        var end = value.EndsWith('"')
            ? value.Length - 1
            : value.Length;

        return new(value[start..end]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(StringLiteral literal) => literal.Value;
}

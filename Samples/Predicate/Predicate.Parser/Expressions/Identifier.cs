﻿using System.Runtime.CompilerServices;

namespace Predicate.Parser.Expressions;

public sealed record Identifier(
    string Value)
    : Expression
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Identifier(string value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(Identifier value) => value.Value;
}

﻿using System.Globalization;
using System.Runtime.CompilerServices;

namespace Predicate.Parser.Expressions;

public sealed record NumericLiteral(
    NumericTypes Type,
    double Value)
    : Expression
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNaN() => Type == NumericTypes.NotANumber;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(NumericLiteral literal) => Convert.ToInt32(Math.Round(literal.Value, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NumericLiteral(int value) => new(
            NumericTypes.Integer,
            Convert.ToDouble(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double(NumericLiteral literal) => literal.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NumericLiteral(double value) => new(
            NumericTypes.FloatingPoint,
            value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(NumericLiteral literal) => literal.Type switch
    {
        NumericTypes.ScientificNotation or NumericTypes.FloatingPoint => literal.Value
            .ToString(CultureInfo.InvariantCulture),
        NumericTypes.Integer => Convert.ToInt32(Double.Round(literal.Value, 0))
            .ToString(CultureInfo.InvariantCulture),
        NumericTypes.NotANumber or _ => "NaN",
    };
}

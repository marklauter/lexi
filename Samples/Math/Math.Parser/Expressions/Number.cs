﻿using System.Runtime.CompilerServices;

namespace Math.Parser.Expressions;

public sealed record Number(
    NumericTypes Type,
    double Value)
    : Expression
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNaN() => Type == NumericTypes.NotANumber;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override double Evaluate() => Value;

    public override IEnumerable<Expression> Children() => [];

    private string TypeName => Type switch
    {
        NumericTypes.NotANumber => "NaN",
        NumericTypes.Integer => "Integer",
        NumericTypes.FloatingPoint => "FloatingPoint",
        NumericTypes.ScientificNotation => "ScientificNotation",
        _ => throw new NotSupportedException($"unexpected type {Type}"),
    };

    public override void Print(string indent = "")
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        var value = IsNaN()
            ? "NaN"
            : $"{Value}";

        Console.WriteLine($"{indent}{TypeName}: {value}");
        Console.ForegroundColor = color;
    }
}

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record NumericLiteral(
    NumericTypes Type,
    double Value)
    : Expression
{
    /// <summary>
    /// The numeric classification. Rejected at construction unless it names a defined
    /// <see cref="NumericTypes"/> member, so <see cref="ToString"/> cannot fail.
    /// </summary>
    public NumericTypes Type { get; } = Enum.IsDefined(Type)
        ? Type
        : throw new ArgumentOutOfRangeException(nameof(Type));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNaN() => Type == NumericTypes.NotANumber;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32() => Convert.ToInt32(Math.Round(Value, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericLiteral FromInt32(int value) => new(
            NumericTypes.Integer,
            Convert.ToDouble(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble() => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericLiteral FromDouble(double value) => new(
            NumericTypes.FloatingPoint,
            value);

    /// <summary>
    /// The canonical text form, rendered per <see cref="Type"/> in the invariant culture.
    /// </summary>
    public override string ToString() => Type switch
    {
        NumericTypes.ScientificNotation or NumericTypes.FloatingPoint => Value
            .ToString(CultureInfo.InvariantCulture),
        NumericTypes.Integer => Convert.ToInt32(double.Round(Value, 0))
            .ToString(CultureInfo.InvariantCulture),
        NumericTypes.NotANumber or _ => "NaN",
    };
}

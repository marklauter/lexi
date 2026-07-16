using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record BooleanLiteral(
    bool Value)
    : Expression
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ToBoolean() => Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanLiteral FromBoolean(bool value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanLiteral FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return bool.TryParse(value, out var result)
            ? new(result)
            : throw new InvalidOperationException($"value is not a bool '{value}'");
    }

    /// <summary>
    /// The canonical text form — the inverse of <see cref="FromString"/>.
    /// </summary>
    public override string ToString() => Value.ToString();
}

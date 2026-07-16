using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record Identifier(
    string Value)
    : Expression
{
    /// <summary>
    /// The identifier text. Rejected at construction when null, so <see cref="ToString"/> cannot return null.
    /// </summary>
    public string Value { get; } = Value ?? throw new ArgumentNullException(nameof(Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Identifier FromString(string value) => new(value);

    /// <summary>
    /// The canonical text form — the inverse of <see cref="FromString"/>.
    /// </summary>
    public override string ToString() => Value;
}

using System.Runtime.CompilerServices;

namespace Predicate.Parsing.Expressions;

public sealed record CharacterLiteral(
    string Value)
    : Expression
{
    /// <summary>
    /// The unquoted character text. Rejected at construction when null, so <see cref="ToString"/> cannot return null.
    /// </summary>
    public string Value { get; } = Value ?? throw new ArgumentNullException(nameof(Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CharacterLiteral FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var start = value.StartsWith('\'')
            ? 1
            : 0;

        var end = value.EndsWith('\'')
            ? value.Length - 1
            : value.Length;

        return new(value[start..end]);
    }

    /// <summary>
    /// The canonical text form — the inverse of <see cref="FromString"/>, minus the delimiting quotes.
    /// </summary>
    public override string ToString() => Value;
}

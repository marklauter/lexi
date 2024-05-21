using System.Diagnostics.CodeAnalysis;

namespace Predicate.Parser.Expressions;

[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "IDGAD")]
public enum NumericTypes : uint
{
    NotANumber = 0, // NaN
    Integer = TokenIds.INTEGER_LITERAL,
    FloatingPoint = TokenIds.FLOATING_POINT_LITERAL,
    ScientificNotation = TokenIds.SCIENTIFIC_NOTATION_LITERAL,
}

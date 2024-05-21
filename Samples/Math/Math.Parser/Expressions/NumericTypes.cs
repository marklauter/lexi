using System.Diagnostics.CodeAnalysis;

namespace Math.Parser.Expressions;

[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "IDGAD")]
public enum NumericTypes
{
    NotANumber = 0, // NaN
    Integer = 1,
    FloatingPoint = 2,
    ScientificNotation = 3,
}

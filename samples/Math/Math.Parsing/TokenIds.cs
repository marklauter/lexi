namespace Math.Parsing;

internal sealed class TokenIds
{
    public const uint WHITE_SPACE = 0;

    // literals
    public const uint FALSE = 1;
    public const uint TRUE = 2;
    public const uint FLOATING_POINT_LITERAL = 3;
    public const uint INTEGER_LITERAL = 4;
    public const uint SCIENTIFIC_NOTATION_LITERAL = 5;

    // operators
    public const uint ADD = '+'; // 43
    public const uint SUBTRACT = '-'; // 45
    public const uint MULTIPLY = '*'; // 42
    public const uint DIVIDE = '/'; // 47
    public const uint MODULUS = '%'; // 37

    // grouping
    public const uint OPEN_PARENTHESIS = '('; // 40
    public const uint CLOSE_PARENTHESIS = ')'; // 41
}

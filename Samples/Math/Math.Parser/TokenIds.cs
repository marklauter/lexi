namespace Math.Parser;

internal sealed class TokenIds
{
    public const int WHITE_SPACE = 0;

    // literals
    public const int FALSE = 1;
    public const int TRUE = 2;
    public const int FLOATING_POINT_LITERAL = 3;
    public const int INTEGER_LITERAL = 4;
    public const int SCIENTIFIC_NOTATION_LITERAL = 5;

    // operators
    public const int ADD = '+'; // 43
    public const int SUBTRACT = '-'; // 45
    public const int MULTIPLY = '*'; // 42
    public const int DIVIDE = '/'; // 47
    public const int MODULUS = '%'; // 37

    // grouping
    public const int OPEN_PARENTHESIS = '('; // 40
    public const int CLOSE_PARENTHESIS = ')'; // 41
}

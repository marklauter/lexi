using Predicate.Parsing.Exceptions;
using Predicate.Parsing.Expressions;

namespace Predicate.REPL;

internal static class StatementPrinter
{
    public static void Print(this Statement statement)
    {
        PrintExpressionType(nameof(statement.From));
        PrintExpressionValue(statement.From.ToString());
        Console.WriteLine();

        if (statement.Skip != null)
        {
            PrintExpressionType(nameof(statement.Skip));
            PrintExpressionValue(statement.Skip.ToString());
            Console.WriteLine();
        }

        if (statement.Take != null)
        {
            PrintExpressionType(nameof(statement.Take));
            PrintExpressionValue(statement.Take.ToString());
            Console.WriteLine();
        }

        PrintTree(statement.Predicate);
    }

    public static void Print(this ParseException exception)
    {
        var fgcolor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.Message);
        if (exception.Data.Contains("errors"))
        {
            var errors = (IEnumerable<string>?)exception.Data["errors"];
            if (errors is not null)
            {
                foreach (var symbol in errors)
                {
                    Console.WriteLine(symbol);
                }
            }
        }

        Console.ForegroundColor = fgcolor;
    }

    public static void PrintTree(
        Expression expression,
        string indent = "")
    {
        if (expression is BinaryExpression binaryExpression)
        {
            PrintExpressionType(expression.GetType().Name, indent);
            Console.WriteLine();

            indent += "|-- ";
            PrintTree(binaryExpression.Left, indent + "L: ");

            Console.WriteLine();
            PrintExpressionType("Operator", indent);
            PrintExpressionValue(ReadOperator(binaryExpression));
            Console.WriteLine();

            PrintTree(binaryExpression.Right, indent + "R: ");
        }
        else if (expression is ParentheticalExpression parentheticalExpression)
        {
            PrintExpressionType(expression.GetType().Name, indent);
            Console.WriteLine();
            indent += "|-- ";
            PrintTree(parentheticalExpression.Expression, indent + "(: ");
        }
        else
        {
            PrintLeaf(expression, indent);
        }
    }

    private static void PrintExpressionType(string value, string indent = "")
    {
        var fgcolor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(indent);
        Console.Write(value);
        Console.ForegroundColor = fgcolor;
        Console.Write(": ");
    }

    private static void PrintExpressionValue(string value, string indent = "")
    {
        var fgcolor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(indent);
        Console.Write(value);
        Console.ForegroundColor = fgcolor;
    }

    private static string ReadOperator(BinaryExpression binaryExpression) => binaryExpression switch
    {
        ComparisonExpression comparisonExpression => comparisonExpression.ToString(),
        LogicalExpression logicalExpression => logicalExpression.ToString(),
        _ => "unexpected expression",
    };

    private static void PrintLeaf(
        Expression expression,
        string indent)
    {
        var value = expression switch
        {
            Identifier identifier => identifier.ToString(),
            Keyword keyword => keyword.ToString(),
            BooleanLiteral booleanLiteral => booleanLiteral.ToString(),
            StringLiteral stringLiteral => stringLiteral.ToString(),
            NumericLiteral numericLiteral => numericLiteral.ToString(),
            _ => "unexpected expression",
        };

        PrintExpressionType(expression.GetType().Name, indent);
        PrintExpressionValue(value);
    }
}

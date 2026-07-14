using System.Text.RegularExpressions;

namespace Lexi.Tests;

public sealed class SQLiteTest
{
    internal enum StatementTokens : uint
    {
        Create,
        Select,
        Alter,
        Drop,
        Insert,
        Update,
        Delete,
        Whitespace,
    }

    private static readonly Lexer Lexer = VocabularyBuilder
        .Create(RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        .Match("CREATE", (uint)StatementTokens.Create)
        .Match(nameof(StatementTokens.Select), (uint)StatementTokens.Select)
        .Match(nameof(StatementTokens.Alter), (uint)StatementTokens.Alter)
        .Match(nameof(StatementTokens.Drop), (uint)StatementTokens.Drop)
        .Match(nameof(StatementTokens.Insert), (uint)StatementTokens.Insert)
        .Match(nameof(StatementTokens.Update), (uint)StatementTokens.Update)
        .Match(nameof(StatementTokens.Delete), (uint)StatementTokens.Delete)
        .Ignore(CommonPatterns.NewLine(), (uint)StatementTokens.Whitespace)
        .Ignore(CommonPatterns.Whitespace(), (uint)StatementTokens.Whitespace)
        .Build();

    [Fact]
    public void SQLiteCreateStatement()
    {
        var sql = File.ReadAllText("sqlitecreate.txt");
        var result = Lexer.NextMatch(sql);
        Assert.True(result.Symbol.IsMatch);
    }
}

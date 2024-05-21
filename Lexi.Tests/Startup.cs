using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class Startup
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "xunit requires instance method")]
    public void ConfigureServices(IServiceCollection services) => services
        .TryAddTransient(serviceProvider =>
            VocabularyBuilder
                .Create(RegexOptions.CultureInvariant)
                .Match(@"\G\-?\d+\.\d+", (uint)TestToken.FloatingPointLiteral)
                .Match(@"\G\-?\d+", (uint)TestToken.IntegerLiteral)
                .Match(@"\G\+", (uint)TestToken.AdditionOperator)
                .Match(@"\G\-", (uint)TestToken.SubtractionOperator)
                .Match(@"\G\*", (uint)TestToken.MultiplicationOperator)
                .Match(@"\G/", (uint)TestToken.DivisionOperator)
                .Match(@"\G%", (uint)TestToken.ModulusOperator)
                .Match(@"\G<", (uint)TestToken.GreaterThanOperator)
                .Match(@"\G<=", (uint)TestToken.GreaterThanOrEqualOperator)
                .Match(CommonPatterns.QuotedStringLiteral(), (uint)TestToken.StringLiteral)
                .Ignore(CommonPatterns.Whitespace(), (uint)TestToken.WhiteSpace)
                .Ignore(CommonPatterns.NewLine(), (uint)TestToken.WhiteSpace)
                .Build());
}

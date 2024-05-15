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
                .Match(@"\G\-?\d+\.\d+", TestToken.FloatingPointLiteral)
                .Match(@"\G\-?\d+", TestToken.IntegerLiteral)
                .Match(@"\G\+", TestToken.AdditionOperator)
                .Match(@"\G\-", TestToken.SubtractionOperator)
                .Match(@"\G\*", TestToken.MultiplicationOperator)
                .Match(@"\G/", TestToken.DivisionOperator)
                .Match(@"\G%", TestToken.ModulusOperator)
                .Match(@"\G<", TestToken.GreaterThanOperator)
                .Match(@"\G<=", TestToken.GreaterThanOrEqualOperator)
                .Match(CommonPatterns.QuotedStringLiteral(), TestToken.StringLiteral)
                .Build());
}

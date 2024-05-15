using Lexi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;

namespace Math.Parser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddParser(this IServiceCollection services)
    {
        var builder = VocabularyBuilder
            .Create(RegexOptions.CultureInvariant)
            .Match("false", TokenIds.FALSE)
            .Match("true", TokenIds.TRUE)
            .Match(CommonPatterns.IntegerLiteral(), TokenIds.INTEGER_LITERAL)
            .Match(CommonPatterns.FloatingPointLiteral(), TokenIds.FLOATING_POINT_LITERAL)
            .Match(CommonPatterns.ScientificNotationLiteral(), TokenIds.SCIENTIFIC_NOTATION_LITERAL)
            .Match(@"\+", TokenIds.ADD)
            .Match("-", TokenIds.SUBTRACT)
            .Match(@"\*", TokenIds.MULTIPLY)
            .Match("/", TokenIds.DIVIDE)
            .Match("%", TokenIds.MODULUS)
            .Match(@"\(", TokenIds.OPEN_PARENTHESIS)
            .Match(@"\)", TokenIds.CLOSE_PARENTHESIS);

        services.TryAddTransient(serviceProvider => builder.Build());
        services.TryAddTransient<Parser>();

        return services;
    }
}

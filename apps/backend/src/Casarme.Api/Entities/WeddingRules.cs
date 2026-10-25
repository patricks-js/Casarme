using System.Text.RegularExpressions;

namespace Casarme.Api.Entities;

public static partial class WeddingRules
{
    public const int NameMaxLength = 100;
    public const int SlugMinLength = 3;
    public const int SlugMaxLength = 60;

    // lowercase letters and digits, single hyphens between them (e.g. "ana-e-joao")
    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    public static bool IsValidSlug(string? slug) =>
        slug is { Length: >= SlugMinLength and <= SlugMaxLength } && SlugPattern().IsMatch(slug);
}

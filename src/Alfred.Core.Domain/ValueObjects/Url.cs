using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Exceptions;

namespace Alfred.Core.Domain.ValueObjects;

/// <summary>
/// Value Object for an HTTP or HTTPS URL field.
/// </summary>
public sealed class Url : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; private set; } = string.Empty;

    private Url() { }

    private Url(string value) => Value = value;

    public static Url Create(string? urlString)
    {
        if (string.IsNullOrWhiteSpace(urlString))
        {
            return new Url(string.Empty);
        }

        var sanitized = urlString.Trim();

        if (sanitized.Length > MaxLength)
        {
            throw new DomainException($"URL cannot exceed {MaxLength} characters.");
        }

        if (!IsValidUrl(sanitized))
        {
            throw new DomainException("URL must be a valid HTTP or HTTPS address.");
        }

        return new Url(sanitized);
    }

    public static Url Empty() => new(string.Empty);

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Value);

    public bool IsHttps()
    {
        if (IsEmpty())
        {
            return false;
        }

        return Uri.TryCreate(Value, UriKind.Absolute, out var uri)
               && uri.Scheme == Uri.UriSchemeHttps;
    }

    private static bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var abs))
        {
            return abs.Scheme == Uri.UriSchemeHttp || abs.Scheme == Uri.UriSchemeHttps;
        }

        return Uri.TryCreate(url, UriKind.Relative, out _) && url.StartsWith("/");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Url url) => url.Value;

    public static explicit operator Url(string? s) => Create(s);
}

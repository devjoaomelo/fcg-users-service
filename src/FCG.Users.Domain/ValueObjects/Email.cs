using System.Text.RegularExpressions;


namespace FCG.Users.Domain.ValueObjects;
public sealed partial record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Email is required");
        if (!EmailRegex().IsMatch(value)) throw new ArgumentException("Invalid email");

        return new Email(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}

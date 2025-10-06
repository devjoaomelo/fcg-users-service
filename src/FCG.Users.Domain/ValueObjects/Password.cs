using System.Text.RegularExpressions;


namespace FCG.Users.Domain.ValueObjects;
public sealed record Password
{
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Password is required");
        if (!Regex.IsMatch(value, @"^(?=.*\p{Lu})(?=.*\p{Ll})(?=.*\d)(?=.*[\p{P}\p{S}]).{8,}$")) 
            throw new ArgumentException("Password must have at least 8 chars, 1 letter, 1 number and 1 special character");

        return new Password(value);
    }

    public override string ToString() => "[Protected]";
}

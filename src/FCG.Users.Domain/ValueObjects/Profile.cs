namespace FCG.Users.Domain.ValueObjects;
public sealed record Profile
{
    public string Value { get; }

    private Profile(string value)
    {
        Value = value;
    }

    public static readonly Profile User = new("User");
    public static readonly Profile Admin = new("Admin");

    public static Profile Parse(string? value)
    {
        return string.Equals(value, "Admin", StringComparison.OrdinalIgnoreCase) ? Admin : User;
    }

    public override string ToString() => Value;
}


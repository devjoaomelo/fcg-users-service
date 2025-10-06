using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public Profile Profile { get; private set; }

    protected User() { }

    public User(string name, Email email, Password password)
    {
        SetName(name);
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Id = Guid.NewGuid();
        Profile = Profile.User;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
    }

    public void PromoteToAdmin()
    {
        Profile = Profile.Admin;
    }

    public void Update(string name, Password password)
    {
        SetName(name);
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }
}


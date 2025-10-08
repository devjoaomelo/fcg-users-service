using FluentAssertions;
using Moq;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.UnitTests;

public class UserCreationService_FirstUserTests
{
    [Fact]
    public async Task CreateUserAsync_Should_Promote_To_Admin_When_First_User()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);

        var name = "Alice";
        var email = "alice@test.com";
        var rawPassword = "Abcdef1!";
        var hashedPassword = "HashOk1!";

        hasher.Setup(h => h.Hash(rawPassword)).Returns(hashedPassword);

        repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new UserCreationService(repo.Object, hasher.Object);
        var user = await service.CreateUserAsync(name, email, rawPassword, CancellationToken.None);

        user.Should().NotBeNull();

        user.Email.Value.Should().Be(email);

        user.Profile.Should().Be(Profile.Admin);
    }

    [Fact]
    public async Task CreateUserAsync_Should_Not_Promote_When_Not_First_User()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);

        var name = "Bob";
        var email = "bob@test.com";
        var rawPassword = "Abcdef1!";
        var hashedPassword = "HashOk1!";

        hasher.Setup(h => h.Hash(rawPassword)).Returns(hashedPassword);
        repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = new UserCreationService(repo.Object, hasher.Object);
        var user = await service.CreateUserAsync(name, email, rawPassword, CancellationToken.None);

        user.Should().NotBeNull();

        user.Profile.Should().Be(Profile.User);
    }
}

using FluentAssertions;
using Moq;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.UnitTests;

public class UserCreationService_PasswordHashingTests
{
    [Fact]
    public async Task CreateUserAsync_Should_Store_Hashed_Password_Not_Raw()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);

        var name = "Carol";
        var email = "carol@test.com";
        var rawPassword = "Abcdef1!";
        var hashedPassword = "HashOk1!";

        hasher.Setup(h => h.Hash(rawPassword)).Returns(hashedPassword);
        repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserCreationService(repo.Object, hasher.Object);

        var user = await service.CreateUserAsync(name, email, rawPassword, CancellationToken.None);

        user.Password.Should().NotBeNull();
        user.Password.Value.Should().Be(hashedPassword);
        user.Password.Value.Should().NotBe(rawPassword);
    }
}

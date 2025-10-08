using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.UnitTests;

public class UserCreationService_EmailTests
{
    [Fact]
    public async Task CreateUserAsync_Should_Throw_When_Email_Already_Exists()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);

        var email = "user@teste.com";
        var name = "John Doe";
        var rawPassword = "Abcdef1!";        
        var hashedPassword = "Xyz12345!";   

        hasher.Setup(h => h.Hash(rawPassword)).Returns(hashedPassword);

        repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new UserCreationService(repo.Object, hasher.Object);

        var act = async () => await service.CreateUserAsync(
            name: name,
            email: email,
            password: rawPassword,
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        hasher.Verify(h => h.Hash(rawPassword), Times.Once);
        repo.Verify(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

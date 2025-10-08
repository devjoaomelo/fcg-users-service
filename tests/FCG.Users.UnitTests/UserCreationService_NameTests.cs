using FluentAssertions;
using Moq;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;

namespace FCG.Users.UnitTests;

public class UserCreationService_NameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateUserAsync_Should_Throw_When_Name_Is_NullOrWhiteSpace(string? badName)
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);

        var service = new UserCreationService(repo.Object, hasher.Object);

        var act = async () => await service.CreateUserAsync(
            name: badName!,
            email: "user@teste.com",
            password: "Senha@123",
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithParameterName("name");

        hasher.VerifyNoOtherCalls();
        repo.VerifyNoOtherCalls();
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using FCG.Users.Application.Interfaces;
using FCG.Users.Application.UseCases.Users.Login;

namespace FCG.Users.UnitTests;

public class LoginUserHandler_FailureTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Credentials_Are_Invalid()
    {
        var auth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenGenerator>(MockBehavior.Strict);

        var req = new LoginUserRequest("john@test.com", "WrongPass1!");

        auth.Setup(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        var handler = new LoginUserHandler(auth.Object, jwt.Object);

        var act = async () => await handler.Handle(
            request: req,
            issuer: "fcg-users",
            audience: "fcg-clients",
            signingKey: "DEV_ONLY_CHANGE_THIS_32CHARS_MINIMUM________________",
            ttl: TimeSpan.FromHours(2),
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("*Invalid credentials*");

        jwt.VerifyNoOtherCalls();
        auth.Verify(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var auth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenGenerator>(MockBehavior.Strict);

        var req = new LoginUserRequest("ghost@test.com", "Abcdef1!");

        auth.Setup(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User not found"));

        var handler = new LoginUserHandler(auth.Object, jwt.Object);

        var act = async () => await handler.Handle(
            req, "fcg-users", "fcg-clients",
            "DEV_ONLY_CHANGE_THIS_32CHARS_MINIMUM________________",
            TimeSpan.FromHours(2),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        jwt.VerifyNoOtherCalls();
        auth.Verify(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()), Times.Once);
    }
}

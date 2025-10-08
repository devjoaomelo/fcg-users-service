using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using FCG.Users.Application.Interfaces;
using FCG.Users.Application.UseCases.Users.Login;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.ValueObjects;

namespace FCG.Users.UnitTests;

public class LoginUserHandler_SuccessTests
{
    [Fact]
    public async Task Handle_Should_Return_Token_And_Expiry_When_Credentials_Are_Valid()
    {
        var auth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenGenerator>(MockBehavior.Strict);

        var req = new LoginUserRequest("john@test.com", "Abcdef1!");
        var issuer = "fcg-users";
        var audience = "fcg-clients";
        var key = "DEV_ONLY_CHANGE_THIS_32CHARS_MINIMUM________________";
        var ttl = TimeSpan.FromHours(2);

        var user = new User(
            name: "John",
            email: Email.Create(req.Email),
            password: Password.Create("HashOk1!") 
        );
        user.PromoteToAdmin();

        auth.Setup(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var expectedToken = "header.payload.signature";
        DateTime expectedExpiry = DateTime.UtcNow.Add(ttl);

        jwt.Setup(j => j.Generate(
                user.Id,
                user.Name,
                user.Email.Value,
                user.Profile.Value, 
                issuer,
                audience,
                key,
                out expectedExpiry,
                ttl))
           .Returns(expectedToken);

        var handler = new LoginUserHandler(auth.Object, jwt.Object);

        var result = await handler.Handle(req, issuer, audience, key, ttl, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be(expectedToken);
        result.ExpiresAtUtc.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));

        auth.Verify(a => a.AuthenticateAsync(req.Email, req.Password, It.IsAny<CancellationToken>()), Times.Once);
        jwt.Verify(j => j.Generate(
            user.Id, user.Name, user.Email.Value, user.Profile.Value,
            issuer, audience, key, out expectedExpiry, ttl), Times.Once);
        auth.VerifyNoOtherCalls();
        jwt.VerifyNoOtherCalls();
    }
}

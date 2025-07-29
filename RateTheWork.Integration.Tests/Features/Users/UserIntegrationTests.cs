using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RateTheWork.Application.Features.Users.Commands.RegisterUser;
using RateTheWork.Application.Features.Users.Commands.LoginUser;
using RateTheWork.Integration.Tests.Common;

namespace RateTheWork.Integration.Tests.Features.Users;

/// <summary>
/// Integration tests for user-related features including registration, login, and email verification.
/// Tests the entire flow from API endpoint to database persistence.
/// </summary>
public class UserIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task RegisterUser_Should_CreateNewUser_WhenValidData()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+905551234567"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<RegisterUserCommandResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.IsEmailVerified.Should().BeFalse();

        // Verify database
        var user = await ExecuteDbContextAsync(async db =>
            await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email));
        
        user.Should().NotBeNull();
        user!.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
    }

    [Fact]
    public async Task RegisterUser_Should_ReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "existing@example.com",
            Password = "Test123!@#",
            FirstName = "Jane",
            LastName = "Doe",
            PhoneNumber = "+905559876543"
        };

        // First registration
        await Client.PostAsJsonAsync("/api/users/register", command);

        // Act - Second registration with same email
        var response = await Client.PostAsJsonAsync("/api/users/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginUser_Should_ReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Email = "login@example.com",
            Password = "Test123!@#",
            FirstName = "Login",
            LastName = "User",
            PhoneNumber = "+905555555555"
        };

        await Client.PostAsJsonAsync("/api/users/register", registerCommand);

        var loginCommand = new LoginUserCommand
        {
            Email = registerCommand.Email,
            Password = registerCommand.Password
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginUserCommandResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be(loginCommand.Email);
    }

    [Fact]
    public async Task LoginUser_Should_ReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Email = "wrongpass@example.com",
            Password = "Test123!@#",
            FirstName = "Wrong",
            LastName = "Password",
            PhoneNumber = "+905554444444"
        };

        await Client.PostAsJsonAsync("/api/users/register", registerCommand);

        var loginCommand = new LoginUserCommand
        {
            Email = registerCommand.Email,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyEmail_Should_UpdateUserStatus_WhenTokenIsValid()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Email = "verify@example.com",
            Password = "Test123!@#",
            FirstName = "Verify",
            LastName = "Email",
            PhoneNumber = "+905553333333"
        };

        await Client.PostAsJsonAsync("/api/users/register", registerCommand);

        // Get verification token from database
        var user = await ExecuteDbContextAsync(async db =>
            await db.Users.FirstOrDefaultAsync(u => u.Email == registerCommand.Email));

        var verificationToken = user!.EmailVerificationToken;

        // Act
        var response = await Client.PostAsync($"/api/users/verify-email?token={verificationToken}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database update
        var updatedUser = await ExecuteDbContextAsync(async db =>
            await db.Users.FirstOrDefaultAsync(u => u.Email == registerCommand.Email));
        
        updatedUser!.IsEmailVerified.Should().BeTrue();
        updatedUser.EmailVerificationToken.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfile_Should_ReturnUserData_WhenAuthenticated()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Email = "profile@example.com",
            Password = "Test123!@#",
            FirstName = "Profile",
            LastName = "Test",
            PhoneNumber = "+905552222222"
        };

        await Client.PostAsJsonAsync("/api/users/register", registerCommand);

        var loginCommand = new LoginUserCommand
        {
            Email = registerCommand.Email,
            Password = registerCommand.Password
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/users/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>();

        // Add authorization header
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Act
        var response = await Client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var profile = await response.Content.ReadFromJsonAsync<dynamic>();
        profile.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserProfile_Should_ReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await Client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
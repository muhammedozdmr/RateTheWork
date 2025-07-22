using FluentAssertions;
using Moq;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Services;

public class UserDomainServiceTests : DomainTestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly Mock<ITcIdentityValidationService> _tcIdentityValidationServiceMock;
    private readonly UserDomainService _service;

    public UserDomainServiceTests()
    {
        _userRepositoryMock = CreateMock<IUserRepository>();
        _passwordHashingServiceMock = CreateMock<IPasswordHashingService>();
        _tcIdentityValidationServiceMock = CreateMock<ITcIdentityValidationService>();

        _service = new UserDomainService(
            _userRepositoryMock.Object,
            _passwordHashingServiceMock.Object,
            _tcIdentityValidationServiceMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var password = "StrongP@ssw0rd!";
        var tcIdentityNumber = "12345678901";
        var firstName = "John";
        var lastName = "Doe";
        var birthYear = 1990;
        var hashedPassword = "hashedPassword123";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(tcIdentityNumber))
            .ReturnsAsync((User)null);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(password))
            .Returns(hashedPassword);
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(tcIdentityNumber, firstName, lastName, birthYear))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterUserAsync(email, password, tcIdentityNumber, firstName, lastName, birthYear);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.TcIdentityNumber.Should().Be(tcIdentityNumber);
        result.PasswordHash.Should().Be(hashedPassword);
        result.IsBanned.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User { Email = email };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _service.RegisterUserAsync(
            email, "password", "12345678901", "John", "Doe", 1990);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("User with this email already exists");
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingTcIdentity_ShouldThrowException()
    {
        // Arrange
        var tcIdentityNumber = "12345678901";
        var existingUser = new User { TcIdentityNumber = tcIdentityNumber };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(tcIdentityNumber))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _service.RegisterUserAsync(
            "test@example.com", "password", tcIdentityNumber, "John", "Doe", 1990);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("User with this TC Identity Number already exists");
    }

    [Fact]
    public async Task RegisterUserAsync_WithInvalidTcIdentity_ShouldThrowException()
    {
        // Arrange
        var tcIdentityNumber = "12345678901";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(tcIdentityNumber))
            .ReturnsAsync((User)null);
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            tcIdentityNumber, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _service.RegisterUserAsync(
            "test@example.com", "password", tcIdentityNumber, "John", "Doe", 1990);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("Invalid TC Identity information");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidOldPassword_ShouldChangePassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPassword = "OldP@ssw0rd!";
        var newPassword = "NewP@ssw0rd!";
        var currentHash = "currentHash";
        var newHash = "newHash";
        
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = currentHash
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(oldPassword, currentHash))
            .Returns(true);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(newPassword))
            .Returns(newHash);

        // Act
        await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        user.PasswordHash.Should().Be(newHash);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidOldPassword_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "currentHash"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var act = async () => await _service.ChangePasswordAsync(userId, "wrongPassword", "newPassword");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedDomainActionException>()
            .WithMessage("Invalid old password");
    }

    [Fact]
    public async Task BanUserAsync_WithValidUser_ShouldBanUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reason = "Violation of terms";
        var banDuration = TimeSpan.FromDays(30);
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            IsBanned = false
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.BanUserAsync(userId, reason, banDuration);

        // Assert
        user.IsBanned.Should().BeTrue();
        user.BanReason.Should().Be(reason);
        user.BannedUntil.Should().BeCloseTo(DateTime.UtcNow.Add(banDuration), TimeSpan.FromMinutes(1));
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task BanUserAsync_WithAlreadyBannedUser_ShouldNotUpdateAgain()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            IsBanned = true,
            BanReason = "Previous violation",
            BannedUntil = DateTime.UtcNow.AddDays(10)
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.BanUserAsync(userId, "New reason", TimeSpan.FromDays(30));

        // Assert
        user.IsBanned.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UnbanUserAsync_WithBannedUser_ShouldUnbanUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            IsBanned = true,
            BanReason = "Previous violation",
            BannedUntil = DateTime.UtcNow.AddDays(10)
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.UnbanUserAsync(userId);

        // Assert
        user.IsBanned.Should().BeFalse();
        user.BanReason.Should().BeNull();
        user.BannedUntil.Should().BeNull();
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UnbanUserAsync_WithNonBannedUser_ShouldNotUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            IsBanned = false
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.UnbanUserAsync(userId);

        // Assert
        user.IsBanned.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAnonymitySettingsAsync_WithValidSettings_ShouldUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anonymityLevel = AnonymityLevel.PartiallyAnonymous;
        var anonymousUsername = "anonymous123";
        var user = new User
        {
            Id = userId,
            Email = "test@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.UpdateAnonymitySettingsAsync(userId, anonymityLevel, anonymousUsername);

        // Assert
        user.AnonymityLevel.Should().Be(anonymityLevel);
        user.AnonymousUsername.Should().Be(anonymousUsername);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateAnonymitySettingsAsync_WithInvalidUsername_ShouldThrowException(string invalidUsername)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _service.UpdateAnonymitySettingsAsync(
            userId, AnonymityLevel.PartiallyAnonymous, invalidUsername);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("Anonymous username is required for non-public anonymity levels");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedUser);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User)null);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserCredentialsAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidP@ssw0rd!";
        var passwordHash = "hashedPassword";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsBanned = false
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(true);

        // Act
        var result = await _service.ValidateUserCredentialsAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(user);
    }

    [Fact]
    public async Task ValidateUserCredentialsAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword"
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _service.ValidateUserCredentialsAsync(email, "wrongPassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserCredentialsAsync_WithBannedUser_ShouldReturnNull()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidP@ssw0rd!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword",
            IsBanned = true
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _service.ValidateUserCredentialsAsync(email, password);

        // Assert
        result.Should().BeNull();
    }
}
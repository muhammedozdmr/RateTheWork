using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Features.Users.Commands.RegisterUser;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;

namespace RateTheWork.Application.Tests.Features.Users.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly Mock<ITcIdentityValidationService> _tcIdentityValidationServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RegisterUserCommandHandler _handler;
    private readonly IFixture _fixture;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _tcIdentityValidationServiceMock = new Mock<ITcIdentityValidationService>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _fixture = new Fixture();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHashingServiceMock.Object,
            _tcIdentityValidationServiceMock.Object,
            _emailServiceMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        var hashedPassword = "hashedPassword123";
        var emailVerificationToken = "verificationToken123";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync((User)null);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            command.TcIdentityNumber, command.FirstName, command.LastName, command.BirthYear))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        User capturedUser = null;
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => 
            {
                capturedUser = u;
                capturedUser.EmailVerificationToken = emailVerificationToken;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(command.Email);
        result.Data.IsEmailVerified.Should().BeFalse();

        capturedUser.Should().NotBeNull();
        capturedUser.Email.Should().Be(command.Email);
        capturedUser.PasswordHash.Should().Be(hashedPassword);
        capturedUser.TcIdentityNumber.Should().Be(command.TcIdentityNumber);
        capturedUser.IsBanned.Should().BeFalse();

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            command.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "existing@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        var existingUser = new User { Email = command.Email };
        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already registered");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingTcIdentity_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);

        var existingUser = new User { TcIdentityNumber = command.TcIdentityNumber };
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("TC Identity Number");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidTcIdentity_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync((User)null);
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            command.TcIdentityNumber, command.FirstName, command.LastName, command.BirthYear))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid TC Identity");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldGenerateVerificationToken()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync((User)null);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        User capturedUser = null;
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser.EmailVerificationToken.Should().NotBeNullOrEmpty();
        capturedUser.EmailVerificationTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        capturedUser.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPublishUserRegisteredEvent()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync((User)null);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        User capturedUser = null;
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser.DomainEvents.Should().NotBeEmpty();
        capturedUser.DomainEvents.Should().ContainItemsAssignableTo<Domain.Events.User.UserRegisteredEvent>();
    }

    [Fact]
    public async Task Handle_WhenEmailServiceFails_ShouldStillSucceed()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.GetUserByTcIdentityNumberAsync(command.TcIdentityNumber))
            .ReturnsAsync((User)null);
        _passwordHashingServiceMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");
        _tcIdentityValidationServiceMock.Setup(x => x.ValidateTcIdentityAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("12345678")]
    [InlineData("password")]
    [InlineData("Password")]
    public async Task Handle_WithWeakPassword_ShouldReturnFailure(string weakPassword)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = weakPassword,
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("password");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure(string invalidEmail)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = invalidEmail,
            Password = "StrongP@ssw0rd!",
            TcIdentityNumber = "12345678901",
            FirstName = "John",
            LastName = "Doe",
            BirthYear = 1990
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
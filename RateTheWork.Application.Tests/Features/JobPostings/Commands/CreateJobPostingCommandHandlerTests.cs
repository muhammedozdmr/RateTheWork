using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Features.JobPostings.Commands.CreateJobPosting;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Application.Tests.Features.JobPostings.Commands;

public class CreateJobPostingCommandHandlerTests
{
    private readonly Mock<IRepository<JobPosting>> _jobPostingRepositoryMock;
    private readonly Mock<IRepository<CompanySubscription>> _companySubscriptionRepositoryMock;
    private readonly Mock<IRepository<HRPersonnel>> _hrPersonnelRepositoryMock;
    private readonly Mock<IValidator<CreateJobPostingCommand>> _validatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly CreateJobPostingCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreateJobPostingCommandHandlerTests()
    {
        _jobPostingRepositoryMock = new Mock<IRepository<JobPosting>>();
        _companySubscriptionRepositoryMock = new Mock<IRepository<CompanySubscription>>();
        _hrPersonnelRepositoryMock = new Mock<IRepository<HRPersonnel>>();
        _validatorMock = new Mock<IValidator<CreateJobPostingCommand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _fixture = new Fixture();

        _handler = new CreateJobPostingCommandHandler(
            _jobPostingRepositoryMock.Object,
            _companySubscriptionRepositoryMock.Object,
            _hrPersonnelRepositoryMock.Object,
            _validatorMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateJobPosting()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var hrPersonnelId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = hrPersonnelId,
            Title = "Senior Software Engineer",
            Description = "We are looking for an experienced developer...",
            Requirements = "5+ years of experience...",
            Location = new Address("Turkey", "Istanbul", "Kadikoy", "Tech Street", "34710"),
            FirstInterviewDate = DateTime.UtcNow.AddDays(14),
            TargetApplicationCount = 50,
            HiringProcess = "Phone Screen -> Technical Interview -> HR Interview -> Offer",
            EstimatedProcessDays = 30,
            EmploymentType = EmploymentType.FullTime,
            WorkLocation = WorkLocation.Hybrid,
            ExperienceLevel = ExperienceLevel.Senior,
            SalaryRange = new SalaryRange(100000, 150000, "USD", SalaryPeriod.Yearly)
        };

        var subscription = CompanySubscription.Create(
            companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            10,
            5);

        var hrPersonnel = HRPersonnel.Create(
            companyId,
            Guid.NewGuid(),
            "John Doe",
            "HR Manager",
            "HR",
            "john@company.com",
            "+905551234567",
            null);
        hrPersonnel.Verify();

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription> { subscription });

        _hrPersonnelRepositoryMock.Setup(x => x.GetByIdAsync(hrPersonnelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hrPersonnel);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Title.Should().Be(command.Title);
        result.Data.CompanyId.Should().Be(companyId);
        result.Data.HRPersonnelId.Should().Be(hrPersonnelId);

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.Is<JobPosting>(jp => 
                jp.Title == command.Title &&
                jp.CompanyId == companyId &&
                jp.HRPersonnelId == hrPersonnelId),
            It.IsAny<CancellationToken>()), Times.Once);

        _companySubscriptionRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<CompanySubscription>(cs => cs.UsedJobPostingQuota == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateJobPostingCommand
        {
            CompanyId = Guid.NewGuid(),
            Title = ""  // Invalid
        };

        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required"),
            new ValidationFailure("Description", "Description is required")
        });

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Title is required");
        result.Error.Should().Contain("Description is required");

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<JobPosting>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoActiveSubscription_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = Guid.NewGuid(),
            Title = "Software Engineer"
        };

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No active subscription found");

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<JobPosting>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExhaustedQuota_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = Guid.NewGuid(),
            Title = "Software Engineer"
        };

        var subscription = CompanySubscription.Create(
            companyId,
            CompanySubscriptionPlan.Basic,
            BillingCycle.Monthly,
            5,
            2);

        // Exhaust the quota
        for (int i = 0; i < 5; i++)
        {
            subscription.ConsumeJobPostingQuota();
        }

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription> { subscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Job posting quota exceeded");

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<JobPosting>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnverifiedHRPersonnel_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var hrPersonnelId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = hrPersonnelId,
            Title = "Software Engineer"
        };

        var subscription = CompanySubscription.Create(
            companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            10,
            5);

        var hrPersonnel = HRPersonnel.Create(
            companyId,
            Guid.NewGuid(),
            "John Doe",
            "HR Manager",
            "HR",
            "john@company.com",
            "+905551234567",
            null);
        // Not verified

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription> { subscription });

        _hrPersonnelRepositoryMock.Setup(x => x.GetByIdAsync(hrPersonnelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hrPersonnel);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("HR personnel is not verified");

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<JobPosting>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveHRPersonnel_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var hrPersonnelId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = hrPersonnelId,
            Title = "Software Engineer"
        };

        var subscription = CompanySubscription.Create(
            companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            10,
            5);

        var hrPersonnel = HRPersonnel.Create(
            companyId,
            Guid.NewGuid(),
            "John Doe",
            "HR Manager",
            "HR",
            "john@company.com",
            "+905551234567",
            null);
        hrPersonnel.Verify();
        hrPersonnel.Deactivate();

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription> { subscription });

        _hrPersonnelRepositoryMock.Setup(x => x.GetByIdAsync(hrPersonnelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hrPersonnel);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("HR personnel is not active");

        _jobPostingRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<JobPosting>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAssignHRPersonnelToJobPosting()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var hrPersonnelId = Guid.NewGuid();
        var command = new CreateJobPostingCommand
        {
            CompanyId = companyId,
            HRPersonnelId = hrPersonnelId,
            Title = "Senior Developer",
            Description = "Description",
            Requirements = "Requirements",
            Location = new Address("Turkey", "Istanbul", "Kadikoy", "Street", "34710"),
            FirstInterviewDate = DateTime.UtcNow.AddDays(14),
            TargetApplicationCount = 50,
            HiringProcess = "Process",
            EstimatedProcessDays = 30
        };

        var subscription = CompanySubscription.Create(companyId, CompanySubscriptionPlan.Professional, BillingCycle.Monthly, 10, 5);
        var hrPersonnel = HRPersonnel.Create(companyId, Guid.NewGuid(), "John Doe", "HR Manager", "HR", "john@company.com", "+905551234567", null);
        hrPersonnel.Verify();

        _validatorMock.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _companySubscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanySubscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanySubscription> { subscription });

        _hrPersonnelRepositoryMock.Setup(x => x.GetByIdAsync(hrPersonnelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hrPersonnel);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        JobPosting capturedJobPosting = null;
        _jobPostingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<JobPosting>(), It.IsAny<CancellationToken>()))
            .Callback<JobPosting, CancellationToken>((jp, ct) => capturedJobPosting = jp)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        hrPersonnel.TotalJobPostings.Should().Be(1);
        hrPersonnel.LastJobPostingAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        _hrPersonnelRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<HRPersonnel>(hr => hr.TotalJobPostings == 1),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedJobPosting.Should().NotBeNull();
        hrPersonnel.GetActiveJobPostingIds().Should().Contain(capturedJobPosting.Id);
    }
}
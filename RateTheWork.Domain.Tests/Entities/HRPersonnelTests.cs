using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Events.HRPersonnel;
using RateTheWork.Domain.Exceptions.HRPersonnelException;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Entities;

public class HRPersonnelTests : DomainTestBase
{
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly string _fullName = "John Doe";
    private readonly string _title = "Senior HR Manager";
    private readonly string _department = "Human Resources";
    private readonly string _email = "john.doe@company.com";
    private readonly string _phoneNumber = "+1234567890";
    private readonly string _linkedInProfile = "https://linkedin.com/in/johndoe";

    [Fact]
    public void Create_WithValidParameters_ShouldCreateHRPersonnel()
    {
        // Act
        var hrPersonnel = HRPersonnel.Create(
            _companyId,
            _userId,
            _fullName,
            _title,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        hrPersonnel.Should().NotBeNull();
        hrPersonnel.CompanyId.Should().Be(_companyId);
        hrPersonnel.UserId.Should().Be(_userId);
        hrPersonnel.FullName.Should().Be(_fullName);
        hrPersonnel.Title.Should().Be(_title);
        hrPersonnel.Department.Should().Be(_department);
        hrPersonnel.Email.Should().Be(_email);
        hrPersonnel.PhoneNumber.Should().Be(_phoneNumber);
        hrPersonnel.LinkedInProfile.Should().Be(_linkedInProfile);
        hrPersonnel.IsActive.Should().BeTrue();
        hrPersonnel.IsVerified.Should().BeFalse();
        hrPersonnel.DomainEvents.Should().ContainSingle(e => e is HRPersonnelCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyCompanyId_ShouldThrowException()
    {
        // Act
        var act = () => HRPersonnel.Create(
            Guid.Empty,
            _userId,
            _fullName,
            _title,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("Company ID is required");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowException()
    {
        // Act
        var act = () => HRPersonnel.Create(
            _companyId,
            Guid.Empty,
            _fullName,
            _title,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("User ID is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidFullName_ShouldThrowException(string invalidName)
    {
        // Act
        var act = () => HRPersonnel.Create(
            _companyId,
            _userId,
            invalidName,
            _title,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("Full name is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidTitle_ShouldThrowException(string invalidTitle)
    {
        // Act
        var act = () => HRPersonnel.Create(
            _companyId,
            _userId,
            _fullName,
            invalidTitle,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("Title is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Act
        var act = () => HRPersonnel.Create(
            _companyId,
            _userId,
            _fullName,
            _title,
            _department,
            invalidEmail,
            _phoneNumber,
            _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public void Verify_WhenNotVerified_ShouldVerifyPersonnel()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();

        // Act
        hrPersonnel.Verify();

        // Assert
        hrPersonnel.IsVerified.Should().BeTrue();
        hrPersonnel.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelVerifiedEvent);
    }

    [Fact]
    public void Verify_WhenAlreadyVerified_ShouldNotAddDuplicateEvent()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        hrPersonnel.Verify();
        hrPersonnel.ClearDomainEvents();

        // Act
        hrPersonnel.Verify();

        // Assert
        hrPersonnel.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivatePersonnel()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();

        // Act
        hrPersonnel.Deactivate();

        // Assert
        hrPersonnel.IsActive.Should().BeFalse();
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelDeactivatedEvent);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotAddDuplicateEvent()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        hrPersonnel.Deactivate();
        hrPersonnel.ClearDomainEvents();

        // Act
        hrPersonnel.Deactivate();

        // Assert
        hrPersonnel.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivatePersonnel()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        hrPersonnel.Deactivate();
        hrPersonnel.ClearDomainEvents();

        // Act
        hrPersonnel.Activate();

        // Assert
        hrPersonnel.IsActive.Should().BeTrue();
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelActivatedEvent);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateProfile()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var newName = "Jane Smith";
        var newTitle = "Chief HR Officer";
        var newDepartment = "Executive";
        var newEmail = "jane.smith@company.com";
        var newPhone = "+0987654321";
        var newLinkedIn = "https://linkedin.com/in/janesmith";

        // Act
        hrPersonnel.UpdateProfile(newName, newTitle, newDepartment, newEmail, newPhone, newLinkedIn);

        // Assert
        hrPersonnel.FullName.Should().Be(newName);
        hrPersonnel.Title.Should().Be(newTitle);
        hrPersonnel.Department.Should().Be(newDepartment);
        hrPersonnel.Email.Should().Be(newEmail);
        hrPersonnel.PhoneNumber.Should().Be(newPhone);
        hrPersonnel.LinkedInProfile.Should().Be(newLinkedIn);
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelProfileUpdatedEvent);
    }

    [Fact]
    public void UpdateProfile_WithInvalidData_ShouldThrowException()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();

        // Act
        var act = () => hrPersonnel.UpdateProfile("", _title, _department, _email, _phoneNumber, _linkedInProfile);

        // Assert
        act.Should().Throw<HRPersonnelException>()
            .WithMessage("Full name is required");
    }

    [Fact]
    public void RecordJobPosting_ShouldIncrementJobPostingCount()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var initialCount = hrPersonnel.TotalJobPostings;

        // Act
        hrPersonnel.RecordJobPosting();

        // Assert
        hrPersonnel.TotalJobPostings.Should().Be(initialCount + 1);
        hrPersonnel.LastJobPostingAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void RecordSuccessfulHire_ShouldIncrementSuccessfulHires()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var initialCount = hrPersonnel.SuccessfulHires;

        // Act
        hrPersonnel.RecordSuccessfulHire();

        // Assert
        hrPersonnel.SuccessfulHires.Should().Be(initialCount + 1);
    }

    [Fact]
    public void AddNote_WithValidNote_ShouldAddNote()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var note = "Excellent performance in Q1";

        // Act
        hrPersonnel.AddNote(note);

        // Assert
        hrPersonnel.Notes.Should().Contain(note);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddNote_WithInvalidNote_ShouldNotAddNote(string invalidNote)
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var initialNoteCount = hrPersonnel.Notes.Count;

        // Act
        hrPersonnel.AddNote(invalidNote);

        // Assert
        hrPersonnel.Notes.Should().HaveCount(initialNoteCount);
    }

    [Fact]
    public void CalculateHiringRate_WithJobPostings_ShouldCalculateCorrectRate()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        
        // Record some job postings and successful hires
        for (int i = 0; i < 10; i++)
        {
            hrPersonnel.RecordJobPosting();
        }
        
        for (int i = 0; i < 3; i++)
        {
            hrPersonnel.RecordSuccessfulHire();
        }

        // Act
        var hiringRate = hrPersonnel.CalculateHiringRate();

        // Assert
        hiringRate.Should().Be(0.3m); // 3 hires / 10 postings = 0.3
    }

    [Fact]
    public void CalculateHiringRate_WithNoJobPostings_ShouldReturnZero()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();

        // Act
        var hiringRate = hrPersonnel.CalculateHiringRate();

        // Assert
        hiringRate.Should().Be(0);
    }

    [Fact]
    public void GetActiveJobPostingIds_ShouldReturnActiveIds()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var jobPostingIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Use reflection to add job posting IDs
        var propertyInfo = hrPersonnel.GetType().GetProperty("ActiveJobPostingIds");
        var activeJobPostingIds = (List<Guid>)propertyInfo!.GetValue(hrPersonnel)!;
        activeJobPostingIds.AddRange(jobPostingIds);

        // Act
        var result = hrPersonnel.GetActiveJobPostingIds();

        // Assert
        result.Should().BeEquivalentTo(jobPostingIds);
    }

    [Fact]
    public void AssignToJobPosting_ShouldAddJobPostingId()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var jobPostingId = Guid.NewGuid();

        // Act
        hrPersonnel.AssignToJobPosting(jobPostingId);

        // Assert
        hrPersonnel.GetActiveJobPostingIds().Should().Contain(jobPostingId);
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelAssignedToJobPostingEvent);
    }

    [Fact]
    public void UnassignFromJobPosting_WhenAssigned_ShouldRemoveJobPostingId()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var jobPostingId = Guid.NewGuid();
        hrPersonnel.AssignToJobPosting(jobPostingId);
        hrPersonnel.ClearDomainEvents();

        // Act
        hrPersonnel.UnassignFromJobPosting(jobPostingId);

        // Assert
        hrPersonnel.GetActiveJobPostingIds().Should().NotContain(jobPostingId);
        hrPersonnel.DomainEvents.Should().Contain(e => e is HRPersonnelUnassignedFromJobPostingEvent);
    }

    [Fact]
    public void UnassignFromJobPosting_WhenNotAssigned_ShouldNotRaiseEvent()
    {
        // Arrange
        var hrPersonnel = CreateValidHRPersonnel();
        var jobPostingId = Guid.NewGuid();

        // Act
        hrPersonnel.UnassignFromJobPosting(jobPostingId);

        // Assert
        hrPersonnel.DomainEvents.Should().NotContain(e => e is HRPersonnelUnassignedFromJobPostingEvent);
    }

    private HRPersonnel CreateValidHRPersonnel()
    {
        return HRPersonnel.Create(
            _companyId,
            _userId,
            _fullName,
            _title,
            _department,
            _email,
            _phoneNumber,
            _linkedInProfile);
    }
}
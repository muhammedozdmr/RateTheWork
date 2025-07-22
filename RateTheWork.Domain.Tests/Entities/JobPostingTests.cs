using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Domain.Events.JobPosting;
using RateTheWork.Domain.Exceptions.JobPostingException;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.Entities;

public class JobPostingTests : DomainTestBase
{
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _hrPersonnelId = Guid.NewGuid();
    private readonly string _title = "Senior Software Engineer";
    private readonly string _description = "We are looking for a senior software engineer...";
    private readonly string _requirements = "5+ years of experience...";
    private readonly Address _location = new("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
    private readonly DateTime _firstInterviewDate = DateTime.UtcNow.AddDays(14);
    private readonly int _targetApplicationCount = 50;
    private readonly string _hiringProcess = "Phone Screen -> Technical Interview -> HR Interview -> Offer";
    private readonly int _estimatedProcessDays = 30;

    [Fact]
    public void Create_WithValidParameters_ShouldCreateJobPosting()
    {
        // Act
        var jobPosting = JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            _targetApplicationCount,
            _hiringProcess,
            _estimatedProcessDays);

        // Assert
        jobPosting.Should().NotBeNull();
        jobPosting.CompanyId.Should().Be(_companyId);
        jobPosting.HRPersonnelId.Should().Be(_hrPersonnelId);
        jobPosting.Title.Should().Be(_title);
        jobPosting.Description.Should().Be(_description);
        jobPosting.Requirements.Should().Be(_requirements);
        jobPosting.Location.Should().Be(_location);
        jobPosting.FirstInterviewDate.Should().Be(_firstInterviewDate);
        jobPosting.TargetApplicationCount.Should().Be(_targetApplicationCount);
        jobPosting.HiringProcess.Should().Be(_hiringProcess);
        jobPosting.EstimatedProcessDays.Should().Be(_estimatedProcessDays);
        jobPosting.Status.Should().Be(JobPostingStatus.Draft);
        jobPosting.DomainEvents.Should().ContainSingle(e => e is JobPostingCreatedEvent);
    }

    [Fact]
    public void Create_WithPastInterviewDate_ShouldThrowException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            pastDate,
            _targetApplicationCount,
            _hiringProcess,
            _estimatedProcessDays);

        // Assert
        act.Should().Throw<JobPostingException>()
            .WithMessage("First interview date must be in the future");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetApplicationCount_ShouldThrowException(int invalidCount)
    {
        // Act
        var act = () => JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            invalidCount,
            _hiringProcess,
            _estimatedProcessDays);

        // Assert
        act.Should().Throw<JobPostingException>()
            .WithMessage("Target application count must be greater than zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidEstimatedProcessDays_ShouldThrowException(int invalidDays)
    {
        // Act
        var act = () => JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            _targetApplicationCount,
            _hiringProcess,
            invalidDays);

        // Assert
        act.Should().Throw<JobPostingException>()
            .WithMessage("Estimated process days must be greater than zero");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidTitle_ShouldThrowException(string invalidTitle)
    {
        // Act
        var act = () => JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            invalidTitle,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            _targetApplicationCount,
            _hiringProcess,
            _estimatedProcessDays);

        // Assert
        act.Should().Throw<JobPostingException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public void Publish_WhenDraft_ShouldPublishJobPosting()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();

        // Act
        jobPosting.Publish();

        // Assert
        jobPosting.Status.Should().Be(JobPostingStatus.Published);
        jobPosting.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingPublishedEvent);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldNotAddDuplicateEvent()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        jobPosting.ClearDomainEvents();

        // Act
        jobPosting.Publish();

        // Assert
        jobPosting.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Close_WhenPublished_ShouldCloseJobPosting()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        var reason = "Position filled";

        // Act
        jobPosting.Close(reason);

        // Assert
        jobPosting.Status.Should().Be(JobPostingStatus.Closed);
        jobPosting.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        jobPosting.ClosureReason.Should().Be(reason);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingClosedEvent);
    }

    [Fact]
    public void Suspend_WhenPublished_ShouldSuspendJobPosting()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        var reason = "Temporarily pausing recruitment";

        // Act
        jobPosting.Suspend(reason);

        // Assert
        jobPosting.Status.Should().Be(JobPostingStatus.Suspended);
        jobPosting.SuspendedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        jobPosting.SuspensionReason.Should().Be(reason);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingSuspendedEvent);
    }

    [Fact]
    public void Resume_WhenSuspended_ShouldResumeJobPosting()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        jobPosting.Suspend("Temporary pause");

        // Act
        jobPosting.Resume();

        // Assert
        jobPosting.Status.Should().Be(JobPostingStatus.Published);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingResumedEvent);
    }

    [Fact]
    public void UpdateSalaryRange_WithValidRange_ShouldUpdateSalaryRange()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var salaryRange = new SalaryRange(80000, 120000, "USD", SalaryPeriod.Yearly);

        // Act
        jobPosting.UpdateSalaryRange(salaryRange);

        // Assert
        jobPosting.SalaryRange.Should().Be(salaryRange);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingUpdatedEvent);
    }

    [Fact]
    public void UpdateTransparencyInfo_WithValidInfo_ShouldUpdateInfo()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var newFirstInterviewDate = DateTime.UtcNow.AddDays(20);
        var newTargetApplicationCount = 100;
        var newHiringProcess = "Updated hiring process";
        var newEstimatedDays = 45;

        // Act
        jobPosting.UpdateTransparencyInfo(
            newFirstInterviewDate,
            newTargetApplicationCount,
            newHiringProcess,
            newEstimatedDays);

        // Assert
        jobPosting.FirstInterviewDate.Should().Be(newFirstInterviewDate);
        jobPosting.TargetApplicationCount.Should().Be(newTargetApplicationCount);
        jobPosting.HiringProcess.Should().Be(newHiringProcess);
        jobPosting.EstimatedProcessDays.Should().Be(newEstimatedDays);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingTransparencyUpdatedEvent);
    }

    [Fact]
    public void AddTag_WithValidTag_ShouldAddTag()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var tag = "remote";

        // Act
        jobPosting.AddTag(tag);

        // Assert
        jobPosting.Tags.Should().Contain(tag);
    }

    [Fact]
    public void AddTag_WithDuplicateTag_ShouldNotAddDuplicate()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var tag = "remote";
        jobPosting.AddTag(tag);

        // Act
        jobPosting.AddTag(tag);

        // Assert
        jobPosting.Tags.Count(t => t == tag).Should().Be(1);
    }

    [Fact]
    public void RemoveTag_WhenTagExists_ShouldRemoveTag()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var tag = "remote";
        jobPosting.AddTag(tag);

        // Act
        jobPosting.RemoveTag(tag);

        // Assert
        jobPosting.Tags.Should().NotContain(tag);
    }

    [Fact]
    public void RecordView_ShouldIncrementViewCount()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        var initialViews = jobPosting.ViewCount;

        // Act
        jobPosting.RecordView();

        // Assert
        jobPosting.ViewCount.Should().Be(initialViews + 1);
    }

    [Fact]
    public void RecordView_WhenNotPublished_ShouldNotIncrement()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var initialViews = jobPosting.ViewCount;

        // Act
        jobPosting.RecordView();

        // Assert
        jobPosting.ViewCount.Should().Be(initialViews);
    }

    [Fact]
    public void RecordApplication_ShouldIncrementApplicationCount()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();
        var initialApplications = jobPosting.ApplicationCount;

        // Act
        jobPosting.RecordApplication();

        // Assert
        jobPosting.ApplicationCount.Should().Be(initialApplications + 1);
    }

    [Fact]
    public void RecordApplication_WhenTargetReached_ShouldRaiseEvent()
    {
        // Arrange
        var targetCount = 5;
        var jobPosting = JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            targetCount,
            _hiringProcess,
            _estimatedProcessDays);
        
        jobPosting.Publish();

        // Record applications up to target
        for (int i = 0; i < targetCount - 1; i++)
        {
            jobPosting.RecordApplication();
        }
        jobPosting.ClearDomainEvents();

        // Act
        jobPosting.RecordApplication();

        // Assert
        jobPosting.ApplicationCount.Should().Be(targetCount);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingTargetReachedEvent);
    }

    [Fact]
    public void UpdateDetails_WithValidDetails_ShouldUpdateDetails()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newRequirements = "Updated Requirements";
        var newLocation = new Address("USA", "California", "San Francisco", "Market Street", "94105");

        // Act
        jobPosting.UpdateDetails(newTitle, newDescription, newRequirements, newLocation);

        // Assert
        jobPosting.Title.Should().Be(newTitle);
        jobPosting.Description.Should().Be(newDescription);
        jobPosting.Requirements.Should().Be(newRequirements);
        jobPosting.Location.Should().Be(newLocation);
        jobPosting.DomainEvents.Should().Contain(e => e is JobPostingUpdatedEvent);
    }

    [Fact]
    public void UpdateDetails_WhenPublished_ShouldThrowException()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        jobPosting.Publish();

        // Act
        var act = () => jobPosting.UpdateDetails("New Title", "New Desc", "New Req", _location);

        // Assert
        act.Should().Throw<JobPostingException>()
            .WithMessage("Cannot update details of a published job posting");
    }

    [Fact]
    public void SetEmploymentType_WithValidType_ShouldSetEmploymentType()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var employmentType = EmploymentType.FullTime;

        // Act
        jobPosting.SetEmploymentType(employmentType);

        // Assert
        jobPosting.EmploymentType.Should().Be(employmentType);
    }

    [Fact]
    public void SetWorkLocation_WithValidLocation_ShouldSetWorkLocation()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var workLocation = WorkLocation.Hybrid;

        // Act
        jobPosting.SetWorkLocation(workLocation);

        // Assert
        jobPosting.WorkLocation.Should().Be(workLocation);
    }

    [Fact]
    public void SetExperienceLevel_WithValidLevel_ShouldSetExperienceLevel()
    {
        // Arrange
        var jobPosting = CreateValidJobPosting();
        var experienceLevel = ExperienceLevel.Senior;

        // Act
        jobPosting.SetExperienceLevel(experienceLevel);

        // Assert
        jobPosting.ExperienceLevel.Should().Be(experienceLevel);
    }

    private JobPosting CreateValidJobPosting()
    {
        return JobPosting.Create(
            _companyId,
            _hrPersonnelId,
            _title,
            _description,
            _requirements,
            _location,
            _firstInterviewDate,
            _targetApplicationCount,
            _hiringProcess,
            _estimatedProcessDays);
    }
}
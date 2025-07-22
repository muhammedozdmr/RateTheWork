using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobApplication;
using RateTheWork.Domain.Events.JobApplication;
using RateTheWork.Domain.Exceptions.JobApplicationException;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Entities;

public class JobApplicationTests : DomainTestBase
{
    private readonly Guid _jobPostingId = Guid.NewGuid();
    private readonly Guid _applicantUserId = Guid.NewGuid();
    private readonly string _coverLetter = "I am excited to apply for this position...";
    private readonly string _resumeUrl = "https://example.com/resume.pdf";

    [Fact]
    public void Create_WithValidParameters_ShouldCreateJobApplication()
    {
        // Act
        var application = JobApplication.Create(
            _jobPostingId,
            _applicantUserId,
            _coverLetter,
            _resumeUrl);

        // Assert
        application.Should().NotBeNull();
        application.JobPostingId.Should().Be(_jobPostingId);
        application.ApplicantUserId.Should().Be(_applicantUserId);
        application.CoverLetter.Should().Be(_coverLetter);
        application.ResumeUrl.Should().Be(_resumeUrl);
        application.Status.Should().Be(ApplicationStatus.Submitted);
        application.ApplicationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.DomainEvents.Should().ContainSingle(e => e is JobApplicationSubmittedEvent);
    }

    [Fact]
    public void Create_WithEmptyJobPostingId_ShouldThrowException()
    {
        // Act
        var act = () => JobApplication.Create(
            Guid.Empty,
            _applicantUserId,
            _coverLetter,
            _resumeUrl);

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Job posting ID is required");
    }

    [Fact]
    public void Create_WithEmptyApplicantUserId_ShouldThrowException()
    {
        // Act
        var act = () => JobApplication.Create(
            _jobPostingId,
            Guid.Empty,
            _coverLetter,
            _resumeUrl);

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Applicant user ID is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidResumeUrl_ShouldThrowException(string invalidUrl)
    {
        // Act
        var act = () => JobApplication.Create(
            _jobPostingId,
            _applicantUserId,
            _coverLetter,
            invalidUrl);

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Resume URL is required");
    }

    [Fact]
    public void Create_WithoutCoverLetter_ShouldCreateApplication()
    {
        // Act
        var application = JobApplication.Create(
            _jobPostingId,
            _applicantUserId,
            null,
            _resumeUrl);

        // Assert
        application.Should().NotBeNull();
        application.CoverLetter.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_WithValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var application = CreateValidApplication();
        var newStatus = ApplicationStatus.Reviewing;
        var notes = "Initial review completed";

        // Act
        application.UpdateStatus(newStatus, notes);

        // Assert
        application.Status.Should().Be(newStatus);
        application.StatusUpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.StatusHistory.Should().HaveCount(1);
        application.StatusHistory.First().Status.Should().Be(newStatus);
        application.StatusHistory.First().Notes.Should().Be(notes);
        application.DomainEvents.Should().Contain(e => e is JobApplicationStatusUpdatedEvent);
    }

    [Fact]
    public void UpdateStatus_ToSameStatus_ShouldNotAddEvent()
    {
        // Arrange
        var application = CreateValidApplication();
        application.ClearDomainEvents();

        // Act
        application.UpdateStatus(ApplicationStatus.Submitted, "Same status");

        // Assert
        application.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ScheduleInterview_WithValidData_ShouldScheduleInterview()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        var interviewDate = DateTime.UtcNow.AddDays(7);
        var interviewType = "Technical Interview";
        var interviewerName = "John Smith";
        var location = "Conference Room A";

        // Act
        application.ScheduleInterview(interviewDate, interviewType, interviewerName, location);

        // Assert
        application.Status.Should().Be(ApplicationStatus.InterviewScheduled);
        application.InterviewDate.Should().Be(interviewDate);
        application.InterviewType.Should().Be(interviewType);
        application.InterviewerName.Should().Be(interviewerName);
        application.InterviewLocation.Should().Be(location);
        application.DomainEvents.Should().Contain(e => e is JobApplicationInterviewScheduledEvent);
    }

    [Fact]
    public void ScheduleInterview_WithPastDate_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => application.ScheduleInterview(pastDate, "Interview", "Interviewer", "Location");

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Interview date must be in the future");
    }

    [Fact]
    public void ScheduleInterview_WhenNotInReviewingStatus_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();

        // Act
        var act = () => application.ScheduleInterview(
            DateTime.UtcNow.AddDays(7),
            "Interview",
            "Interviewer",
            "Location");

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Can only schedule interview when application is in reviewing status");
    }

    [Fact]
    public void CompleteInterview_WithValidFeedback_ShouldCompleteInterview()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        application.ScheduleInterview(
            DateTime.UtcNow.AddDays(7),
            "Technical",
            "John Smith",
            "Room A");
        
        var feedback = "Excellent technical skills";
        var rating = 4;

        // Act
        application.CompleteInterview(feedback, rating);

        // Assert
        application.Status.Should().Be(ApplicationStatus.InterviewCompleted);
        application.InterviewFeedback.Should().Be(feedback);
        application.InterviewRating.Should().Be(rating);
        application.DomainEvents.Should().Contain(e => e is JobApplicationInterviewCompletedEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CompleteInterview_WithInvalidRating_ShouldThrowException(int invalidRating)
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        application.ScheduleInterview(
            DateTime.UtcNow.AddDays(7),
            "Technical",
            "John Smith",
            "Room A");

        // Act
        var act = () => application.CompleteInterview("Feedback", invalidRating);

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Interview rating must be between 1 and 5");
    }

    [Fact]
    public void CompleteInterview_WhenNotScheduled_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();

        // Act
        var act = () => application.CompleteInterview("Feedback", 4);

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("No interview scheduled for this application");
    }

    [Fact]
    public void MakeOffer_WithValidData_ShouldMakeOffer()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        application.ScheduleInterview(DateTime.UtcNow.AddDays(7), "Technical", "John", "Room");
        application.CompleteInterview("Good", 4);
        
        var salary = 100000m;
        var startDate = DateTime.UtcNow.AddMonths(1);
        var offerDetails = "Full benefits package included";

        // Act
        application.MakeOffer(salary, startDate, offerDetails);

        // Assert
        application.Status.Should().Be(ApplicationStatus.OfferMade);
        application.OfferedSalary.Should().Be(salary);
        application.OfferDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.ProposedStartDate.Should().Be(startDate);
        application.OfferDetails.Should().Be(offerDetails);
        application.DomainEvents.Should().Contain(e => e is JobApplicationOfferMadeEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MakeOffer_WithInvalidSalary_ShouldThrowException(decimal invalidSalary)
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.InterviewCompleted, null);

        // Act
        var act = () => application.MakeOffer(invalidSalary, DateTime.UtcNow.AddMonths(1), "Details");

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Offered salary must be greater than zero");
    }

    [Fact]
    public void MakeOffer_WithPastStartDate_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.InterviewCompleted, null);

        // Act
        var act = () => application.MakeOffer(100000, DateTime.UtcNow.AddDays(-1), "Details");

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Proposed start date must be in the future");
    }

    [Fact]
    public void AcceptOffer_WhenOfferMade_ShouldAcceptOffer()
    {
        // Arrange
        var application = CreateValidApplication();
        PrepareApplicationForOffer(application);
        application.MakeOffer(100000, DateTime.UtcNow.AddMonths(1), "Details");

        // Act
        application.AcceptOffer();

        // Assert
        application.Status.Should().Be(ApplicationStatus.OfferAccepted);
        application.OfferAcceptedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.DomainEvents.Should().Contain(e => e is JobApplicationOfferAcceptedEvent);
    }

    [Fact]
    public void AcceptOffer_WhenNoOfferMade_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();

        // Act
        var act = () => application.AcceptOffer();

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("No offer has been made for this application");
    }

    [Fact]
    public void RejectOffer_WhenOfferMade_ShouldRejectOffer()
    {
        // Arrange
        var application = CreateValidApplication();
        PrepareApplicationForOffer(application);
        application.MakeOffer(100000, DateTime.UtcNow.AddMonths(1), "Details");
        var reason = "Better offer elsewhere";

        // Act
        application.RejectOffer(reason);

        // Assert
        application.Status.Should().Be(ApplicationStatus.OfferRejected);
        application.OfferRejectedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.OfferRejectionReason.Should().Be(reason);
        application.DomainEvents.Should().Contain(e => e is JobApplicationOfferRejectedEvent);
    }

    [Fact]
    public void Withdraw_WhenNotFinalized_ShouldWithdrawApplication()
    {
        // Arrange
        var application = CreateValidApplication();
        var reason = "Found another opportunity";

        // Act
        application.Withdraw(reason);

        // Assert
        application.Status.Should().Be(ApplicationStatus.Withdrawn);
        application.WithdrawnAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.WithdrawalReason.Should().Be(reason);
        application.DomainEvents.Should().Contain(e => e is JobApplicationWithdrawnEvent);
    }

    [Fact]
    public void Withdraw_WhenAlreadyAccepted_ShouldThrowException()
    {
        // Arrange
        var application = CreateValidApplication();
        PrepareApplicationForOffer(application);
        application.MakeOffer(100000, DateTime.UtcNow.AddMonths(1), "Details");
        application.AcceptOffer();

        // Act
        var act = () => application.Withdraw("Reason");

        // Assert
        act.Should().Throw<JobApplicationException>()
            .WithMessage("Cannot withdraw an application that has been accepted");
    }

    [Fact]
    public void Reject_WithReason_ShouldRejectApplication()
    {
        // Arrange
        var application = CreateValidApplication();
        var reason = "Not enough experience";

        // Act
        application.Reject(reason);

        // Assert
        application.Status.Should().Be(ApplicationStatus.Rejected);
        application.RejectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        application.RejectionReason.Should().Be(reason);
        application.DomainEvents.Should().Contain(e => e is JobApplicationRejectedEvent);
    }

    [Fact]
    public void AddNote_WithValidNote_ShouldAddNote()
    {
        // Arrange
        var application = CreateValidApplication();
        var note = "Strong candidate, proceed to next round";

        // Act
        application.AddNote(note);

        // Assert
        application.Notes.Should().Contain(note);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddNote_WithInvalidNote_ShouldNotAddNote(string invalidNote)
    {
        // Arrange
        var application = CreateValidApplication();
        var initialNoteCount = application.Notes.Count;

        // Act
        application.AddNote(invalidNote);

        // Assert
        application.Notes.Should().HaveCount(initialNoteCount);
    }

    [Fact]
    public void GetStatusHistory_ShouldReturnImmutableHistory()
    {
        // Arrange
        var application = CreateValidApplication();
        application.UpdateStatus(ApplicationStatus.Reviewing, "Review started");
        
        // Act
        var history = application.GetStatusHistory();
        
        // Assert
        history.Should().HaveCount(1);
        history.Should().BeOfType<List<ApplicationStatusHistory>>();
        
        // Verify it's a copy by trying to modify it
        history.Clear();
        application.GetStatusHistory().Should().HaveCount(1);
    }

    private JobApplication CreateValidApplication()
    {
        return JobApplication.Create(
            _jobPostingId,
            _applicantUserId,
            _coverLetter,
            _resumeUrl);
    }

    private void PrepareApplicationForOffer(JobApplication application)
    {
        application.UpdateStatus(ApplicationStatus.Reviewing, null);
        application.ScheduleInterview(DateTime.UtcNow.AddDays(7), "Technical", "John", "Room");
        application.CompleteInterview("Good", 4);
    }
}
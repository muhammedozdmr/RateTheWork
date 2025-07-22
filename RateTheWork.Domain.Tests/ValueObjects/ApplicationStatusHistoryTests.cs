using FluentAssertions;
using RateTheWork.Domain.Enums.JobApplication;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class ApplicationStatusHistoryTests : DomainTestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateHistory()
    {
        // Arrange
        var status = ApplicationStatus.Reviewing;
        var changedAt = DateTime.UtcNow;
        var changedBy = "HR Manager";
        var notes = "Initial review started";

        // Act
        var history = new ApplicationStatusHistory(status, changedAt, changedBy, notes);

        // Assert
        history.Should().NotBeNull();
        history.Status.Should().Be(status);
        history.ChangedAt.Should().Be(changedAt);
        history.ChangedBy.Should().Be(changedBy);
        history.Notes.Should().Be(notes);
    }

    [Fact]
    public void Constructor_WithoutNotes_ShouldCreateHistory()
    {
        // Arrange
        var status = ApplicationStatus.InterviewScheduled;
        var changedAt = DateTime.UtcNow;
        var changedBy = "System";

        // Act
        var history = new ApplicationStatusHistory(status, changedAt, changedBy, null);

        // Assert
        history.Should().NotBeNull();
        history.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidChangedBy_ShouldThrowException(string invalidChangedBy)
    {
        // Act
        var act = () => new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            invalidChangedBy,
            "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ChangedBy is required");
    }

    [Fact]
    public void Constructor_WithFutureDate_ShouldThrowException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            futureDate,
            "HR Manager",
            "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ChangedAt cannot be in the future");
    }

    [Fact]
    public void Constructor_WithMinDate_ShouldThrowException()
    {
        // Act
        var act = () => new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.MinValue,
            "HR Manager",
            "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ChangedAt must be a valid date");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var changedAt = DateTime.UtcNow;
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");
        var history2 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");

        // Act & Assert
        history1.Should().Be(history2);
        history1.Equals(history2).Should().BeTrue();
        (history1 == history2).Should().BeTrue();
        (history1 != history2).Should().BeFalse();
    }

    [Theory]
    [InlineData(ApplicationStatus.InterviewScheduled, "HR Manager", "Review notes")] // Different status
    [InlineData(ApplicationStatus.Reviewing, "Recruiter", "Review notes")] // Different changedBy
    [InlineData(ApplicationStatus.Reviewing, "HR Manager", "Different notes")] // Different notes
    public void Equals_WithDifferentValues_ShouldReturnFalse(
        ApplicationStatus status, string changedBy, string notes)
    {
        // Arrange
        var changedAt = DateTime.UtcNow;
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");
        var history2 = new ApplicationStatusHistory(status, changedAt, changedBy, notes);

        // Act & Assert
        history1.Should().NotBe(history2);
        history1.Equals(history2).Should().BeFalse();
        (history1 == history2).Should().BeFalse();
        (history1 != history2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentDates_ShouldReturnFalse()
    {
        // Arrange
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            "HR Manager",
            "Review notes");
        var history2 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow.AddSeconds(-1),
            "HR Manager",
            "Review notes");

        // Act & Assert
        history1.Should().NotBe(history2);
    }

    [Fact]
    public void Equals_WithBothNullNotes_ShouldReturnTrue()
    {
        // Arrange
        var changedAt = DateTime.UtcNow;
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            null);
        var history2 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            null);

        // Act & Assert
        history1.Should().Be(history2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var history = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            "HR Manager",
            "Notes");

        // Act & Assert
        history.Equals(null).Should().BeFalse();
        (history == null).Should().BeFalse();
        (null == history).Should().BeFalse();
        (history != null).Should().BeTrue();
        (null != history).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var changedAt = DateTime.UtcNow;
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");
        var history2 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");

        // Act & Assert
        history1.GetHashCode().Should().Be(history2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var history1 = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            "HR Manager",
            "Review notes");
        var history2 = new ApplicationStatusHistory(
            ApplicationStatus.Rejected,
            DateTime.UtcNow.AddHours(-1),
            "Recruiter",
            "Not qualified");

        // Act & Assert
        history1.GetHashCode().Should().NotBe(history2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var changedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var history = new ApplicationStatusHistory(
            ApplicationStatus.InterviewScheduled,
            changedAt,
            "John Doe",
            "Technical interview scheduled");

        // Act
        var result = history.ToString();

        // Assert
        result.Should().Be("InterviewScheduled at 2024-01-15 10:30:00 by John Doe");
    }

    [Fact]
    public void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var history = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            "HR Manager",
            "Notes");

        // Act & Assert
        // Properties should not have setters
        history.GetType().GetProperty(nameof(ApplicationStatusHistory.Status))!.CanWrite.Should().BeFalse();
        history.GetType().GetProperty(nameof(ApplicationStatusHistory.ChangedAt))!.CanWrite.Should().BeFalse();
        history.GetType().GetProperty(nameof(ApplicationStatusHistory.ChangedBy))!.CanWrite.Should().BeFalse();
        history.GetType().GetProperty(nameof(ApplicationStatusHistory.Notes))!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void GetComponents_ShouldReturnProtectedList()
    {
        // Arrange
        var changedAt = DateTime.UtcNow;
        var history = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");

        // Act
        var components = history.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(4);
        components.Should().ContainInOrder(
            ApplicationStatus.Reviewing,
            changedAt,
            "HR Manager",
            "Review notes");
    }

    [Theory]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Reviewing)]
    [InlineData(ApplicationStatus.InterviewScheduled)]
    [InlineData(ApplicationStatus.InterviewCompleted)]
    [InlineData(ApplicationStatus.OfferMade)]
    [InlineData(ApplicationStatus.OfferAccepted)]
    [InlineData(ApplicationStatus.OfferRejected)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void Constructor_WithAllStatuses_ShouldCreateValidHistory(ApplicationStatus status)
    {
        // Act
        var history = new ApplicationStatusHistory(
            status,
            DateTime.UtcNow,
            "Test User",
            $"Changed to {status}");

        // Assert
        history.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("System")]
    [InlineData("hr@company.com")]
    [InlineData("Automated Process")]
    [InlineData("John Doe (HR Manager)")]
    public void Constructor_WithVariousChangedByFormats_ShouldCreateValidHistory(string changedBy)
    {
        // Act
        var history = new ApplicationStatusHistory(
            ApplicationStatus.Reviewing,
            DateTime.UtcNow,
            changedBy,
            "Notes");

        // Assert
        history.ChangedBy.Should().Be(changedBy);
    }
}
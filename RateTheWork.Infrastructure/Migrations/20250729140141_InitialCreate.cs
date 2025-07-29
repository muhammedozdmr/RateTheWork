using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateTheWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AdminUserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    AppealNotes = table.Column<string>(type: "text", nullable: true),
                    AppealedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LiftedBy = table.Column<string>(type: "text", nullable: true),
                    LiftedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LiftReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MersisNo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CompanyType = table.Column<int>(type: "integer", nullable: false),
                    EstablishedYear = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivationReason = table.Column<string>(type: "text", nullable: true),
                    Sector = table.Column<int>(type: "integer", nullable: false),
                    SubSector = table.Column<string>(type: "text", nullable: true),
                    CompanySize = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: true),
                    EmployeeCountRange = table.Column<string>(type: "text", nullable: true),
                    EmployeeCountUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AverageEmployeeTenure = table.Column<decimal>(type: "numeric", nullable: true),
                    RevenueRange = table.Column<string>(type: "text", nullable: true),
                    FundingStage = table.Column<string>(type: "text", nullable: true),
                    MarketCap = table.Column<decimal>(type: "numeric", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    FaxNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HrEmail = table.Column<string>(type: "text", nullable: true),
                    SupportEmail = table.Column<string>(type: "text", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: false),
                    CareersPageUrl = table.Column<string>(type: "text", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "text", nullable: true),
                    XUrl = table.Column<string>(type: "text", nullable: true),
                    InstagramUrl = table.Column<string>(type: "text", nullable: true),
                    FacebookUrl = table.Column<string>(type: "text", nullable: true),
                    YouTubeUrl = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    GalleryImages = table.Column<string>(type: "jsonb", nullable: false),
                    CompanyVideo = table.Column<string>(type: "text", nullable: true),
                    WorkingHours = table.Column<string>(type: "jsonb", nullable: true),
                    HasRemoteWork = table.Column<bool>(type: "boolean", nullable: true),
                    RemoteWorkPolicy = table.Column<string>(type: "text", nullable: true),
                    Benefits = table.Column<string>(type: "jsonb", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    RatingBreakdown = table.Column<string>(type: "jsonb", nullable: false),
                    ReviewCountByType = table.Column<string>(type: "jsonb", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "text", nullable: true),
                    VerificationMethod = table.Column<string>(type: "text", nullable: true),
                    VerificationNotes = table.Column<string>(type: "text", nullable: true),
                    VerificationMetadata = table.Column<string>(type: "jsonb", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric", nullable: true),
                    RiskScoreUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskLevel = table.Column<string>(type: "text", nullable: true),
                    ComplianceCertificates = table.Column<string>(type: "jsonb", nullable: false),
                    IsMerged = table.Column<bool>(type: "boolean", nullable: false),
                    MergedWithCompanyId = table.Column<string>(type: "text", nullable: true),
                    MergedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParentCompanyId = table.Column<string>(type: "text", nullable: true),
                    SubsidiaryIds = table.Column<string>(type: "jsonb", nullable: false),
                    BranchCount = table.Column<int>(type: "integer", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    RejectedBy = table.Column<string>(type: "text", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "TRY"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsTrialPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    TrialEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMethodId = table.Column<string>(type: "text", nullable: true),
                    BillingContactId = table.Column<string>(type: "text", nullable: true),
                    Features = table.Column<string>(type: "jsonb", nullable: false),
                    UsageLimits = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentUsage = table.Column<string>(type: "jsonb", nullable: false),
                    AuthorizedHRPersonnelIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    InvoiceAddress = table.Column<string>(type: "text", nullable: true),
                    TaxNumber = table.Column<string>(type: "text", nullable: true),
                    TaxOffice = table.Column<string>(type: "text", nullable: true),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HRPersonnel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    LinkedInProfile = table.Column<string>(type: "text", nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationDocumentUrl = table.Column<string>(type: "text", nullable: true),
                    VerifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CanPostJobs = table.Column<bool>(type: "boolean", nullable: false),
                    CanRespondToReviews = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivationReason = table.Column<string>(type: "text", nullable: true),
                    TotalJobsPosted = table.Column<int>(type: "integer", nullable: false),
                    ActiveJobsCount = table.Column<int>(type: "integer", nullable: false),
                    TotalResponses = table.Column<int>(type: "integer", nullable: false),
                    ResponseRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageResponseTime = table.Column<decimal>(type: "numeric", nullable: false),
                    TrustScore = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalApplicationsReceived = table.Column<int>(type: "integer", nullable: false),
                    TotalHiresMade = table.Column<int>(type: "integer", nullable: false),
                    HiringSuccessRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageTimeToHire = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRPersonnel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    JobPostingId = table.Column<string>(type: "text", nullable: false),
                    ApplicantUserId = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: false),
                    ResumeUrl = table.Column<string>(type: "text", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "text", nullable: true),
                    LinkedInProfile = table.Column<string>(type: "text", nullable: true),
                    AdditionalDocuments = table.Column<List<string>>(type: "text[]", nullable: false),
                    ApplicantName = table.Column<string>(type: "text", nullable: false),
                    ApplicantEmail = table.Column<string>(type: "text", nullable: false),
                    ApplicantPhone = table.Column<string>(type: "text", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    InterviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InterviewNotes = table.Column<string>(type: "text", nullable: true),
                    InterviewScore = table.Column<int>(type: "integer", nullable: true),
                    OfferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OfferedSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    ResponseDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusHistory = table.Column<string>(type: "jsonb", nullable: false),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    SkillAssessments = table.Column<string>(type: "jsonb", nullable: false),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "text", nullable: true),
                    RelatedEntityId = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    Channels = table.Column<int>(type: "integer", nullable: false),
                    IsEmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    IsSmsSent = table.Column<bool>(type: "boolean", nullable: false),
                    IsPushSent = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ReportedEntityType = table.Column<string>(type: "text", nullable: false),
                    ReportedEntityId = table.Column<string>(type: "text", nullable: false),
                    ReporterUserId = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewerUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewerNotes = table.Column<string>(type: "text", nullable: true),
                    Resolution = table.Column<int>(type: "integer", nullable: true),
                    EvidenceUrls = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CommentType = table.Column<int>(type: "integer", nullable: false),
                    OverallRating = table.Column<decimal>(type: "numeric", nullable: false),
                    CommentText = table.Column<string>(type: "text", nullable: false),
                    DocumentUrl = table.Column<string>(type: "text", nullable: true),
                    IsDocumentVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Upvotes = table.Column<int>(type: "integer", nullable: false),
                    Downvotes = table.Column<int>(type: "integer", nullable: false),
                    ReportCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    LastEditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EditReason = table.Column<string>(type: "text", nullable: true),
                    EditCount = table.Column<int>(type: "integer", nullable: false),
                    HelpfulnessScore = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetType = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewVotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ReviewId = table.Column<string>(type: "text", nullable: false),
                    IsUpvote = table.Column<bool>(type: "boolean", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    IsVerifiedUser = table.Column<bool>(type: "boolean", nullable: false),
                    UserReputationAtTime = table.Column<int>(type: "integer", nullable: false),
                    WasChanged = table.Column<bool>(type: "boolean", nullable: false),
                    LastChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChangeCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetType = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    VoteType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewVotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "TRY"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsTrialPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    TrialEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMethodId = table.Column<string>(type: "text", nullable: true),
                    Features = table.Column<string>(type: "jsonb", nullable: false),
                    UsageLimits = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentUsage = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    BadgeId = table.Column<string>(type: "text", nullable: true),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AwardReason = table.Column<string>(type: "text", nullable: true),
                    IsDisplayed = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AnonymousUsername = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EncryptedFirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EncryptedLastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Profession = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedTcIdentityNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EncryptedAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EncryptedCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedPhoneNumber = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncryptedBirthDate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneVerificationCode = table.Column<string>(type: "text", nullable: true),
                    PhoneVerificationCodeExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsTcIdentityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    TcIdentityVerificationDocumentUrl = table.Column<string>(type: "text", nullable: true),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailHash = table.Column<string>(type: "text", nullable: true),
                    TcIdentityHash = table.Column<string>(type: "text", nullable: true),
                    Roles = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AdminUserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "text", nullable: true),
                    RelatedEntityId = table.Column<string>(type: "text", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyBranches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    District = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: true),
                    IsHeadquarters = table.Column<bool>(type: "boolean", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    BranchType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OpenPositionCount = table.Column<int>(type: "integer", nullable: false),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: false),
                    ManagerId = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    CompanyId1 = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyId1",
                        column: x => x.CompanyId1,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    HRPersonnelId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Requirements = table.Column<string>(type: "text", nullable: false),
                    Responsibilities = table.Column<string>(type: "text", nullable: false),
                    JobType = table.Column<int>(type: "integer", nullable: false),
                    ExperienceLevel = table.Column<int>(type: "integer", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    IsRemoteAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    MinSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ShowSalary = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetApplicationCount = table.Column<int>(type: "integer", nullable: false),
                    FirstInterviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedHiringDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HiringProcess = table.Column<string>(type: "text", nullable: false),
                    EstimatedProcessDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    ApplicationCount = table.Column<int>(type: "integer", nullable: false),
                    ShortlistedCount = table.Column<int>(type: "integer", nullable: false),
                    InterviewedCount = table.Column<int>(type: "integer", nullable: false),
                    OfferedCount = table.Column<int>(type: "integer", nullable: false),
                    HiredCount = table.Column<int>(type: "integer", nullable: false),
                    Skills = table.Column<string>(type: "jsonb", nullable: false),
                    Benefits = table.Column<string>(type: "jsonb", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractorReviews",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ProjectDescription = table.Column<string>(type: "text", nullable: false),
                    ProjectBudget = table.Column<decimal>(type: "numeric", nullable: false),
                    ProjectDuration = table.Column<string>(type: "text", nullable: false),
                    ProjectStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentTimelinessRating = table.Column<decimal>(type: "numeric", nullable: false),
                    CommunicationRating = table.Column<decimal>(type: "numeric", nullable: false),
                    ProjectManagementRating = table.Column<decimal>(type: "numeric", nullable: false),
                    TechnicalCompetenceRating = table.Column<decimal>(type: "numeric", nullable: false),
                    OverallRating = table.Column<decimal>(type: "numeric", nullable: false),
                    ReviewText = table.Column<string>(type: "text", nullable: false),
                    WouldWorkAgain = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationDocumentUrl = table.Column<string>(type: "text", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Upvotes = table.Column<int>(type: "integer", nullable: false),
                    Downvotes = table.Column<int>(type: "integer", nullable: false),
                    HelpfulnessScore = table.Column<decimal>(type: "numeric", nullable: false),
                    CompanyId1 = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractorReviews_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractorReviews_Companies_CompanyId1",
                        column: x => x.CompanyId1,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContractorReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CVApplications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    DepartmentIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    ApplicantName = table.Column<string>(type: "text", nullable: false),
                    ApplicantEmail = table.Column<string>(type: "text", nullable: false),
                    ApplicantPhone = table.Column<string>(type: "text", nullable: false),
                    ApplicantWebsite = table.Column<string>(type: "text", nullable: true),
                    CVFileUrl = table.Column<string>(type: "text", nullable: false),
                    MotivationLetterUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseMessage = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeedbackDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleteReason = table.Column<string>(type: "text", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "jsonb", nullable: false),
                    CompanyId1 = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVApplications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CVApplications_Companies_CompanyId1",
                        column: x => x.CompanyId1,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CVApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobAlerts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Keywords = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    JobType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CVApplicationDepartment",
                columns: table => new
                {
                    CVApplicationsId = table.Column<string>(type: "text", nullable: false),
                    DepartmentsId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVApplicationDepartment", x => new { x.CVApplicationsId, x.DepartmentsId });
                    table.ForeignKey(
                        name: "FK_CVApplicationDepartment_CVApplications_CVApplicationsId",
                        column: x => x.CVApplicationsId,
                        principalTable: "CVApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CVApplicationDepartment_Departments_DepartmentsId",
                        column: x => x.DepartmentsId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CompanyId",
                table: "CompanyBranches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorReviews_CompanyId",
                table: "ContractorReviews",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorReviews_CompanyId1",
                table: "ContractorReviews",
                column: "CompanyId1");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorReviews_UserId",
                table: "ContractorReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CVApplicationDepartment_DepartmentsId",
                table: "CVApplicationDepartment",
                column: "DepartmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_CVApplications_CompanyId",
                table: "CVApplications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CVApplications_CompanyId1",
                table: "CVApplications",
                column: "CompanyId1");

            migrationBuilder.CreateIndex(
                name: "IX_CVApplications_UserId",
                table: "CVApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId1",
                table: "Departments",
                column: "CompanyId1");

            migrationBuilder.CreateIndex(
                name: "IX_JobAlerts_UserId",
                table: "JobAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CompanyId",
                table: "JobPostings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AnonymousUsername",
                table: "Users",
                column: "AnonymousUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EncryptedTcIdentityNumber",
                table: "Users",
                column: "EncryptedTcIdentityNumber",
                unique: true,
                filter: "\"EncryptedTcIdentityNumber\" IS NOT NULL AND \"EncryptedTcIdentityNumber\" != ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bans");

            migrationBuilder.DropTable(
                name: "CompanyBranches");

            migrationBuilder.DropTable(
                name: "CompanySubscriptions");

            migrationBuilder.DropTable(
                name: "ContractorReviews");

            migrationBuilder.DropTable(
                name: "CVApplicationDepartment");

            migrationBuilder.DropTable(
                name: "HRPersonnel");

            migrationBuilder.DropTable(
                name: "JobAlerts");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "ReviewVotes");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "Warnings");

            migrationBuilder.DropTable(
                name: "CVApplications");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}

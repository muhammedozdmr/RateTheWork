using FluentAssertions;
using Microsoft.Playwright;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.E2E.Tests.Common;

namespace RateTheWork.E2E.Tests.Scenarios;

/// <summary>
/// End-to-end tests for job search and application functionality.
/// Tests the complete user journey for finding and applying to jobs.
/// </summary>
public class JobSearchE2ETests : E2ETestBase
{
    [Fact]
    public async Task SearchJobs_Should_FilterAndDisplayResults()
    {
        // Arrange - Seed test data
        await SeedJobPostingsAsync();

        // Act - Navigate to job search
        await Page.GotoAsync("/jobs");

        // Search by keyword
        await Page.FillAsync("input[placeholder*='Search jobs']", "Software Engineer");
        await Page.ClickAsync("button[aria-label='Search']");

        // Wait for results
        await Page.WaitForSelectorAsync(".job-listing-item");

        // Apply filters
        await Page.ClickAsync("text=Remote Only");
        await Page.SelectOptionAsync("select[name='experienceLevel']", "Senior");
        await Page.FillAsync("input[name='minSalary']", "50000");

        // Wait for filtered results
        await Page.WaitForResponseAsync(resp => resp.Url.Contains("/api/jobpostings/search"));

        // Assert - Check results
        var jobCards = await Page.QuerySelectorAllAsync(".job-listing-item");
        jobCards.Count.Should().BeGreaterThan(0);

        var firstJobTitle = await Page.TextContentAsync(".job-listing-item:first-child .job-title");
        firstJobTitle.Should().Contain("Software Engineer");

        await TakeScreenshotAsync("job_search_results");
    }

    [Fact]
    public async Task ApplyToJob_Should_SubmitApplication()
    {
        // Arrange - Create user and job
        var (user, jobPosting) = await SeedUserAndJobAsync();

        // Login
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Navigate to job details
        await Page.GotoAsync($"/jobs/{jobPosting.Id}");

        // Click apply button
        await Page.ClickAsync("text=Apply Now");

        // Fill application form
        await Page.FillAsync("textarea[name='coverLetter']", "I am very interested in this position and believe my skills make me a perfect fit.");
        
        // Upload resume (simulate file upload)
        var fileInput = await Page.QuerySelectorAsync("input[type='file']");
        if (fileInput != null)
        {
            await fileInput.SetInputFilesAsync(new FilePayload
            {
                Name = "resume.pdf",
                MimeType = "application/pdf",
                Buffer = System.Text.Encoding.UTF8.GetBytes("Mock resume content")
            });
        }

        // Submit application
        await Page.ClickAsync("button[type='submit']");

        // Wait for success
        var successMessage = await Page.WaitForSelectorAsync("text=Application submitted successfully");
        successMessage.Should().NotBeNull();

        // Verify in database
        var application = await ExecuteDbContextAsync(async db =>
            await db.JobApplications.FirstOrDefaultAsync(a => 
                a.JobPostingId == jobPosting.Id && a.ApplicantUserId == user.Id));

        application.Should().NotBeNull();

        await TakeScreenshotAsync("job_application_success");
    }

    [Fact]
    public async Task JobDetails_Should_DisplayCompleteInformation()
    {
        // Arrange
        var jobPosting = await SeedDetailedJobPostingAsync();

        // Act - Navigate to job details
        await Page.GotoAsync($"/jobs/{jobPosting.Id}");

        // Assert - Check all sections are present
        var jobTitle = await Page.TextContentAsync("h1");
        jobTitle.Should().Be(jobPosting.Title);

        var companyName = await Page.TextContentAsync(".company-name");
        companyName.Should().NotBeNullOrEmpty();

        var salary = await Page.TextContentAsync(".salary-range");
        salary.Should().Contain("50,000");
        salary.Should().Contain("80,000");

        var requirements = await Page.QuerySelectorAllAsync(".requirement-item");
        requirements.Count.Should().Be(jobPosting.Requirements.Count);

        var locationInfo = await Page.TextContentAsync(".location-info");
        locationInfo.Should().Contain(jobPosting.Location);
        if (jobPosting.IsRemote)
        {
            locationInfo.Should().Contain("Remote");
        }

        await TakeScreenshotAsync("job_details_page");
    }

    [Fact]
    public async Task SaveJob_Should_AddToUserSavedJobs()
    {
        // Arrange
        var (user, jobPosting) = await SeedUserAndJobAsync();
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Navigate to job and save it
        await Page.GotoAsync($"/jobs/{jobPosting.Id}");
        
        var saveButton = await Page.WaitForSelectorAsync("button[aria-label='Save job']");
        await saveButton.ClickAsync();

        // Wait for save confirmation
        await Page.WaitForSelectorAsync("button[aria-label='Unsave job']");

        // Navigate to saved jobs
        await Page.GotoAsync("/dashboard/saved-jobs");

        // Assert - Job should appear in saved list
        var savedJobTitle = await Page.TextContentAsync(".saved-job-item .job-title");
        savedJobTitle.Should().Be(jobPosting.Title);

        await TakeScreenshotAsync("saved_jobs_list");
    }

    [Fact]
    public async Task JobApplicationTracking_Should_ShowApplicationStatus()
    {
        // Arrange
        var (user, jobPosting, application) = await SeedUserWithApplicationAsync();
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Navigate to applications
        await Page.GotoAsync("/dashboard/applications");

        // Assert - Should see application with status
        var applicationCard = await Page.WaitForSelectorAsync(".application-card");
        applicationCard.Should().NotBeNull();

        var jobTitle = await Page.TextContentAsync(".application-card .job-title");
        jobTitle.Should().Be(jobPosting.Title);

        var status = await Page.TextContentAsync(".application-status");
        status.Should().Be("Pending");

        // Check timeline
        var timelineItems = await Page.QuerySelectorAllAsync(".timeline-item");
        timelineItems.Count.Should().BeGreaterThan(0);

        await TakeScreenshotAsync("application_tracking");
    }

    private async Task SeedJobPostingsAsync()
    {
        await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Tech Corp",
                "Leading tech company",
                "https://techcorp.com",
                1000,
                2010,
                "Technology",
                "Istanbul"
            );

            var jobPostings = new[]
            {
                JobPosting.Create(
                    company.Id,
                    "Senior Software Engineer",
                    "We are looking for an experienced software engineer",
                    new[] { "5+ years experience", "C# expertise", "Cloud knowledge" },
                    "Istanbul",
                    70000m,
                    100000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.Senior,
                    true
                ),
                JobPosting.Create(
                    company.Id,
                    "Frontend Developer",
                    "React developer needed",
                    new[] { "3+ years React", "TypeScript", "CSS" },
                    "Ankara",
                    50000m,
                    70000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.MidLevel,
                    false
                ),
                JobPosting.Create(
                    company.Id,
                    "DevOps Engineer",
                    "DevOps position available",
                    new[] { "Kubernetes", "CI/CD", "AWS" },
                    "Remote",
                    60000m,
                    90000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.Senior,
                    true
                )
            };

            db.Companies.Add(company);
            db.JobPostings.AddRange(jobPostings);
            await db.SaveChangesAsync();
        });
    }

    private async Task<(User user, JobPosting jobPosting)> SeedUserAndJobAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var user = User.CreateForTesting("jobseeker@example.com", "jobseeker123");
            user.SetPassword("Test123!@#"); // Set a known password for testing

            var company = Company.Create(
                "Hiring Company",
                "Great place to work",
                "https://hiringcompany.com",
                500,
                2015,
                "Technology",
                "Istanbul"
            );

            var jobPosting = JobPosting.Create(
                company.Id,
                "Full Stack Developer",
                "Looking for talented full stack developer",
                new[] { "React", "Node.js", "PostgreSQL" },
                "Istanbul",
                45000m,
                65000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.MidLevel,
                true
            );

            db.Users.Add(user);
            db.Companies.Add(company);
            db.JobPostings.Add(jobPosting);
            await db.SaveChangesAsync();

            return (user, jobPosting);
        });
    }

    private async Task<JobPosting> SeedDetailedJobPostingAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Detailed Company",
                "Company with detailed job posting",
                "https://detailedcompany.com",
                750,
                2012,
                "Technology",
                "Istanbul"
            );

            var jobPosting = JobPosting.Create(
                company.Id,
                "Senior Backend Developer",
                "We are seeking a talented Senior Backend Developer to join our growing team. You will be responsible for designing and implementing scalable backend services.",
                new[] { 
                    "5+ years of backend development experience",
                    "Strong knowledge of C# and .NET Core",
                    "Experience with microservices architecture",
                    "Proficiency in PostgreSQL and Redis",
                    "Understanding of Docker and Kubernetes"
                },
                "Istanbul",
                50000m,
                80000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.Senior,
                true
            );

            db.Companies.Add(company);
            db.JobPostings.Add(jobPosting);
            await db.SaveChangesAsync();

            return jobPosting;
        });
    }

    private async Task<(User user, JobPosting jobPosting, JobApplication application)> SeedUserWithApplicationAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var user = User.CreateForTesting("applicant@example.com", "applicant123");
            user.SetPassword("Test123!@#");

            var company = Company.Create(
                "Applied Company",
                "Company user applied to",
                "https://appliedcompany.com",
                300,
                2018,
                "Technology",
                "Ankara"
            );

            var jobPosting = JobPosting.Create(
                company.Id,
                "Software Developer",
                "Software developer position",
                new[] { "C#", ".NET", "SQL" },
                "Ankara",
                40000m,
                60000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.MidLevel,
                false
            );

            var application = JobApplication.Create(
                jobPosting.Id,
                user.Id,
                company.Id,
                "Applicant Name",
                user.Email,
                "+905551234567",
                "I am interested in this position",
                "https://example.com/resume.pdf"
            );

            db.Users.Add(user);
            db.Companies.Add(company);
            db.JobPostings.Add(jobPosting);
            db.JobApplications.Add(application);
            await db.SaveChangesAsync();

            return (user, jobPosting, application);
        });
    }
}
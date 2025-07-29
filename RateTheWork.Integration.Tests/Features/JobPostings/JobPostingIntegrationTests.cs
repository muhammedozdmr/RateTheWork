using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Integration.Tests.Common;

namespace RateTheWork.Integration.Tests.Features.JobPostings;

/// <summary>
/// Integration tests for job posting features including creation, search, and applications.
/// Tests the complete flow from API to database for job-related operations.
/// </summary>
public class JobPostingIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateJobPosting_Should_PersistToDatabase()
    {
        // Arrange
        var company = await SeedCompanyAsync();
        
        var command = new
        {
            CompanyId = company.Id,
            Title = "Senior Software Engineer",
            Description = "We are looking for an experienced software engineer",
            Requirements = new[] { "5+ years experience", "C# expertise", "Cloud knowledge" },
            Location = "Istanbul",
            SalaryMin = 50000m,
            SalaryMax = 80000m,
            Currency = "TRY",
            JobType = "FullTime",
            ExperienceLevel = "Senior",
            IsRemote = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/jobpostings", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();

        // Verify database
        var jobPosting = await ExecuteDbContextAsync(async db =>
            await db.JobPostings.FirstOrDefaultAsync(j => j.Title == command.Title));
        
        jobPosting.Should().NotBeNull();
        jobPosting!.CompanyId.Should().Be(command.CompanyId);
        jobPosting.IsRemote.Should().Be(command.IsRemote);
    }

    [Fact]
    public async Task SearchJobPostings_Should_ReturnFilteredResults()
    {
        // Arrange
        await SeedJobPostingsAsync();

        // Act
        var response = await Client.GetAsync("/api/jobpostings/search?keyword=engineer&location=Istanbul&isRemote=true&minSalary=40000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        
        var items = (IEnumerable<dynamic>)result!.items;
        items.Should().NotBeEmpty();
        items.Should().OnlyContain(j => ((string)j.title).Contains("Engineer", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApplyToJob_Should_CreateApplication()
    {
        // Arrange
        var (company, jobPosting, user) = await SeedJobPostingWithUserAsync();
        
        var application = new
        {
            JobPostingId = jobPosting.Id,
            ApplicantName = "John Doe",
            ApplicantEmail = "john@example.com",
            ApplicantPhone = "+905551234567",
            CoverLetter = "I am very interested in this position",
            ResumeUrl = "https://example.com/resume.pdf"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/jobpostings/apply", application);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database
        var savedApplication = await ExecuteDbContextAsync(async db =>
            await db.JobApplications.FirstOrDefaultAsync(a => a.JobPostingId == jobPosting.Id));
        
        savedApplication.Should().NotBeNull();
        savedApplication!.ApplicantName.Should().Be(application.ApplicantName);
        savedApplication.Status.Should().Be(ApplicationStatus.Pending);
    }

    [Fact]
    public async Task GetJobPostingById_Should_ReturnJobWithCompanyDetails()
    {
        // Arrange
        var (company, jobPosting, _) = await SeedJobPostingWithUserAsync();

        // Act
        var response = await Client.GetAsync($"/api/jobpostings/{jobPosting.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        ((string)result!.title).Should().Be(jobPosting.Title);
        result.company.Should().NotBeNull();
        ((string)result.company.name).Should().Be(company.Name);
    }

    [Fact]
    public async Task GetActiveJobPostings_Should_OnlyReturnActiveJobs()
    {
        // Arrange
        await SeedActiveAndInactiveJobPostingsAsync();

        // Act
        var response = await Client.GetAsync("/api/jobpostings/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        
        var items = (IEnumerable<dynamic>)result!.items;
        items.Should().NotBeEmpty();
        items.Should().OnlyContain(j => (bool)j.isActive == true);
    }

    [Fact]
    public async Task GetJobApplications_Should_ReturnUserApplications()
    {
        // Arrange
        var (_, jobPosting, user) = await SeedJobPostingWithApplicationsAsync();

        // Simulate authenticated request
        Client.DefaultRequestHeaders.Add("UserId", user.Id);

        // Act
        var response = await Client.GetAsync($"/api/jobpostings/applications/user/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        
        var applications = (IEnumerable<dynamic>)result!;
        applications.Should().NotBeEmpty();
    }

    private async Task<Company> SeedCompanyAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Tech Company",
                "Technology company",
                "https://techcompany.com",
                100,
                2020,
                "Technology",
                "Istanbul"
            );

            db.Companies.Add(company);
            await db.SaveChangesAsync();
            return company;
        });
    }

    private async Task SeedJobPostingsAsync()
    {
        await ExecuteDbContextAsync(async db =>
        {
            var company = await SeedCompanyAsync();

            var jobPostings = new[]
            {
                JobPosting.Create(
                    company.Id,
                    "Senior Software Engineer",
                    "Senior position for experienced developers",
                    new[] { "5+ years", "C#", "Azure" },
                    "Istanbul",
                    60000m,
                    90000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.Senior,
                    true
                ),
                JobPosting.Create(
                    company.Id,
                    "Junior Developer",
                    "Entry level position",
                    new[] { "0-2 years", "Basic programming" },
                    "Ankara",
                    30000m,
                    40000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.Junior,
                    false
                ),
                JobPosting.Create(
                    company.Id,
                    "DevOps Engineer",
                    "DevOps position",
                    new[] { "3+ years", "K8s", "Docker" },
                    "Istanbul",
                    50000m,
                    70000m,
                    "TRY",
                    JobType.FullTime,
                    ExperienceLevel.MidLevel,
                    true
                )
            };

            db.JobPostings.AddRange(jobPostings);
            await db.SaveChangesAsync();
        });
    }

    private async Task<(Company company, JobPosting jobPosting, User user)> SeedJobPostingWithUserAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Hiring Company",
                "Company that is hiring",
                "https://hiringcompany.com",
                200,
                2015,
                "Technology",
                "Istanbul"
            );

            var user = User.CreateForTesting("applicant@example.com", "applicant123");

            var jobPosting = JobPosting.Create(
                company.Id,
                "Full Stack Developer",
                "Looking for full stack developer",
                new[] { "3+ years", "React", "Node.js" },
                "Istanbul",
                45000m,
                65000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.MidLevel,
                true
            );

            db.Companies.Add(company);
            db.Users.Add(user);
            db.JobPostings.Add(jobPosting);
            await db.SaveChangesAsync();

            return (company, jobPosting, user);
        });
    }

    private async Task SeedActiveAndInactiveJobPostingsAsync()
    {
        await ExecuteDbContextAsync(async db =>
        {
            var company = await SeedCompanyAsync();

            var activeJob = JobPosting.Create(
                company.Id,
                "Active Position",
                "This position is active",
                new[] { "Requirements" },
                "Istanbul",
                40000m,
                60000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.MidLevel,
                false
            );

            var inactiveJob = JobPosting.Create(
                company.Id,
                "Inactive Position",
                "This position is inactive",
                new[] { "Requirements" },
                "Istanbul",
                40000m,
                60000m,
                "TRY",
                JobType.FullTime,
                ExperienceLevel.MidLevel,
                false
            );

            // Deactivate the second job
            inactiveJob.Deactivate();

            db.JobPostings.AddRange(activeJob, inactiveJob);
            await db.SaveChangesAsync();
        });
    }

    private async Task<(Company company, JobPosting jobPosting, User user)> SeedJobPostingWithApplicationsAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var (company, jobPosting, user) = await SeedJobPostingWithUserAsync();

            var applications = new[]
            {
                JobApplication.Create(
                    jobPosting.Id,
                    user.Id,
                    company.Id,
                    "John Doe",
                    "john@example.com",
                    "+905551234567",
                    "I am interested in this position",
                    "https://example.com/resume1.pdf"
                ),
                JobApplication.Create(
                    jobPosting.Id,
                    user.Id,
                    company.Id,
                    "Jane Smith",
                    "jane@example.com",
                    "+905559876543",
                    "I would like to apply",
                    "https://example.com/resume2.pdf"
                )
            };

            db.JobApplications.AddRange(applications);
            await db.SaveChangesAsync();

            return (company, jobPosting, user);
        });
    }
}
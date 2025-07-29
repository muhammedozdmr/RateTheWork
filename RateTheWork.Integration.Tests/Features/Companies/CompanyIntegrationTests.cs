using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RateTheWork.Application.Features.Companies.Commands.CreateCompany;
using RateTheWork.Application.Features.Companies.Queries.SearchCompanies;
using RateTheWork.Domain.Entities;
using RateTheWork.Integration.Tests.Common;

namespace RateTheWork.Integration.Tests.Features.Companies;

/// <summary>
/// Integration tests for company-related features including creation, search, and updates.
/// Tests API endpoints with actual database operations.
/// </summary>
public class CompanyIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCompany_Should_PersistToDatabase_WhenValidData()
    {
        // Arrange
        var command = new CreateCompanyCommand
        {
            Name = "Tech Corp",
            Description = "A leading technology company",
            Website = "https://techcorp.com",
            EmployeeCount = 500,
            FoundedYear = 2010,
            Industry = "Technology",
            Headquarters = "Istanbul"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/companies", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<CreateCompanyCommandResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(command.Name);

        // Verify database
        var company = await ExecuteDbContextAsync(async db =>
            await db.Companies.FirstOrDefaultAsync(c => c.Name == command.Name));
        
        company.Should().NotBeNull();
        company!.Description.Should().Be(command.Description);
        company.Website.Should().Be(command.Website);
    }

    [Fact]
    public async Task SearchCompanies_Should_ReturnFilteredResults()
    {
        // Arrange
        await SeedCompaniesAsync();

        var query = new SearchCompaniesQuery
        {
            SearchTerm = "Tech",
            Industry = "Technology",
            MinEmployeeCount = 100,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await Client.GetAsync($"/api/companies/search?searchTerm={query.SearchTerm}&industry={query.Industry}&minEmployeeCount={query.MinEmployeeCount}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SearchCompaniesQueryResponse>();
        result.Should().NotBeNull();
        result!.Companies.Should().NotBeEmpty();
        result.Companies.Should().OnlyContain(c => c.Name.Contains("Tech") || c.Industry == "Technology");
        result.Companies.Should().OnlyContain(c => c.EmployeeCount >= 100);
    }

    [Fact]
    public async Task GetCompanyById_Should_ReturnCompanyWithDetails()
    {
        // Arrange
        var company = await SeedSingleCompanyAsync();

        // Act
        var response = await Client.GetAsync($"/api/companies/{company.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        ((string)result!.name).Should().Be(company.Name);
    }

    [Fact]
    public async Task GetCompanyById_Should_ReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Act
        var response = await Client.GetAsync($"/api/companies/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCompany_Should_ModifyExistingCompany()
    {
        // Arrange
        var company = await SeedSingleCompanyAsync();
        
        var updateCommand = new
        {
            Name = "Updated Tech Corp",
            Description = "Updated description",
            Website = "https://updated-techcorp.com",
            EmployeeCount = 750
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/companies/{company.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database
        var updatedCompany = await ExecuteDbContextAsync(async db =>
            await db.Companies.FirstOrDefaultAsync(c => c.Id == company.Id));
        
        updatedCompany.Should().NotBeNull();
        updatedCompany!.Name.Should().Be(updateCommand.Name);
        updatedCompany.Description.Should().Be(updateCommand.Description);
        updatedCompany.EmployeeCount.Should().Be(updateCommand.EmployeeCount);
    }

    [Fact]
    public async Task GetCompanyReviews_Should_ReturnPaginatedReviews()
    {
        // Arrange
        var company = await SeedCompanyWithReviewsAsync();

        // Act
        var response = await Client.GetAsync($"/api/companies/{company.Id}/reviews?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        ((int)result!.totalCount).Should().BeGreaterThan(0);
    }

    private async Task<Company> SeedSingleCompanyAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Test Tech Corp",
                "A test technology company",
                "https://test-techcorp.com",
                500,
                2015,
                "Technology",
                "Istanbul"
            );

            db.Companies.Add(company);
            await db.SaveChangesAsync();
            return company;
        });
    }

    private async Task SeedCompaniesAsync()
    {
        await ExecuteDbContextAsync(async db =>
        {
            var companies = new[]
            {
                Company.Create("Tech Innovators", "Innovation company", "https://techinnovators.com", 250, 2018, "Technology", "Istanbul"),
                Company.Create("Digital Solutions", "Digital services", "https://digitalsolutions.com", 150, 2020, "Technology", "Ankara"),
                Company.Create("Finance Plus", "Financial services", "https://financeplus.com", 300, 2010, "Finance", "Istanbul"),
                Company.Create("Small Startup", "New startup", "https://smallstartup.com", 50, 2022, "Technology", "Izmir")
            };

            db.Companies.AddRange(companies);
            await db.SaveChangesAsync();
        });
    }

    private async Task<Company> SeedCompanyWithReviewsAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Reviewed Company",
                "A company with reviews",
                "https://reviewed-company.com",
                1000,
                2010,
                "Technology",
                "Istanbul"
            );

            var user = User.CreateForTesting("reviewer@example.com", "reviewer123");

            db.Companies.Add(company);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var reviews = new[]
            {
                Review.Create(company.Id, user.Id, Domain.Enums.Review.CommentType.WorkEnvironment, 4.5m, "Great work environment!"),
                Review.Create(company.Id, user.Id, Domain.Enums.Review.CommentType.Management, 4.0m, "Good management team"),
                Review.Create(company.Id, user.Id, Domain.Enums.Review.CommentType.CareerGrowth, 3.5m, "Decent career opportunities")
            };

            db.Reviews.AddRange(reviews);
            await db.SaveChangesAsync();

            return company;
        });
    }
}
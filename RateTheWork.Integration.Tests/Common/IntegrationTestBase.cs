using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RateTheWork.Integration.Tests.Common;

/// <summary>
/// Base class for integration tests that provides test container support and database setup.
/// Uses PostgreSQL test container for realistic database testing.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("ratethework_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add PostgreSQL database using test container
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_postgres.GetConnectionString());
                    });

                    // Ensure database is created
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureCreated();
                });
            });

        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
    }

    public async Task DisposeAsync()
    {
        Scope?.Dispose();
        Client?.Dispose();
        Factory?.Dispose();
        await _postgres.DisposeAsync();
    }

    protected T GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        var dbContext = GetService<ApplicationDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        var dbContext = GetService<ApplicationDbContext>();
        await action(dbContext);
    }

    protected async Task ClearDatabase()
    {
        var dbContext = GetService<ApplicationDbContext>();
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Companies.RemoveRange(dbContext.Companies);
        dbContext.JobPostings.RemoveRange(dbContext.JobPostings);
        dbContext.Reviews.RemoveRange(dbContext.Reviews);
        dbContext.JobApplications.RemoveRange(dbContext.JobApplications);
        await dbContext.SaveChangesAsync();
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using RateTheWork.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RateTheWork.E2E.Tests.Common;

/// <summary>
/// Base class for E2E tests that provides browser automation and full application testing.
/// Uses Playwright for browser automation and PostgreSQL test container for database.
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("ratethework_e2e")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = null!;
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        await _postgres.StartAsync();

        // Setup web application
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("E2E");
                
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add test database
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

        // Get base URL
        var client = Factory.CreateClient();
        BaseUrl = client.BaseAddress!.ToString().TrimEnd('/');

        // Setup Playwright
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false to see browser during test
        });
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
        Factory?.Dispose();
        await _postgres.DisposeAsync();
    }

    protected async Task<string> TakeScreenshotAsync(string name)
    {
        var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots", $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
        return screenshotPath;
    }

    protected async Task LoginAsUserAsync(string email, string password)
    {
        await Page.GotoAsync("/login");
        await Page.FillAsync("input[type='email']", email);
        await Page.FillAsync("input[type='password']", password);
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForURLAsync("**/dashboard");
    }

    protected async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(dbContext);
    }

    protected async Task WaitForApiCallAsync(string urlPattern)
    {
        await Page.WaitForResponseAsync(response => 
            response.Url.Contains(urlPattern) && response.Status == 200);
    }
}
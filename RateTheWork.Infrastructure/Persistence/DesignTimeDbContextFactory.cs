using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RateTheWork.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // For migrations, use a default connection string
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? "Host=localhost;Database=ratethework;Username=postgres;Password=postgres";

        // Convert Railway DATABASE_URL to Npgsql format if it's a URL
        string npgsqlConnectionString;
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            // Parse URL format: postgresql://user:password@host:port/database
            var uri = new Uri(connectionString.Replace("postgres://", "postgresql://"));
            var userInfo = uri.UserInfo.Split(':');
            
            npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            // Already in standard format
            npgsqlConnectionString = connectionString;
        }
        
        optionsBuilder.UseNpgsql(npgsqlConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
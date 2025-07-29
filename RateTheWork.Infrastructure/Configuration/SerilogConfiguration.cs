using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

namespace RateTheWork.Infrastructure.Configuration;

public static class SerilogConfiguration
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            var connectionString = context.Configuration["DATABASE_URL"] ??
                                   context.Configuration.GetConnectionString("DefaultConnection");

            configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "RateTheWork")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
                .WriteTo.Debug();

            // Üretim ortamı log kaydı
            if (!context.HostingEnvironment.IsDevelopment())
            {
                // PostgreSQL log kaydı
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var columnOptions = new Dictionary<string, ColumnWriterBase>
                    {
                        { "message", new RenderedMessageColumnWriter() }
                        , { "message_template", new MessageTemplateColumnWriter() }
                        , { "level", new LevelColumnWriter() }, { "timestamp", new TimestampColumnWriter() }
                        , { "exception", new ExceptionColumnWriter() }
                        , { "log_event", new LogEventSerializedColumnWriter() }
                        , { "properties", new PropertiesColumnWriter() }
                    };

                    configuration.WriteTo.PostgreSQL(
                        connectionString: ConvertToNpgsqlFormat(connectionString),
                        tableName: "Logs",
                        columnOptions: columnOptions,
                        needAutoCreateTable: true,
                        restrictedToMinimumLevel: LogEventLevel.Warning);
                }

                // Seq log kaydı (eğer yapılandırıldıysa)
                var seqUrl = context.Configuration["Seq:ServerUrl"];
                if (!string.IsNullOrEmpty(seqUrl))
                {
                    configuration.WriteTo.Seq(seqUrl, apiKey: context.Configuration["Seq:ApiKey"]);
                }
            }
        });
    }

    private static string ConvertToNpgsqlFormat(string connectionString)
    {
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            var uri = new Uri(connectionString.Replace("postgres://", "postgresql://"));
            var userInfo = uri.UserInfo.Split(':');

            return
                $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }

        return connectionString;
    }
}

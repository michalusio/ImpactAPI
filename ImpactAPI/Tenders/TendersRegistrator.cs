using ImpactAPI.Tenders.Controller;
using ImpactAPI.Tenders.Database;
using ImpactAPI.Tenders.External;
using ImpactAPI.Tenders.Service;
using Infrastructure;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace ImpactAPI.Tenders;

public class TendersRegistrator : ISliceRegistrator
{
    public async Task RegisterWebApplicationBuilder(IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration.GetSection("Tenders");
        builder.Services.AddControllers(c =>
        {
            c.Filters.Add<UnavailableBeforeTendersLoadActionFilter>();
        });

        builder.Services
            .AddRefitClient<ITendersGuruAPI>()
            .ConfigureHttpClient(c => c.BaseAddress = configuration.GetValue<Uri>("GuruAPIUri")!);

        if (builder.Environment.IsDevelopment() && !EF.IsDesignTime)
        {
            var db = new TestContainerDatabase("Tenders");
            builder.Services.AddHostedService(_ => db);
            await db.PreStartDatabase();
            builder.Configuration.AddInMemoryCollection([
                new KeyValuePair<string, string?>($"Tenders:ConnectionString", db.ConnectionString)
            ]);
        }

        builder.Services.AddDbContext<TendersDbContext>(options => options
            .UseSqlServer(configuration.GetValue<string>("ConnectionString"))
        );

        builder.Services.AddSingleton<TenderDownloaderHostedService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<TenderDownloaderHostedService>());

        builder.Services.AddScoped<TenderService>();
        builder.Services.AddScoped<UnavailableBeforeTendersLoadActionFilter>();
    }

    public async Task RegisterWebApplication(IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment() && !EF.IsDesignTime)
        {
            using var scope = app.ApplicationServices.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TendersDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TendersDbContext>>();
            try
            {

                logger.LogInformation("Performing Tenders DB migration");
                await db.Database.MigrateAsync();
                logger.LogInformation("Tenders DB migration performed succesfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tenders DB migration failed");
            }
        }
    }
}

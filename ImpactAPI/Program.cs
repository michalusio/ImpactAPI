using System.Text.Json.Serialization;
using Infrastructure;
using Infrastructure.Querying;

namespace ImpactAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var registrators = ISliceRegistrator.GetAllRegistrators();
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        foreach (var registrator in registrators)
        {
            await registrator.RegisterWebApplicationBuilder(builder);
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<NotFoundExceptionMiddleware>();

        foreach (var registrator in registrators)
        {
            await registrator.RegisterWebApplication(app);
        }

        app.MapControllers();

        app.Run();
    }
}

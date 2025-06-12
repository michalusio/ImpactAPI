using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace Infrastructure.Database;

public class TestContainerDatabase(string serviceName, bool withVolumeMount = true) : IHostedService, IAsyncDisposable
{
    private readonly MsSqlContainer _database = new Lazy<MsSqlContainer>(() =>
    {
        var builder = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04");
        if (withVolumeMount)
        {
            builder = builder.WithVolumeMount(serviceName + "DbVolume", "/var/opt/mssql");
        }
        return builder.Build();
    }).Value;

    public string ServiceName => serviceName;
    public string ConnectionString => _database.GetConnectionString();

    public async Task PreStartDatabase()
    {
        await _database.StartAsync();
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        if (_database.State == TestcontainersStates.Running) return;
        await _database.StartAsync(stoppingToken);
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        await _database.StopAsync(stoppingToken);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _database.DisposeAsync();
    }
}

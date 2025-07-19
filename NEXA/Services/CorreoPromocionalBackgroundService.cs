using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NEXA.Services
{
    public class CorreoPromocionalBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CorreoPromocionalBackgroundService> _logger;

        public CorreoPromocionalBackgroundService(IServiceProvider serviceProvider, ILogger<CorreoPromocionalBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobService = scope.ServiceProvider.GetRequiredService<CorreoPromocionalJobService>();

                    try
                    {
                        _logger.LogInformation("Procesando correos promocionales...");
                        await jobService.ProcesarCorreosPendientesAsync();
                        _logger.LogInformation("Correos procesados correctamente.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando correos.");
                    }
                }

                // Esperar 10 segundos antes de la siguiente ejecución para que podamos hacer pruebas sin esperar tanto
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

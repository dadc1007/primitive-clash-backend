namespace PrimitiveClash.Backend.Services.Impl
{
    public class GameLoopWorker(IServiceProvider serviceProvider) : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private Timer? _timer = null;

        // 20 Ticks por segundo
        private const int TickIntervalMs = 5000;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TickIntervalMs));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            // Se crea un scope para resolver el GameLoopService, necesario en IHostedService
            using var scope = _serviceProvider.CreateScope();
            // Usamos GetRequiredService para asegurar que la dependencia exista
            var gameLoopService = scope.ServiceProvider.GetRequiredService<IGameLoopService>();

            // Llama al método de procesamiento paralelo de todos los juegos
            await gameLoopService.ProcessTick();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
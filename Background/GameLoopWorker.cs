using PrimitiveClash.Backend.Services;

namespace PrimitiveClash.Backend.Background
{
    public class GameLoopWorker(IServiceProvider serviceProvider) : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private Timer? _timer = null;

        private const int TickIntervalMs = 1000;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TickIntervalMs));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IGameLoopService gameLoopService = scope.ServiceProvider.GetRequiredService<IGameLoopService>();

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
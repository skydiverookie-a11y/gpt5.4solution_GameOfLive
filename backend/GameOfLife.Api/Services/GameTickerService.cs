namespace GameOfLife.Api.Services;

public sealed class GameTickerService(GameOfLifeService gameService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            gameService.Tick();
            await Task.Delay(gameService.GetTickDelayMilliseconds(), stoppingToken);
        }
    }
}

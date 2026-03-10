using GameOfLife.Api.Models;

namespace GameOfLife.Api.Services;

public interface IGameOfLifeService
{
    GameState GetState();
    GameState ConfigureGrid(int width, int height);
    GameState ToggleCell(int x, int y);
    GameState Clear();
    GameState Randomize(int density);
    GameState LoadPattern(string name);
    GameState Step();
    GameState Reset();
    GameState SetPlayback(bool isPlaying);
    GameState SetSpeed(int speed);
}

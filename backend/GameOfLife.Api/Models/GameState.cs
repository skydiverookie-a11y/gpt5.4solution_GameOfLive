namespace GameOfLife.Api.Models;

public sealed record GameState(
    int Width,
    int Height,
    int Generation,
    int Speed,
    bool IsPlaying,
    IReadOnlyCollection<CellCoordinate> AliveCells,
    IReadOnlyList<string> AvailablePatterns);

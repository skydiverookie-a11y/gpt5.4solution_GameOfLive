namespace GameOfLife.Api.Models;

public sealed record GamePattern(string Name, IReadOnlyList<CellCoordinate> Cells);

using GameOfLife.Api.Models;

namespace GameOfLife.Api.Services;

public sealed class GameOfLifeService : IGameOfLifeService
{
    public const int MinSize = 10;
    public const int MaxSize = 100;
    public const int DefaultSize = 30;
    public const int MinSpeed = 1;
    public const int MaxSpeed = 30;

    private static readonly IReadOnlyDictionary<string, GamePattern> Patterns =
        new Dictionary<string, GamePattern>(StringComparer.OrdinalIgnoreCase)
        {
            ["Glider"] = new("Glider", new CellCoordinate[]
            {
                new CellCoordinate(1, 0),
                new CellCoordinate(2, 1),
                new CellCoordinate(0, 2),
                new CellCoordinate(1, 2),
                new CellCoordinate(2, 2)
            }),
            ["Blinker"] = new("Blinker", new CellCoordinate[]
            {
                new CellCoordinate(0, 1),
                new CellCoordinate(1, 1),
                new CellCoordinate(2, 1)
            }),
            ["Pulsar"] = new("Pulsar", new CellCoordinate[]
            {
                new(2, 0), new(3, 0), new(4, 0), new(8, 0), new(9, 0), new(10, 0),
                new(0, 2), new(5, 2), new(7, 2), new(12, 2),
                new(0, 3), new(5, 3), new(7, 3), new(12, 3),
                new(0, 4), new(5, 4), new(7, 4), new(12, 4),
                new(2, 5), new(3, 5), new(4, 5), new(8, 5), new(9, 5), new(10, 5),
                new(2, 7), new(3, 7), new(4, 7), new(8, 7), new(9, 7), new(10, 7),
                new(0, 8), new(5, 8), new(7, 8), new(12, 8),
                new(0, 9), new(5, 9), new(7, 9), new(12, 9),
                new(0, 10), new(5, 10), new(7, 10), new(12, 10),
                new(2, 12), new(3, 12), new(4, 12), new(8, 12), new(9, 12), new(10, 12)
            }),
            ["Gosper Glider Gun"] = new("Gosper Glider Gun", new CellCoordinate[]
            {
                new(1, 5), new(1, 6), new(2, 5), new(2, 6),
                new(11, 5), new(11, 6), new(11, 7),
                new(12, 4), new(12, 8),
                new(13, 3), new(13, 9),
                new(14, 3), new(14, 9),
                new(15, 6),
                new(16, 4), new(16, 8),
                new(17, 5), new(17, 6), new(17, 7),
                new(18, 6),
                new(21, 3), new(21, 4), new(21, 5),
                new(22, 3), new(22, 4), new(22, 5),
                new(23, 2), new(23, 6),
                new(25, 1), new(25, 2), new(25, 6), new(25, 7),
                new(35, 3), new(35, 4), new(36, 3), new(36, 4)
            }),
            ["Lightweight Spaceship (LWSS)"] = new("Lightweight Spaceship (LWSS)", new CellCoordinate[]
            {
                new(1, 0), new(4, 0),
                new(0, 1),
                new(0, 2), new(4, 2),
                new(0, 3), new(1, 3), new(2, 3), new(3, 3)
            })
        };

    private readonly Lock _lock = new();
    private readonly Random _random = new();

    private int _width = DefaultSize;
    private int _height = DefaultSize;
    private int _generation;
    private int _speed = 10;
    private bool _isPlaying;
    private HashSet<CellCoordinate> _aliveCells = [];
    private HashSet<CellCoordinate> _initialAliveCells = [];

    public GameState GetState()
    {
        lock (_lock)
        {
            return CreateState();
        }
    }

    public GameState ConfigureGrid(int width, int height)
    {
        ValidateDimensions(width, height);

        lock (_lock)
        {
            _width = width;
            _height = height;
            _aliveCells = [];
            _initialAliveCells = [];
            _generation = 0;
            _isPlaying = false;
            return CreateState();
        }
    }

    public GameState ToggleCell(int x, int y)
    {
        lock (_lock)
        {
            ValidateCoordinate(x, y);
            PrepareManualEdit();

            var cell = new CellCoordinate(x, y);
            if (!_aliveCells.Add(cell))
            {
                _aliveCells.Remove(cell);
            }

            CaptureInitialState();
            return CreateState();
        }
    }

    public GameState Clear()
    {
        lock (_lock)
        {
            PrepareManualEdit();
            _aliveCells.Clear();
            CaptureInitialState();
            return CreateState();
        }
    }

    public GameState Randomize(int density)
    {
        if (density is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(density), "Density must be between 0 and 100.");
        }

        lock (_lock)
        {
            PrepareManualEdit();
            _aliveCells.Clear();

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    if (_random.Next(100) < density)
                    {
                        _aliveCells.Add(new CellCoordinate(x, y));
                    }
                }
            }

            CaptureInitialState();
            return CreateState();
        }
    }

    public GameState LoadPattern(string name)
    {
        if (!Patterns.TryGetValue(name, out var pattern))
        {
            throw new KeyNotFoundException($"Unknown pattern '{name}'.");
        }

        lock (_lock)
        {
            PrepareManualEdit();
            _aliveCells = CenterPattern(pattern.Cells);
            CaptureInitialState();
            return CreateState();
        }
    }

    public GameState Step()
    {
        lock (_lock)
        {
            AdvanceGeneration();
            return CreateState();
        }
    }

    public GameState Reset()
    {
        lock (_lock)
        {
            _aliveCells = new HashSet<CellCoordinate>(_initialAliveCells);
            _generation = 0;
            _isPlaying = false;
            return CreateState();
        }
    }

    public GameState SetPlayback(bool isPlaying)
    {
        lock (_lock)
        {
            _isPlaying = isPlaying;
            return CreateState();
        }
    }

    public GameState SetSpeed(int speed)
    {
        if (speed is < MinSpeed or > MaxSpeed)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), $"Speed must be between {MinSpeed} and {MaxSpeed}.");
        }

        lock (_lock)
        {
            _speed = speed;
            return CreateState();
        }
    }

    public void Tick()
    {
        lock (_lock)
        {
            if (!_isPlaying)
            {
                return;
            }

            AdvanceGeneration();
        }
    }

    public int GetTickDelayMilliseconds()
    {
        lock (_lock)
        {
            return (int)Math.Round(1000d / _speed, MidpointRounding.AwayFromZero);
        }
    }

    private void AdvanceGeneration()
    {
        var next = new HashSet<CellCoordinate>();
        var neighborCounts = new Dictionary<CellCoordinate, int>();

        foreach (var cell in _aliveCells)
        {
            foreach (var neighbor in GetNeighbors(cell))
            {
                neighborCounts.TryGetValue(neighbor, out var count);
                neighborCounts[neighbor] = count + 1;
            }
        }

        foreach (var (cell, count) in neighborCounts)
        {
            var isAlive = _aliveCells.Contains(cell);
            if (count == 3 || (isAlive && count == 2))
            {
                next.Add(cell);
            }
        }

        _aliveCells = next;
        _generation++;
    }

    private IEnumerable<CellCoordinate> GetNeighbors(CellCoordinate cell)
    {
        for (var offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                var x = Wrap(cell.X + offsetX, _width);
                var y = Wrap(cell.Y + offsetY, _height);
                yield return new CellCoordinate(x, y);
            }
        }
    }

    private HashSet<CellCoordinate> CenterPattern(IReadOnlyList<CellCoordinate> patternCells)
    {
        if (patternCells.Count == 0)
        {
            return [];
        }

        var patternWidth = patternCells.Max(cell => cell.X) + 1;
        var patternHeight = patternCells.Max(cell => cell.Y) + 1;
        var offsetX = Math.Max(0, (_width - patternWidth) / 2);
        var offsetY = Math.Max(0, (_height - patternHeight) / 2);

        return patternCells
            .Select(cell => new CellCoordinate(
                Wrap(cell.X + offsetX, _width),
                Wrap(cell.Y + offsetY, _height)))
            .ToHashSet();
    }

    private static int Wrap(int value, int max)
    {
        var result = value % max;
        return result < 0 ? result + max : result;
    }

    private void PrepareManualEdit()
    {
        _generation = 0;
        _isPlaying = false;
    }

    private void CaptureInitialState()
    {
        _initialAliveCells = new HashSet<CellCoordinate>(_aliveCells);
    }

    private void ValidateCoordinate(int x, int y)
    {
        if (x < 0 || x >= _width)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {_width - 1}.");
        }

        if (y < 0 || y >= _height)
        {
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {_height - 1}.");
        }
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width is < MinSize or > MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(width), $"Width must be between {MinSize} and {MaxSize}.");
        }

        if (height is < MinSize or > MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(height), $"Height must be between {MinSize} and {MaxSize}.");
        }
    }

    private GameState CreateState() =>
        new(
            _width,
            _height,
            _generation,
            _speed,
            _isPlaying,
            _aliveCells.OrderBy(cell => cell.Y).ThenBy(cell => cell.X).ToArray(),
            Patterns.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray());
}

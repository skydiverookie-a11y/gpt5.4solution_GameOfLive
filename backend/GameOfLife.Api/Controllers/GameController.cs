using GameOfLife.Api.Models.Requests;
using GameOfLife.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.Api.Controllers;

[ApiController]
[Route("api/game")]
public sealed class GameController(IGameOfLifeService gameService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetState() => Ok(gameService.GetState());

    [HttpPut("grid")]
    public IActionResult ConfigureGrid([FromBody] ConfigureGridRequest request) =>
        Execute(() => gameService.ConfigureGrid(request.Width, request.Height));

    [HttpPost("cells/toggle")]
    public IActionResult ToggleCell([FromBody] ToggleCellRequest request) =>
        Execute(() => gameService.ToggleCell(request.X, request.Y));

    [HttpPost("clear")]
    public IActionResult Clear() => Ok(gameService.Clear());

    [HttpPost("randomize")]
    public IActionResult Randomize([FromBody] RandomizeGridRequest request) =>
        Execute(() => gameService.Randomize(request.Density));

    [HttpPost("patterns/{name}")]
    public IActionResult LoadPattern(string name) =>
        Execute(() => gameService.LoadPattern(name));

    [HttpPost("step")]
    public IActionResult Step() => Ok(gameService.Step());

    [HttpPost("reset")]
    public IActionResult Reset() => Ok(gameService.Reset());

    [HttpPut("playback")]
    public IActionResult SetPlayback([FromBody] SetPlaybackRequest request) =>
        Ok(gameService.SetPlayback(request.IsPlaying));

    [HttpPut("speed")]
    public IActionResult SetSpeed([FromBody] SetSpeedRequest request) =>
        Execute(() => gameService.SetSpeed(request.Speed));

    private IActionResult Execute(Func<object> action)
    {
        try
        {
            return Ok(action());
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { error = exception.Message });
        }
    }
}

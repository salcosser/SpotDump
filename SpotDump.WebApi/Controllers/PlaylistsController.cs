using Microsoft.AspNetCore.Mvc;
using SpotDump.Models;
using SpotDump.WebApi.Repositories;

namespace SpotDump.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IPlaylistExportService _playlistExportService;
    private readonly ILogger<PlaylistsController> _logger;

    public PlaylistsController(
        IPlaylistRepository playlistRepository,
        IPlaylistExportService playlistExportService,
        ILogger<PlaylistsController> logger)
    {
        _playlistRepository = playlistRepository;
        _playlistExportService = playlistExportService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlaylistCollectionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlaylists(
        [FromQuery] bool createdByMe = false,
        [FromQuery] string? name = null,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        var query = new PlaylistQueryOptions
        {
            CreatedByCurrentUser = createdByMe,
            NameContains = name
        };

        var playlists = await _playlistRepository.GetPlaylistsAsync(query, cancellationToken);

        if (IsCsv(format))
        {
            var export = await _playlistExportService.WriteCsvAsync(playlists, "playlists", BuildDownloadLink, cancellationToken);
            return Ok(export);
        }

        return Ok(new PlaylistCollectionResult { Playlists = playlists });
    }

    [HttpGet("{playlistId}")]
    [ProducesResponseType(typeof(PlaylistSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlaylistById(
        string playlistId,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        var playlist = await _playlistRepository.GetPlaylistAsync(playlistId, cancellationToken);
        if (playlist == null)
        {
            return NotFound();
        }

        if (IsCsv(format))
        {
            var export = await _playlistExportService.WriteCsvAsync(
                new[] { playlist },
                $"playlist_{playlist.Id}",
                BuildDownloadLink,
                cancellationToken);
            return Ok(export);
        }

        return Ok(playlist);
    }

    [HttpGet("exports/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DownloadExport(string fileName)
    {
        var exportPath = _playlistExportService.ResolvePath(fileName);
        if (!System.IO.File.Exists(exportPath))
        {
            return NotFound();
        }

        return PhysicalFile(exportPath, "text/csv", fileName);
    }

    private string? BuildDownloadLink(string fileName)
    {
        try
        {
            return Url.ActionLink(nameof(DownloadExport), values: new { fileName });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate download link for {FileName}", fileName);
            return null;
        }
    }

    private static bool IsCsv(string? format) =>
        string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
}

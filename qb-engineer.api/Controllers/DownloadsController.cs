using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Serves client-side tool installers (RFID relay, label printer driver, etc.).
/// Installers are built via tools/rfid-relay/build-installer.ps1 and placed in tools/dist/,
/// which is volume-mounted into the container at /app/tools (read-only).
/// Requires authentication so random internet users cannot enumerate or download binaries.
/// </summary>
[ApiController]
[Route("api/v1/downloads")]
[Authorize]
public class DownloadsController : ControllerBase
{
    private readonly string _toolsPath;

    public DownloadsController(IConfiguration config, IWebHostEnvironment env)
    {
        // Allow override via config; default to /app/tools (Docker volume mount)
        _toolsPath = config["ToolsPath"] ?? Path.Combine(env.ContentRootPath, "tools");
    }

    /// <summary>
    /// Returns the RFID Relay Windows installer (rfid-relay-setup.exe).
    /// Build it once with: tools/rfid-relay/build-installer.ps1
    /// The output lands in tools/dist/ which is mounted into the container.
    /// </summary>
    [HttpGet("rfid-relay-setup.exe")]
    public IActionResult GetRfidRelayInstaller()
    {
        var path = Path.Combine(_toolsPath, "rfid-relay-setup.exe");

        if (!System.IO.File.Exists(path))
            return NotFound(new { message = "RFID relay installer not available. Contact your administrator to run tools/rfid-relay/build-installer.ps1." });

        return PhysicalFile(path, "application/octet-stream", "rfid-relay-setup.exe");
    }
}

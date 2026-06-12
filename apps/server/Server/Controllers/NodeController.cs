using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Common.Models;

namespace Server.Controllers;

[ApiController]
[Route("node")]
public class NodeController : ControllerBase
{
	[HttpGet("{id}")]
	public async Task<IActionResult> GetUnknownNode(string id, [FromServices] IServiceProvider sp)
	{
		// Call to the node service?
	}

	[HttpGet("package/{id}")]
	public IActionResult GetPackageNode(string id)
	{
		return Ok(new { Type = "Package", Id = id, Data = "Package details" });
	}

	[HttpGet("dashboard/{id}")]
	public IActionResult GetDashboardNode(string id)
	{
		return Ok(new { Type = "Dashboard", Id = id, Data = "Dashboard details" });
	}

	[HttpGet("package/{id}/connections")]
	public IActionResult GetPackageConnections(string id)
	{
		return Ok(new { Type = "Package Connections", Id = id, Connections = new[] { "ConnA", "ConnB" } });
	}

	[HttpGet("dashboard/{id}/connections")]
	public IActionResult GetDashboardConnections(string id)
	{
		return Ok(new { Type = "Dashboard Connections", Id = id, Connections = new[] { "ConnC", "ConnD" } });
	}
}

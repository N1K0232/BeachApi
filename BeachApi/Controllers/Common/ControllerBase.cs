using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace BeachApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
	protected ControllerBase()
	{
	}
}
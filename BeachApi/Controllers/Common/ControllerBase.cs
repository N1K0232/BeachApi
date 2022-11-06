using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace BeachApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
	protected ControllerBase()
	{
	}
}
using Microsoft.AspNetCore.Mvc;

namespace BeachApi.Controllers;

public class UsersController : ControllerBase
{
	public UsersController()
	{
	}


	[HttpGet("Me")]
	public IActionResult GetMe() => NoContent();
}
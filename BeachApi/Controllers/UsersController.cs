using BeachApi.Authentication.Common;
using BeachApi.Authentication.Extensions;
using BeachApi.Authorization.Filters;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Shared.Models;
using BeachApi.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachApi.Controllers;

public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }


    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var loginResponse = await userService.LoginAsync(loginRequest);
        if (loginResponse is null)
        {
            return BadRequest("wrong email or password");
        }

        return Ok(loginResponse);
    }


    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var registerResponse = await userService.RegisterAsync(registerRequest);
        if (!registerResponse.Succeeded)
        {
            return BadRequest(registerResponse);
        }

        return Ok("User registrated");
    }


    [HttpPost("Refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var loginResponse = await userService.RefreshTokenAsync(refreshTokenRequest);
        if (loginResponse is null)
        {
            return BadRequest("couldn't refresh token");
        }

        return Ok(loginResponse);
    }

    [HttpGet("Me")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.Staff, RoleNames.User, RoleNames.Customer)]
    public IActionResult GetMe()
    {
        var user = new User
        {
            Id = User.GetId(),
            FirstName = User.GetFirstName(),
            LastName = User.GetLastName(),
            DateOfBirth = User.GetDateOfBirth(),
            PhoneNumber = User.GetPhoneNumber(),
            Email = User.GetEmail(),
            UserName = User.GetUserName()
        };

        return Ok(user);
    }
}
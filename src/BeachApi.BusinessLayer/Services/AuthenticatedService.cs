using AutoMapper;
using BeachApi.Authentication.Entities;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Contracts;
using BeachApi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApi.BusinessLayer.Services;

public class AuthenticatedService : IAuthenticatedService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public AuthenticatedService(UserManager<ApplicationUser> userManager, IUserService userService,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result<User>> GetAsync()
    {
        var userName = userService.GetUserName();
        var dbUser = await userManager.FindByNameAsync(userName);

        var user = mapper.Map<User>(dbUser);
        return user;
    }
}
using AutoMapper;
using BeachApi.Authentication.Entities;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.Contracts;
using BeachApi.Shared.Models;
using BeachApi.StorageProviders;
using Microsoft.AspNetCore.Identity;
using MimeMapping;
using OperationResults;

namespace BeachApi.BusinessLayer.Services;

public class AuthenticatedService : IAuthenticatedService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IStorageProvider storageProvider;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public AuthenticatedService(UserManager<ApplicationUser> userManager, IStorageProvider storageProvider,
        IUserService userService,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.storageProvider = storageProvider;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> AddProfilePhotoAsync(Stream stream, string fileName)
    {
        var userName = userService.GetUserName();
        var user = await userManager.FindByNameAsync(userName);

        var path = $"\\users\\{user.Id}_{fileName}";
        user.ProfilePhoto = path;

        await userManager.UpdateAsync(user);
        await storageProvider.UploadAsync(stream, path, true);

        return Result.Ok();
    }

    public async Task<Result<User>> GetAsync()
    {
        var userName = userService.GetUserName();
        var dbUser = await userManager.FindByNameAsync(userName);

        var user = mapper.Map<User>(dbUser);
        return user;
    }

    public async Task<Result<StreamFileContent>> GetProfilePhotoAsync()
    {
        var userName = userService.GetUserName();
        var user = await userManager.FindByIdAsync(userName);

        if (user.ProfilePhoto is null)
        {
            return Result.Fail(FailureReasons.ClientError, "This user doesn't have a profile photo");
        }

        var stream = await storageProvider.ReadAsync(user.ProfilePhoto);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found");
        }

        var contentType = MimeUtility.GetMimeMapping(user.ProfilePhoto);
        return new StreamFileContent(stream, contentType);
    }
}
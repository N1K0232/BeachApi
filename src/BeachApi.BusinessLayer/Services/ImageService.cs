using AutoMapper;
using BeachApi.BusinessLayer.Services.Interfaces;
using BeachApi.DataAccessLayer;
using BeachApi.Shared.Models;
using BeachApi.StorageProviders;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using Entities = BeachApi.DataAccessLayer.Entities;

namespace BeachApi.BusinessLayer.Services;

public class ImageService : IImageService
{
    private readonly IDataContext dataContext;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;

    public ImageService(IDataContext dataContext, IStorageProvider storageProvider, IMapper mapper)
    {
        this.dataContext = dataContext;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await dataContext.GetAsync<Entities.Image>(id);
        if (image is not null)
        {
            dataContext.Delete(image);
            await dataContext.SaveAsync();
            await storageProvider.DeleteAsync(image.Path);

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No image found");
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await dataContext.GetAsync<Entities.Image>(id);
        if (dbImage is not null)
        {
            var image = mapper.Map<Image>(dbImage);
            return image;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No image found");
    }

    public async Task<Result<IEnumerable<Image>>> GetListAsync()
    {
        var query = dataContext.Get<Entities.Image>();
        var dbImages = await query.OrderBy(i => i.Path)
            .ToListAsync();

        var images = mapper.Map<IEnumerable<Image>>(dbImages);
        return Result<IEnumerable<Image>>.Ok(images);
    }

    public async Task<Result<StreamFileContent>> ReadAsync(Guid id)
    {
        var image = await dataContext.GetAsync<Entities.Image>(id);
        if (image is not null)
        {
            var stream = await storageProvider.ReadAsync(image.Path);
            if (stream is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, "No stream found");
            }

            var contentType = MimeUtility.GetMimeMapping(image.Path);
            return new StreamFileContent(stream, contentType);
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No image found");
    }

    public async Task<Result<Image>> UploadAsync(StreamFileContent file, string description)
    {
        var path = CreatePath(file.DownloadFileName);
        var image = new Entities.Image
        {
            Path = path,
            Length = file.Content.Length,
            Description = description
        };

        dataContext.Insert(image);
        await dataContext.SaveAsync();
        await storageProvider.UploadAsync(file.Content, path);

        var savedImage = mapper.Map<Image>(image);
        return savedImage;
    }

    private static string CreatePath(string fileName)
    {
        var now = DateTime.UtcNow;
        return Path.Combine(now.Year.ToString("0000"), now.Month.ToString("00"), now.Day.ToString("00"), fileName);
    }
}
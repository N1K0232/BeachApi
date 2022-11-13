using BeachApi.Shared.Common;

namespace BeachApi.Shared.Requests;

public class SaveCategoryRequest : BaseRequestModel
{
    public string Name { get; set; }

    public string Description { get; set; }
}
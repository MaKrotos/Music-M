using MusicX.Core.Models;
using MusicX.Core.Models.General;

namespace MusicX.Core.Services;

public interface ICustomSectionsService
{
    ValueTask<ResponseData?> HandleSectionRequest(string id, string? nextFrom = null);
    IAsyncEnumerable<Section> GetSectionsAsync();
}
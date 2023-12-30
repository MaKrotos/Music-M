using System.Threading.Tasks;

namespace VkNet.AudioBypassService.Abstractions.Categories;

public interface ILoginCategory
{
    Task ConnectAsync(string uuid);
    Task ConnectAuthCodeAsync(string token, string uuid);
}
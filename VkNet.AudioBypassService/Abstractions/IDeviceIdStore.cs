using JetBrains.Annotations;
using System.Threading.Tasks;

namespace VkNet.AudioBypassService.Abstractions;

public interface IDeviceIdStore
{
    [ItemCanBeNull] ValueTask<string> GetDeviceIdAsync();
    ValueTask SetDeviceIdAsync([NotNull] string deviceId);
}
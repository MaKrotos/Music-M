using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VkNet.AudioBypassService.Abstractions;

public interface IDeviceIdStore
{
    [ItemCanBeNull] ValueTask<string> GetDeviceIdAsync();
    ValueTask SetDeviceIdAsync([NotNull] string deviceId);
}
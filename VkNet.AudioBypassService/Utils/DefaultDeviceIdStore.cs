using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Abstractions;

namespace VkNet.AudioBypassService.Utils;

public class DefaultDeviceIdStore : IDeviceIdStore
{
    [CanBeNull] private string _deviceId;
    
    public ValueTask<string> GetDeviceIdAsync()
    {
        return new(_deviceId);
    }

    public ValueTask SetDeviceIdAsync(string deviceId)
    {
        _deviceId = deviceId;
        return ValueTask.CompletedTask;
    }
}
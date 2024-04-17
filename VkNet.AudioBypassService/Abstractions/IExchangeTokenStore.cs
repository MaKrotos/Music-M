using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VkNet.AudioBypassService.Abstractions;

public interface IExchangeTokenStore
{
    [ItemCanBeNull] ValueTask<string> GetExchangeTokenAsync();
    ValueTask SetExchangeTokenAsync([NotNull] string token);
}
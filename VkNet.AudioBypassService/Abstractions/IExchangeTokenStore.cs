using JetBrains.Annotations;
using System.Threading.Tasks;

namespace VkNet.AudioBypassService.Abstractions;

public interface IExchangeTokenStore
{
    [ItemCanBeNull] ValueTask<string> GetExchangeTokenAsync();
    ValueTask SetExchangeTokenAsync([NotNull] string token);
}
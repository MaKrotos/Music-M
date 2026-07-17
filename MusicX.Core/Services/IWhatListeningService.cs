using System.Collections.Generic;
using System.Threading.Tasks;
using MusicX.Core.Models;

namespace MusicX.Core.Services
{
    public interface IWhatListeningService
    {
        Task<List<ListeningItem>> GetWhatListeningAsync();
    }
}
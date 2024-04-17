using System.Net.Http.Json;
using MusicX.Core.Models.Github;
using NLog;

namespace MusicX.Core.Services
{
    public class GithubService
    {
        private readonly Logger logger;

        private readonly HttpClient _client = new()
        {
            BaseAddress = new("https://api.github.com/repos/fooxboy/MusicX-WPF/"),
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new("musicx", "v1")
                }
            }
        };
        public GithubService(Logger logger)
        {
            this.logger = logger;
        }

        public async Task<Release> GetLastRelease()
        {
            try
            {
                return (await _client.GetFromJsonAsync<Release>("releases/latest"))!;
            }
            catch (Exception ex)
            {
                logger.Error("Error in github ");
                logger.Error(ex, ex.Message);

                throw;
            }
        }

        public async Task<Release> GetReleaseByTag(string tag)
        {
            try
            {
                return (await _client.GetFromJsonAsync<Release>($"releases/tags/{tag}"))!;
            }
            catch (Exception ex)
            {
                logger.Error("Error in github ");
                logger.Error(ex, ex.Message);

                throw;
            }

        }
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using MusicX.Core.Models.Genius;

namespace MusicX.Core.Services;

public class GeniusService
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new("https://api.genius.com/")
    };

    public GeniusService()
    {
        _client.DefaultRequestHeaders.Add("X-Genius-Android-Version", "4.2.1");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Genius/4.2.1 (Android; Android 14; Pixel 7 Pro)");
    }

    public async Task<IEnumerable<Hit>> SearchAsync(string query)
    {
        var (_, response) = await _client.GetFromJsonAsync<GeniusResponse<GeniusSearchResponse>>($"search?q={string.Join('+', query.Split(' '))}");

        return response.Hits;
    }

    public async Task<SongFull> GetSongAsync(int id)
    {
        var (_, response) = await _client.GetFromJsonAsync<GeniusResponse<GeniusSongResponse>>($"songs/{id}?text_format=plain");

        return response.Song;
    }
}

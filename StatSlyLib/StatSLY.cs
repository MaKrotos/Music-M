using Newtonsoft.Json;
using StatSlyLib.Models;

namespace StatSlyLib
{
    public class StatSLY
    {
        public static Uri StatSLYUri { get; } = new Uri("https://statsly.ru/");
        public static string Token { get; set; }

        public static void SetToken(string token) {
            Token = token;
        }

        public static async Task SendEvent(Event @event) {


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{StatSLYUri}events");
            request.Headers.Add("token", Token);
            var content = new StringContent(
                JsonConvert.SerializeObject(@event)
                , null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }
    }
}

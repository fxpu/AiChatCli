using System.Net.Http.Json;
using Fxpu.Perplexity.Api.Client;

namespace FxPu.Perplexity.Client
{
    public class PerplexityClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.perplexity.ai";

        public PerplexityClient(string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<PerplexityResponse> SendQueryAsync(Request request)
        {
            var response = await _httpClient.PostAsJsonAsync("/chat/completions", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PerplexityResponse>();
        }
    }
}

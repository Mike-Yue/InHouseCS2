using InHouseCS2.Core.Clients.Contracts;
using System.Net.Http.Json;

namespace InHouseCS2.Core.Clients;

public class MatchParserServiceClient : IMatchParserServiceClient
{
    private readonly HttpClient httpClient;

    public MatchParserServiceClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<MatchParserServiceResponse> SendMatchForParsing(string path, Uri downloadUri, Uri callbackUri)
    {
        var requestUri = new Uri(this.httpClient.BaseAddress!, path);
        var payload = new
        {
            downloadUri,
            callbackUri
        };
        var response = await this.httpClient.PostAsJsonAsync(requestUri, payload);

        if (response.IsSuccessStatusCode)
        {
            return new MatchParserServiceResponse { Success = true };
        }
        else
        {
            return new MatchParserServiceResponse { Success = false, errorReason = response.ReasonPhrase };
        }
    }
}

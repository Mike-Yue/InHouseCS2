namespace InHouseCS2.Core.Clients.Contracts;

public interface IMatchParserServiceClient
{
    public Task<MatchParserServiceResponse> SendMatchForParsing(string path, Uri downloadUri, Uri callbackUri);
}

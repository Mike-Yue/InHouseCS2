namespace InHouseCS2.Core.Clients.Contracts;

public interface IMatchParserServiceClient
{
    public MatchParserServiceResponse SendMatchForParsing(Uri downloadUri, Uri callbackUri, string callbackToken);
}

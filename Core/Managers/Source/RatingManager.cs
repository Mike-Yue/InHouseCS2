using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Ratings.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class RatingManager : IRatingManager
{
    private readonly ITransactionOperation transactionOperation;
    private readonly IRatingCalculator ratingCalculator;
    private readonly ILogger<RatingManager> logger;

    public RatingManager(ITransactionOperation transactionOperation, IRatingCalculator ratingCalculator, ILogger<RatingManager> logger)
    {
        this.transactionOperation = transactionOperation;
        this.ratingCalculator = ratingCalculator;
        this.logger = logger;
    }

    public async Task RecomputeAllRatings()
    {
        var playerEntityStore = this.transactionOperation.GetEntityStore<PlayerEntity, long>();
        var matchEntityStore = this.transactionOperation.GetEntityStore<MatchEntity, int>();
        var newRatings = this.ratingCalculator.RecalculateAllGames(
            players: await playerEntityStore.FindAll((player) => true),
            matches: await matchEntityStore.QueryAsync(q =>
            {
                return q.OrderBy(match => match.DatePlayed).Include(match => match.PlayerMatchStats);
            }));
        foreach(var steamId in newRatings.Keys)
        {
            await playerEntityStore.Update(steamId, (playerEntity) =>
            {
                playerEntity.Rating = newRatings[steamId].Mean;
                playerEntity.Deviation = newRatings[steamId].StandardDeviation;
            });
        }
    }
}

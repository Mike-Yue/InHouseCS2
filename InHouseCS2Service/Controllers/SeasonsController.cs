using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeasonsController : ControllerBase
    {
        private IEntityStore<SeasonEntity> seasonEntityStore;
        private readonly ILogger<MatchesController> logger;

        public SeasonsController(IEntityStore<SeasonEntity> seasonEntityStore, ILogger<MatchesController> logger)
        {
            this.seasonEntityStore = seasonEntityStore;
            this.logger = logger;
        }

        [HttpGet("{seasonId}")]
        public async Task<IActionResult> Get(string seasonId)
        {
            var season = await this.seasonEntityStore.Get(Int32.Parse(seasonId));
            return this.Ok($"Season {season.Name}, {season.StartDate}-{season.EndDate}");
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            await this.seasonEntityStore.Create(() =>
            {
                return new SeasonEntity
                {
                    Name = "1",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 12, 31)
                };
            });
            return this.Ok();
        }

    }
}

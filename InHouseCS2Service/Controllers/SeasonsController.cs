using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeasonsController : ControllerBase
    {
        private ISeasonsManager seasonsManager;
        private readonly ILogger<MatchesController> logger;

        public SeasonsController(ISeasonsManager seasonsManager, ILogger<MatchesController> logger)
        {
            this.seasonsManager = seasonsManager;
            this.logger = logger;
        }

        [HttpGet("{seasonId}")]
        public async Task<IActionResult> Get(string seasonId)
        {
            var seasonLeaderboardData = await this.seasonsManager.GetLeaderboardData(Int32.Parse(seasonId));
            if (seasonLeaderboardData is null)
            {
                return this.NotFound($"Season {seasonId} not found");
            }
            return this.Ok(seasonLeaderboardData);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            await this.seasonsManager.CreateNextSeason();
            return this.Ok();
        }

    }
}

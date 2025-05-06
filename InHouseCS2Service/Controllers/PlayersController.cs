using InHouseCS2.Core.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayersController : ControllerBase
    {
        private IPlayersManager playersManager;
        private readonly ILogger<MatchesController> logger;

        public PlayersController(IPlayersManager playersManager, ILogger<MatchesController> logger)
        {
            this.playersManager = playersManager;
            this.logger = logger;
        }

        [HttpGet("{playerId}")]
        public async Task<IActionResult> Get(string playerId)
        {
            this.logger.LogInformation($"Processing GET request for Player ID {playerId}");
            var output = await this.playersManager.GetOverallPlayerData(Convert.ToInt64(playerId));
            if (output is null)
            {
                return this.NotFound();
            }
            return this.Ok(JsonSerializer.Serialize(output));
        }

        [HttpPost]
        public string Post()
        {
            this.logger.LogWarning("He;pp");
            return "Post!";
        }

    }
}

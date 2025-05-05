using InHouseCS2.Core.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchesController : ControllerBase
    {
        private IMatchesManager matchesManager;
        private readonly ILogger<MatchesController> logger;

        public MatchesController(IMatchesManager matchesManager, ILogger<MatchesController> logger)
        {
            this.matchesManager = matchesManager;
            this.logger = logger;
        }

        [HttpGet("{matchId}")]
        public async Task<IActionResult> Get(string matchId)
        {
            this.logger.LogInformation($"Processing GET request for Match ID {matchId}");
            var output = await this.matchesManager.GetMatchData(matchId);
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

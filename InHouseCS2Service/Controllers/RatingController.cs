using InHouseCS2.Core.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RatingController : ControllerBase
    {
        private IRatingManager ratingManager;
        private readonly ILogger<RatingController> logger;

        public RatingController(IRatingManager ratingManager, ILogger<RatingController> logger)
        {
            this.ratingManager = ratingManager;
            this.logger = logger;
        }

        [HttpPost("recompute")]
        public async Task<IActionResult> Post()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await this.ratingManager.RecomputeAllRatings();
            watch.Stop();
            this.logger.LogInformation($"Recomputing all matches took: {watch.ElapsedMilliseconds}");
            return this.Ok();
        }

    }
}

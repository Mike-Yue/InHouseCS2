using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly ILogger<MatchesController> logger;

        public MatchesController(ILogger<MatchesController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "Matches!";
        }

        [HttpPost]
        public string Post()
        {
            this.logger.LogWarning("He;pp");
            return "Post!";
        }

    }
}

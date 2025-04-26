using InHouseCS2.Core.Managers.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly ILogger<UploadsController> logger;
        private readonly IUploadsManager uploadsManager;

        public UploadsController(IUploadsManager uploadsManager, ILogger<UploadsController> logger)
        {
            this.uploadsManager = uploadsManager;
            this.logger = logger;
        }

        [HttpPost("url")]
        public string PostUrl()
        {
            return "POSTURL";
        }

        [HttpPost("{matchUploadId}")]
        public string PostMatchUploadId(string matchUploadId)
        {
            return matchUploadId;
        }

        [HttpGet("{matchUploadId}")]
        public string GetMatchUploadId(string matchUploadId)
        {
            return matchUploadId;
        }
    }
}
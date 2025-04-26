using InHouseCS2.Core.StorageClients.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly ILogger<UploadsController> logger;
        private readonly IBlobStorageManager blobStorageManager;

        public UploadsController(IBlobStorageManager blobStorageManager, ILogger<UploadsController> logger)
        {
            this.blobStorageManager = blobStorageManager;
            this.logger = logger;
        }

        [HttpPost]
        public string Post()
        {
            this.logger.LogWarning("He;pp");
            return "Post!";
        }
    }
}
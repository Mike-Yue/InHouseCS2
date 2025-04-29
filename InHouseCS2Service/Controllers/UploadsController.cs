using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2Service.Controllers.Models;
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
        public async Task<IActionResult> PostUrl([FromBody] DemoFileMetadata demoFileMetadata)
        {
            if (!string.Equals(demoFileMetadata.FileExtension, "dem", StringComparison.OrdinalIgnoreCase))
            {
                return this.BadRequest("Invalid demo file extension");
            }
            var uploadMetaData = await this.uploadsManager.GetUploadURL(demoFileMetadata.DemoFingerPrint, demoFileMetadata.FileExtension);

            return uploadMetaData is null ? this.BadRequest($"Demo with fingerprint {demoFileMetadata.DemoFingerPrint} already exists in db") : this.Ok(uploadMetaData);
        }

        [HttpPost("notifyUploadStatus")]
        public async Task<IActionResult> PostMatchUploadComplete()
        {
            if (!this.Request.Headers.TryGetValue("media-storage-uri", out var mediaStorageUri))
            {
                return this.BadRequest("Required header value not provided");
            }
            await this.uploadsManager.UpdateMatchStatusAndPersistWork(new Uri(mediaStorageUri!));
            return this.Ok();
        }

        [HttpPost("{matchUploadId}")]
        public string PostMatchUploadId(string matchUploadId)
        {
            return matchUploadId;
        }

        [HttpGet("{matchUploadId}")]
        public async Task<string> GetMatchUploadId(string matchUploadId)
        {
            return await this.uploadsManager.GetMatchUploadStatus(Int32.Parse(matchUploadId));
        }
    }
}
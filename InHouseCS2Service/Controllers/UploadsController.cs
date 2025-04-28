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
        public async Task<IActionResult> PostUrl()
        {
            var form = await this.Request.ReadFormAsync();

            if (!form.TryGetValue("fileName", out var fileName))
            {
                return this.BadRequest("Missing fileName in request body");
            }

            if (!form.TryGetValue("fileExtension", out var fileExtension))
            {
                return this.BadRequest("Missing fileExtension in request body");
            }
            this.logger.LogInformation($"Filename is: {fileName}, FileExtension is {fileExtension}");

            return this.Ok(this.uploadsManager.GetUploadURL(fileName!, fileExtension!));
        }

        [HttpPost("{matchUploadId}/notifyUploadStatus")]
        public async Task<IActionResult> PostMatchUploadComplete(string matchUploadId)
        {
            await this.uploadsManager.SetMatchUploadStatusToUploaded(Int32.Parse(matchUploadId));
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
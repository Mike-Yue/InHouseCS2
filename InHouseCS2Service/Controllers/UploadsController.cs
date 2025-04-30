using AutoMapper;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Models;
using InHouseCS2Service.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InHouseCS2Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly ILogger<UploadsController> logger;
        private readonly IUploadsManager uploadsManager;
        private readonly IMapper mapper;

        public UploadsController(IUploadsManager uploadsManager, IMapper mapper, ILogger<UploadsController> logger)
        {
            this.uploadsManager = uploadsManager;
            this.mapper = mapper;
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
        public async Task<IActionResult> PostMatchUploadId(string matchUploadId, [FromBody] MatchDataObject matchDataObject)
        {
            var output = this.mapper.Map<CoreMatchDataRecord>(matchDataObject);
            await this.uploadsManager.FinalizeMatchUploadEntityAndRecordData(output);
            return this.Ok();
        }

        [HttpGet("{matchUploadId}")]
        public async Task<string> GetMatchUploadId(string matchUploadId)
        {
            return await this.uploadsManager.GetMatchUploadStatus(Int32.Parse(matchUploadId));
        }
    }
}
using AutoMapper;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
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
            var uploadMetaData = await this.uploadsManager.GetUploadURL(demoFileMetadata.DemoFingerPrint, demoFileMetadata.MatchPlayedAt);

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
        public async Task<IActionResult> PostMatchUploadId(string matchUploadId, [FromBody] MatchDataWrapper matchDataWrapper)
        {

            var output = this.mapper.Map<CoreMatchDataWrapperRecord>(matchDataWrapper);
            await this.uploadsManager.FinalizeMatchUploadEntityAndRecordData(Int32.Parse(matchUploadId), output);
            return this.Ok();
        }

        [HttpGet("{matchUploadId}")]
        public async Task<IActionResult> GetMatchUploadId(string matchUploadId)
        {
            var status = await this.uploadsManager.GetMatchUploadStatus(Int32.Parse(matchUploadId));
            if (status is null)
            {
                return this.NotFound($"Match upload with id {matchUploadId} not found");
            }
            return this.Ok(status);
        }
    }
}
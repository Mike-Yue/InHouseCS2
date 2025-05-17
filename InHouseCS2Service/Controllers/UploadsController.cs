using AutoMapper;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2Service.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;

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
            BinaryData events = await BinaryData.FromStreamAsync(this.Request.Body);
            EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);
            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                // Handle system events
                if (eventGridEvent.TryGetSystemEventData(out object eventData))
                {
                    // Handle the subscription validation event
                    if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                    {
                        this.logger.LogInformation($"Got SubscriptionValidation event data, validation code: {subscriptionValidationEventData.ValidationCode}, topic: {eventGridEvent.Topic}");
                        // Do any additional validation (as required) and then return back the below response
                        var responseData = new
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };

                        return new OkObjectResult(responseData);
                    }
                    else if (eventData is StorageBlobCreatedEventData storageBlobCreatedEventData)
                    {
                        var data = eventGridEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();
                        this.logger.LogWarning($"Event url is: {data.Url}");
                        await this.uploadsManager.UpdateMatchStatusAndPersistWork(new Uri(data.Url));
                        return this.Ok();
                    }
                    else
                    {
                        return this.BadRequest("Unhandled event type.");
                    }
                }
            }
            return this.BadRequest("Unhandled event type.");
        }

        [HttpPost("{matchUploadId}")]
        public async Task<IActionResult> PostMatchUploadId(string matchUploadId, [FromBody] MatchDataWrapper matchDataWrapper)
        {
            await this.uploadsManager.FinalizeMatchUploadEntityAndRecordData(Int32.Parse(matchUploadId), matchDataWrapper);
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
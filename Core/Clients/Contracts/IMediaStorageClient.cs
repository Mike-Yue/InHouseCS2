﻿namespace InHouseCS2.Core.Clients.Contracts;

public interface IMediaStorageClient
{
    public MediaUploadInfo GetUploadUrl(string fileExtension, double hoursValidFor);

    public Uri GetDownloadUrl(string fileUri, double hoursValidFor);
}

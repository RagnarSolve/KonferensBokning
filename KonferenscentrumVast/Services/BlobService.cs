using Azure.Storage;
using Azure.Storage.Blobs;
public class BlobService
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly string _containerName;
  private readonly ILogger<BlobService> _logger;


  public BlobService(BlobServiceClient blobServiceClient, string containerName, ILogger<BlobService> logger)
  {
    _blobServiceClient = blobServiceClient;
    _containerName = containerName;
    _logger = logger;
  }

  public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
  {
    BlobResponseDto response = new BlobResponseDto();
    _logger.LogInformation("Trying to upload file");
    try
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
      BlobClient client = containerClient.GetBlobClient(blob.FileName);

      await using (Stream? data = blob.OpenReadStream())
      {
        await client.UploadAsync(data);
      }
      
      _logger.LogInformation("Uploaded {FileName} to URI {BlobUri}", blob.FileName, client.Uri.AbsoluteUri);

      response.Status = $"File {blob.FileName} uploaded successfully";
      response.Error = false;
      response.Blob.Uri = client.Uri.AbsoluteUri;
      response.Blob.Name = client.Name;

      return response;
    }
    catch (Exception e)
    {
      _logger.LogError("Critical error when trying to upload file", e.Message);
      response.Status = $"Upload failed for file {blob.FileName}. See logs for details.";
      response.Error = true;

      return response;
    }
  }
}
using Azure.Storage;
using Azure.Storage.Blobs;
public class BlobService
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly string _containerName;


  public BlobService(BlobServiceClient blobServiceClient, string containerName)
  {
    _blobServiceClient = blobServiceClient;
    _containerName = containerName;
  }

  public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
  {
    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

    BlobResponseDto response = new BlobResponseDto();
    BlobClient client = containerClient.GetBlobClient(blob.FileName);

    await using (Stream? data = blob.OpenReadStream())
    {
      await client.UploadAsync(data);
    }


    response.Status = $"File {blob.FileName} uploaded successfully";
    response.Error = false;
    response.Blob.Uri = client.Uri.AbsoluteUri;
    response.Blob.Name = client.Name;

    return response;
  }
}
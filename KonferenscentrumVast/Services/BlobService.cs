using Azure.Storage;
using Azure.Storage.Blobs;
public class BlobService
{
  private readonly string _storageAccount = "konferensstorage";
  private readonly string _key = "OAxfJeslZ3e1KF9PoQ64EKpaU3grC38IDGUVB5sEiEtbIvmlBueA5XvuC9aa8XVT4WotRfKXNHm2+ASte+YZ6g==";
  private readonly BlobContainerClient _filesContainer;

  public BlobService()
  {
    var credential = new StorageSharedKeyCredential(_storageAccount, _key);
    var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
    var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
    _filesContainer = blobServiceClient.GetBlobContainerClient("storage-container");
  }

  public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
  {
    BlobResponseDto response = new BlobResponseDto();
    BlobClient client = _filesContainer.GetBlobClient(blob.FileName);

    await using (Stream? data = blob.OpenReadStream())
    {
      await client.UploadAsync(data);
    }

    // if (!response.Error == false)
    // {
    //   response.Status = $"File {blob.FileName} could not upload";
    // }

    response.Status = $"File {blob.FileName} uploaded successfully";
    response.Error = false;
    response.Blob.Uri = client.Uri.AbsoluteUri;
    response.Blob.Name = client.Name;

    return response;
  }
}
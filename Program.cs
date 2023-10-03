using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

            var endpointUri = config["blobEndpointUri"];
            var account = config["account"];
            var key = config["key"];
            var containerName = config["container"];

            Console.WriteLine("Starting Test");

            var client = new BlobServiceClient(new Uri(endpointUri), new StorageSharedKeyCredential(account, key));

            var containerClient = client.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            await Upload(containerClient, 10L * 1024);
            await Upload(containerClient, 50L * 1024);
            await Upload(containerClient, 512L * 1024);
            await Upload(containerClient, 512L * 1024 * 1024, 10);

            var filesConnStr = config["filesConnStr"];
            var sharename = config["sharename"];



            ShareServiceClient shareserviceclient = new ShareServiceClient(filesConnStr);
            ShareClient shareclient = shareserviceclient.GetShareClient(sharename);
            ShareDirectoryClient sharedirectoryclient = shareclient.GetDirectoryClient("test");

            await sharedirectoryclient.CreateIfNotExistsAsync();


            await Upload(sharedirectoryclient, 10L * 1024);
            await Upload(sharedirectoryclient, 50L * 1024);
            await Upload(sharedirectoryclient, 512L * 1024);
            await Upload(sharedirectoryclient, 512L * 1024 * 1024, 10);

        }

        private static Stream CreateDummyFile(long length)
        {
            var ms = new MemoryStream();
            ms.SetLength(length);

            return ms;
        }

        private async static Task Upload(BlobContainerClient client, long length, int iterations = 2000)
        {
            var file = CreateDummyFile(length); //10k
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                file.Seek(0, SeekOrigin.Begin);
                await client.UploadBlobAsync($"{Guid.NewGuid()}.txt".ToString(), file);

            }
            timer.Stop();

            Console.WriteLine($"Time Taken: {timer.Elapsed.TotalSeconds} - Size : {length} - Iterations : {iterations}");
        }

        private async static Task Upload(ShareDirectoryClient client, long length, int iterations = 2000)
        {
            var file = CreateDummyFile(length); //10k
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                file.Seek(0, SeekOrigin.Begin);

                var fileClient = client.GetFileClient($"{Guid.NewGuid()}.txt".ToString());
                await fileClient.CreateAsync(length);
                fileClient.Upload(file);

            }
            timer.Stop();

            Console.WriteLine($"Time Taken: {timer.Elapsed.TotalSeconds} - Size : {length} - Iterations : {iterations}");
        }

    }
}

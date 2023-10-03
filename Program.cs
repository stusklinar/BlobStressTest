using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.IO;

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

            var client = new BlobServiceClient(new Uri(endpointUri),new StorageSharedKeyCredential(account,key));


            var containerClient = client.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            await Upload(containerClient, 10L * 1024);
            await Upload(containerClient, 50L * 1024);
            await Upload(containerClient, 512L * 1024);
            //await Upload(containerClient, 512L * 1024 *1024);

            await containerClient.DeleteAsync();

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


    }
}

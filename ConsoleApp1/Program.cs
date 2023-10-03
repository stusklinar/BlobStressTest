using Azure.Identity;
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
            var containerName = config["container"];

            Console.WriteLine("Starting Test");

            var client = new BlobServiceClient(new Uri(endpointUri), new DefaultAzureCredential());


            var containerClient = client.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            await Upload(containerClient, 10L * 1024);
            //await Upload(containerClient, 50L * 1024);
            //await Upload(containerClient, 512L * 1024);
            //await Upload(containerClient, 512L * 1024 *1024);


        }


        private static Stream CreateDummyFile(long length)
        {
            var ms = new MemoryStream();
            ms.SetLength(length);

            return ms;
        }

        private async static Task Upload(BlobContainerClient client, long length, int iterations = 10)
        {
            var file = CreateDummyFile(length); //10k
            for (int i = 0; i < iterations; i++)
            {
                var timer = Stopwatch.StartNew();
                await client.UploadBlobAsync(Guid.NewGuid().ToString(), file);
                timer.Stop();

                Console.WriteLine($"Time Taken: {timer.Elapsed.TotalSeconds} - Size : {length}");
            }
        }


    }
}

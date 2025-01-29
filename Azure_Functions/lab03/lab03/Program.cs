using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Configuration;

namespace lab03
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string storageAccountName = ConfigurationManager.AppSettings[name: "storageAccountName"];
            string blobServiceEndpointTemplate = ConfigurationManager.AppSettings[name: "blobServiceEndpoint"];
            string blobServiceEndpoint = string.Format(blobServiceEndpointTemplate, storageAccountName);
            string storageAccountKey = ConfigurationManager.AppSettings[name: "storageAccountKey"];

            StorageSharedKeyCredential accountCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

            BlobServiceClient serviceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), accountCredentials);

            AccountInfo info = await serviceClient.GetAccountInfoAsync();

            await Console.Out.WriteLineAsync($"Connectado a \t{storageAccountName}.");
            await Console.Out.WriteLineAsync($"Informações: \t{info?.AccountKind}");
            await Console.Out.WriteLineAsync($"SKU: \t{info?.SkuName}\n");

            var result = await GetContainerAsync(serviceClient, "novo-conteiner-001");

            await EnumerateContainersAsync(serviceClient);

            var blob = await GetBlobAsync(result, "IMG-20240323-WA0018.jpg");
        }

        private static async Task EnumerateContainersAsync(BlobServiceClient client)
        {
            await foreach (BlobContainerItem container in client.GetBlobContainersAsync())
            {
                await Console.Out.WriteLineAsync($"Container: \t{container.Name}");

                await EnumerateBlobAsync(client, container.Name);
            }
        }

        private static async Task EnumerateBlobAsync(BlobServiceClient client, string containerName)
        {
            BlobContainerClient container = client.GetBlobContainerClient(containerName);

            await Console.Out.WriteLineAsync($"Blob: \t{container.Name}...");

            await foreach (BlobItem blob in container.GetBlobsAsync())
            {
                await Console.Out.WriteLineAsync($"Arquivos: \t{blob.Name}");
            }
            Console.WriteLine();
        }

        private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient client, string containerName)
        {
            BlobContainerClient container = client.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            await Console.Out.WriteLineAsync($"Novo contêiner: \t{container.Name}");

            return container;
        }

        private static async Task<BlobClient> GetBlobAsync(BlobContainerClient client, string blobName)
        {
            BlobClient blob = client.GetBlobClient(blobName);
            bool exists = await blob.ExistsAsync();

            if (!exists)
            {
                await Console.Out.WriteLineAsync($"Blob {blob.Name} não encontrado.");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Blob {blob.Name} existe! URI: \t{blob.Uri}.");
            }
            return blob;
        }
    }
}


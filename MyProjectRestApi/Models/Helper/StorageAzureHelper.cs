using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MyProjectRestApi.Models.Helper
{
    public static class StorageAzureHelper
    {
        public static AzureStorageConfig _storageConfig = new AzureStorageConfig()
        {
            AccountKey = "klrTtnLSDv/5SIgrfbKaiAHZkZyZg5Pj7rSYoAHAikn106uBusJDQ+1FeOSwtC5YEXKSFsjCyreHaZL05XzqUg==",
            AccountName = "blobstoragetr1",
            ImageContainer = "images"
        };

        //public static bool IsImage(IFormFile file)
        //{
        //    if (file.ContentType.Contains("image"))
        //    {
        //        return true;
        //    }

        //    string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

        //    return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        //}


        public static async Task<string> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (AzureStorageConfig class)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (AzureStorageConfig class)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            try
            {
                // Upload the file
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
            catch (Exception ex) { }

            string imageUrl = blockBlob.Uri.AbsoluteUri;

            return await Task.FromResult(imageUrl);
        }

        public static async Task<bool> DeleteFileToStorage( string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            try
            {
                // Delete the file
                await blockBlob.DeleteAsync();
            }
            catch (Exception ex) { return false;  }

            return true;
        }

    }
}
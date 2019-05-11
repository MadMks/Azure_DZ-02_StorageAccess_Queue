using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"UseDevelopmentStorage=true;");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("container1");

            Console.WriteLine("Downloading blobs...");
            foreach (IListBlobItem blob in container.ListBlobs())
            {
                string blobName = Uri.UnescapeDataString(Path.GetFileName(blob.Uri.AbsolutePath));
                var reference = container.GetBlobReferenceFromServer(blobName);
                reference.DownloadToFile(blobName, FileMode.Create);
                Console.WriteLine(blobName + "- saved");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}

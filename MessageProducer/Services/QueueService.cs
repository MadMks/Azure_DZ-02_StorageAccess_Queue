using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MessageProducer.Services
{
    public class QueueService
    {
        public CloudQueue Queue { get; set; }

        public QueueService()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"UseDevelopmentStorage=true;");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            Queue = queueClient.GetQueueReference("queue");
            Queue.CreateIfNotExistsAsync().Wait();
        }
    }
}

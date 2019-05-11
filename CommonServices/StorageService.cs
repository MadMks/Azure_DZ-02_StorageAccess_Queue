using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if CONSUMER // Директива компиляции,
namespace MessageConsumer.Services
#elif QUERY // если присутствует в свойствах проекта на вкладке Build
namespace TableSearch.Services
#endif // в поле Conditional compilation symbols, тогда true, иначе false
{
    public class StorageService
    {
#if CONSUMER // Этот код попадёт в компиляцию если true
        public CloudQueue Queue { get; }
#endif
        public CloudTable Table { get; }

        public StorageService()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"UseDevelopmentStorage=true;");
#if CONSUMER
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            Queue = queueClient.GetQueueReference("queue");
            var queueCreationTask = Queue.CreateIfNotExistsAsync();
#endif

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Table = tableClient.GetTableReference("table");
            var tableCreationTask = Table.CreateIfNotExistsAsync();

            Task.WaitAll(
#if CONSUMER
                queueCreationTask,
#endif
                tableCreationTask);

            ConditionalMethod(); // Чтобы обойтись без директивы компиляции
        }

        // Воспользуемся атрибутом
        [System.Diagnostics.Conditional("CONSUMER")]
        private static void ConditionalMethod()
        {
            Console.WriteLine("Conditional method");
        }
    }
}

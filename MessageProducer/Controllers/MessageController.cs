using System;
using System.Linq;
using System.Threading.Tasks;
using MessageProducer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MessageProducer.Controllers
{
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        private readonly QueueService queueService;

        public MessageController(
            // Получим службу работы с очередью из контейнера внедрения зависимостей
            QueueService queueService)
        {
            this.queueService = queueService;
        }

        [HttpGet]
        public string Get() => "Running";

        // POST api/message
        [HttpPost]
        public async Task Post(
            // Если не знаете что такое dynamic, читайте книгу
            // 50 способов улучшить свой C#, издание 3, потом 2
            [FromBody] dynamic value)
        {
            var message = new CloudQueueMessage(value.ToString());
            await queueService.Queue.AddMessageAsync(message);
        }
    }
}

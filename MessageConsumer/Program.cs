using MessageConsumer.Services;
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;

namespace MessageConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Если запустить так мы не сможем остановить приложение клавишей
            //await ProcessQueueAsync();
            Task.Factory.StartNew(
                () => ProcessQueueAsync(),
                TaskCreationOptions.LongRunning); // Чтобы не занимать поток из ThreadPool
            // Ждём нажатия любой клавиши
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task ProcessQueueAsync()
        {
            StorageService storageService = new StorageService();
            var queue = storageService.Queue;
            var table = storageService.Table;

            Console.WriteLine("Listening to a queue changes");
            while (true) // Уходим в бесконечный цикл проверки наличия сообщений
            {
                var message = await queue.GetMessageAsync();
                if (message != null) // Есть сообщение
                {
                    string messageString = message.AsString;
                    if (message.DequeueCount > 2)
                    {
                        var badMessage = new BadMessage
                        {
                            PartitionKey = "BadMessages",
                            RowKey = Guid.NewGuid().ToString(),
                            Text = messageString
                        };
                        var insertOperation = TableOperation.Insert(badMessage);

                        await table.ExecuteAsync(insertOperation);

                        await queue.DeleteMessageAsync(message);
                        Console.WriteLine($"Message \"{messageString}\" deleted");
                    }
                    else
                    {
                        try
                        {
                            // Попробуем десериализовать сообщение
                            // в объект объявленный ниже
                            var requestMessage =
                                JsonConvert.
                                DeserializeObject<ServiceRequestMessage>
                                (messageString);


                            Console.WriteLine($"Broken item: {requestMessage.ItemName}" +
                                $"{Environment.NewLine}Problem description: " +
                                $"{requestMessage.Problem}" + Environment.NewLine);
                            // Десериализация прошла успешно
                            // Удалим сообщение из очереди
                            await queue.DeleteMessageAsync(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Exception] Tried to process bad message\n");
                            // Десериализация прошла не успешно
                            // Сообщение вернётся в очередь само через 5 минут
                        }
                    }
                }
                else // Нет сообщения, подождём 10 сек
                    await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        //private static ServiceRequestMessage DeserializationOfObjects(string messageString)
        //{
        //    Console.WriteLine("---");
        //    Console.WriteLine(messageString);
        //    if (messageString.StartsWith("<?xml"))
        //    {
        //        XmlDocument xml = new XmlDocument();
        //        xml.LoadXml(messageString);

        //        XElement xElement;
        //        using (var reader = new XmlNodeReader(xml))
        //        {
        //            xElement = XElement.Load(reader);
        //        }

        //        messageString = JsonConvert.SerializeXNode(xElement);

        //        Console.WriteLine("Конвертирование из xml в json:");
        //        Console.WriteLine(messageString);
        //    }
            
            
        //    return JsonConvert.
        //        DeserializeObject<ServiceRequestMessage>(messageString);
        //}
    }

    /// <summary>
    /// Описывает запись о проблеме с некоторым устройством
    /// </summary>
    public class ServiceRequestMessage
    {
        /// <summary>Название устройства</summary>
        public string ItemName { get; set; }
        /// <summary>Описание проблемы</summary>
        public string Problem { get; set; }
    }
}

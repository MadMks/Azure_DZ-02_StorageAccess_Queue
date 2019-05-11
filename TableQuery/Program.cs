using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using TableSearch.Services;

namespace TableSearch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var table = new StorageService().Table;

            // Создадим запрос к таблице
            var query = new TableQuery<BadMessage>()
            {
                TakeCount = 5 // Будем читать по 5 записей
            }
            .Where(// Добавим фильтр
                TableQuery.CombineFilters(// Скомбинируем 2 фильтра
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", // Нужно отфильтровать по ключу
                        QueryComparisons.Equal,
                        "BadMessages"), // Нам нужны только эти записи
                    TableOperators.And,
                    TableQuery.CombineFilters(// Скомбинируем ещё 2 фильтра
                        TableQuery.GenerateFilterCondition("Text", QueryComparisons.GreaterThanOrEqual, "A"), // Текст начинается на
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("Text", QueryComparisons.LessThan, "C")))); // Текст зананчивается на букву перед

            TableContinuationToken token = null; // Если не null, там ещё записи есть

            TableQuerySegment<BadMessage> seg;
            do
            {
                // Считаем порцию записей 5 штук
                seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken; // Если не null, там ещё записи есть
                // Пройдёмся по 5 записям по очереди
                foreach (var badMessage in seg.Results)
                {
                    Console.WriteLine(badMessage.Text);
                }
            } while (token != null); // Если null, мы всё прочитали

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}

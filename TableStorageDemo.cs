using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using AzureDemo.Functions;

namespace AzureDemo.Functions
{
    public static class TableStorageDemo
    {
        /// <summary>
        /// Receive data in an http request and then save it to Table Storage and queue.
        /// http trigger with queue and table output binding
        /// </summary>
        /// <param name="req"></param>
        /// <param name="todoitm"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("TableStorageDemo")]
        [return:Queue("demoq")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("demotable",Connection ="")]IAsyncCollector<TodoItem> todoitm,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TodoItem data = JsonConvert.DeserializeObject<Todo>(requestBody);

            await todoitm.AddAsync(data);
           

            return new OkObjectResult(data);
        }



        [FunctionName("QueueToTable")]
        [return: Table("demotable")]
        public static TodoItem QueueToTable(
            [QueueTrigger("demoq")] Todo data,
            ILogger logger
            )
        {
             TodoItem item = data;
            return item;
        }


     


    }
}

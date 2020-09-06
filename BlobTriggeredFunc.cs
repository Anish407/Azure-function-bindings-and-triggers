using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using AzureDemo.Functions;

namespace AzureDemo.Functions
{
    public static class BlobTriggeredFunc
    {
        /// <summary>
        /// Read From Blob to upload to queue
        /// and write it to table storage
        /// </summary>
        /// <param name="myBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("BlobTriggeredFunc")]
        [return:Queue("fromblob", Connection = "AzureWebJobsStorage")]
        public static async Task<string> Run(
            [BlobTrigger("democont/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
            [Table("demotable", Connection = "")] IAsyncCollector<TodoItem> todoitm,
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var data = await new StreamReader(myBlob).ReadToEndAsync();
            await todoitm.AddAsync(new TodoItem { RowKey = Guid.NewGuid().ToString(), PartitionKey = "demo", Name = data });

            return data;
        }
    }
}

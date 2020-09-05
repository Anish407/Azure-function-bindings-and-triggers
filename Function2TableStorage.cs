using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace AzureFunctions.Demo
{
    public static class Function2TableStorage
    {
        [FunctionName("CreateStudent1")]
        public static async Task<IActionResult> CreateStudent1(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Student")] HttpRequest req,
            [Table("Student", Connection = "tableStorageConnection")] IAsyncCollector<StudentTableEntity> student,
            ILogger log)
        {
            log.LogInformation("Add to Table storage");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            StudentTableEntity data = JsonConvert.DeserializeObject<StudentTableEntity>(requestBody);

            try
            {
                // Add to DB, instead just add to static list
                data.StudId = Guid.NewGuid();
                await student.AddAsync(data);
                return new OkObjectResult(data);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

        }

        [FunctionName("GetStudents")]
        public static async Task<IActionResult> GetStudents(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllStudents")] HttpRequest httpRequest,
            [Table(tableName: "Student", Connection = "tableStorageConnection")] CloudTable table,
            ILogger log
            )
        {
            try
            {
                var query = new TableQuery<StudentTableEntity>();
                var segment = await table.ExecuteQuerySegmentedAsync(query, null);

                return new OkObjectResult(segment.ToList());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }

        /// <summary>
        /// We pass the row key and partition key to the table binding and it returns the 
        /// matching table entity, so we dont have to write the code to 
        /// retreive the tableEntity.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="studentTableEntity"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [FunctionName("GetStudentById")]
        public static IActionResult GetStudentById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetStudentById/{id}/{key}")]HttpRequest httpRequest,
            [Table(tableName: "Student", partitionKey: "{key}", rowKey: "{id}", Connection = "tableStorageConnection")]StudentTableEntity studentTableEntity,
            string id,
            string key

          )
        {
            try
            {
                if (studentTableEntity == null) return new NotFoundObjectResult(studentTableEntity);

                return new OkObjectResult(studentTableEntity);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }

        //[FunctionName("UpdateStudents")]
        //public static async Task<IActionResult> UpdateStudents(
        //[HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "GetAllStudents/{rowkey}/{pkey}")] HttpRequest httpRequest,
        //[Table(tableName: "Student", partitionKey: "{pkey}", rowKey: "{rowkey}", Connection = "tableStorageConnection")] CloudTable table,
        //string rowkey,
        //string pkey,
        //ILogger log
        //)
        //{
        //    try
        //    {
        //        // if this doesnt work then just pass the id and write table queries to get the existing record
        //        // and overwrite the existing one, instead of passing the row key and primary key to the table binding
        //        //Get data from table  // also complete delete
        //        var query = new TableQuery<StudentTableEntity>();
        //        var segment = await table.ExecuteQuerySegmentedAsync(query, null);

        //        return new OkObjectResult(segment.ToList());
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BadRequestObjectResult(ex.Message);
        //    }

        //}

        //OR

        [FunctionName("DeleteStudent")]
        public static async Task<IActionResult> DeleteStudent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "DeleteStudent/{rowkey}/{pkey}")] HttpRequest httpRequest,
            [Table(tableName: "Student", Connection = "tableStorageConnection")] CloudTable table,
            string rowkey,
            string pkey,
            ILogger log
            )
        {
            try
            {
                // Etgs as * will ignore the concurrency issues 
                var toDeleteOperation = TableOperation.Delete(new TableEntity { PartitionKey = pkey, RowKey = rowkey, ETag = "*" });
                try
                {
                    var res = await table.ExecuteAsync(toDeleteOperation);
                    return new OkResult();
                }
                catch (Exception ex)
                {
                    return new BadRequestObjectResult(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }


    }

    public class StudentTableEntity : TableEntity
    {
        public Guid StudId { get; set; }
        public string StudName { get; set; }
        public string Address { get; set; }
    }
}

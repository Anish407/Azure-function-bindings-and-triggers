using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace AzureFunctions.Demo
{
    public static class StudentApi
    {
        static List<Student> Students = new List<Student>();
      
        [FunctionName("CreateStudent")]
        public static async Task<IActionResult> CreateStudent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Student")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Student data = JsonConvert.DeserializeObject<Student>(requestBody);

            try
            {
                // Add to DB, instead just add to static list
                data.StudId =  Guid.NewGuid();
                Students.Add(data);
                return new OkObjectResult(data);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

        }

        [FunctionName("GetStudent")]
        public static IActionResult GetStudent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Student")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get Student list");

            try
            {
                return new OkObjectResult(Students);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

        }

        [FunctionName("GetStudentById")]
        public static IActionResult GetStudentById(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Student/{id}")] HttpRequest req,
       ILogger log,Guid id)
        {
            log.LogInformation($"Get Student {id}");

            try
            {
                var student = Students.FirstOrDefault(i => i.StudId == id);
                if (student == null) return new NotFoundObjectResult(student);
              
                return new OkObjectResult(student);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

        }

    }

    internal class Student
    {
        public Guid StudId { get; set; }
        public string StudName { get; set; }
        public string Address { get; set; }
    }
}

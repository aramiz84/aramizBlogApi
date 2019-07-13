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
using Microsoft.WindowsAzure.Storage;

namespace Aramiz
{
    public static class AramizApi
    {
        [FunctionName("CreateBlogPost")]
        public static async Task<IActionResult> CreateBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blog")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] IAsyncCollector<BlogEntity> blogTable,
            ILogger log)
        {
            log.LogInformation("Creating new aramiz blogpost entity");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<BlogCreateModel>(requestBody);
            var blogentity = new Blog()
            {
                BlogSubject = input.BlogSubject,
                BlogCategory = input.BlogCategory,
                BlogBody = input.BlogBody
            };

            await blogTable.AddAsync(blogentity.BlogTableEntity());

            return new OkObjectResult(blogentity);
        }

        [FunctionName("GetBlogPosts")]
        public static async Task<IActionResult> GetBlogPosts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blog")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] CloudTable blogTable,
            ILogger log)
        {
            log.LogInformation("Getting aramiz blogpost entities");
            var query = new TableQuery<BlogEntity>();
            var segment = await blogTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.BlogEntity));
        }

        [FunctionName("GetBlogPostsById")]
        public static IActionResult GetBlogPostsById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blog/{blogid}")] HttpRequest req,
            [Table("aramizblog", "blog", "{blogid}", Connection = "AzureWebJobsStorage")] BlogEntity blogTable,
            ILogger log, string blogid)
        {
            log.LogInformation("Getting aramiz blogpost by id");
            if(blogTable == null)
            {
                log.LogInformation($"BlogEntity {blogid} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(blogTable.BlogEntity());
        }

        [FunctionName("UpdateBlogPost")]
        public static async Task<IActionResult> UpdateBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "blog/{blogid}")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] CloudTable blogTable,
            ILogger log, string blogid)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<BlogUpdateModel>(requestBody);
            var findOpertation = TableOperation.Retrieve<BlogEntity>("blog", blogid);
            var findResult = await blogTable.ExecuteAsync(findOpertation);

            if(findResult == null)
            {
                return new NotFoundResult();
            }

            var existingRow = (BlogEntity)findResult.Result;

            if(!string.IsNullOrEmpty(updated.BlogCategory) && 
                !string.IsNullOrEmpty(updated.BlogSubject) &&
                !string.IsNullOrEmpty(updated.BlogBody))
            {
                log.LogInformation($"Updating blogpost: {blogid} successfully");
                existingRow.BlogCategory = updated.BlogCategory;
                existingRow.BlogSubject = updated.BlogSubject;
                existingRow.BlogBody = updated.BlogBody;
                existingRow.BlogModifiedDate = updated.BlogModifiedDate;
            }
            var replaceOperation = TableOperation.Replace(existingRow);
            await blogTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.BlogEntity());
        }

        [FunctionName("DeleteBlogPost")]
        public static async Task<IActionResult> DeleteBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "blog/{blogid}")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] CloudTable blogTable,
            ILogger log, string blogid)
        {

            var deleteOperation = TableOperation.Delete(new TableEntity()
            { PartitionKey = "blog", RowKey = blogid, ETag = "*" });

            try
            {
                var deleteResult = await blogTable.ExecuteAsync(deleteOperation);
                log.LogInformation($"Deleted blogpost: {blogid} successfully");
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                log.LogInformation($"Something went wrong when trying to delete blogpost {blogid}");
                log.LogInformation($"Source: {e.Source} Exception Message: {e.Message}");
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}

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
using System.Security.Claims;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace Aramiz
{
    public static class AramizApi
    {
        // ### Aramiz BLOG API ###
        #region CreateBlogPost
        [FunctionName("CreateBlogPost")]
        public static async Task<IActionResult> CreateBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blog")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] IAsyncCollector<BlogEntity> blogTable,
            ILogger log)
        {
            log.LogInformation("Creating new aramiz blogpost entity");

            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

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
        #endregion
        #region GetBlogPost
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
        #endregion
        #region GetBlogPostsById
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
        #endregion
        #region UpdateBlogPost
        [FunctionName("UpdateBlogPost")]
        public static async Task<IActionResult> UpdateBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "blog/{blogid}")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] CloudTable blogTable,
            ILogger log, string blogid)
        {
            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if ((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

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
        #endregion
        #region DeleteBlogPost
        [FunctionName("DeleteBlogPost")]
        public static async Task<IActionResult> DeleteBlogPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "blog/{blogid}")] HttpRequest req,
            [Table("aramizblog", Connection = "AzureWebJobsStorage")] CloudTable blogTable,
            ILogger log, string blogid)
        {
            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if ((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

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
        #endregion

        // ### Aramiz Experience API ###
        #region CreateExperince
        [FunctionName("CreateExperince")]
        public static async Task<IActionResult> CreateExperience(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "experience")] HttpRequest req,
            [Table("aramizexperience", Connection = "AzureWebJobsStorage")] IAsyncCollector<ExperienceEntity> exeperinceTable,
            ILogger log)
        {
            log.LogInformation("Creating new aramiz experience entity");

            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if ((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ExperienceCreateModel>(requestBody);

            var experienceEntity = new ExperienceModel
            {
                expEmployeeTitle = input.expEmployeeTitle,
                expCompanyLogoUri = input.expCompanyLogoUri,
                expCompany = input.expCompany,
                expLocation = input.expLocation,
                expCurrentWorkRole = input.expCurrentWorkRole,
                expStartDate = input.expStartDate,
                expEndDate = input.expEndDate,
                expWorkSubject = input.expWorkSubject,
                expWorkDescription = input.expWorkDescription,
                expWorkLinks = input.expWorkLinks
            };

            await exeperinceTable.AddAsync(experienceEntity.ExperienceTableEntity());

            return new OkObjectResult(experienceEntity);
         }

        [FunctionName("GetExperience")]
        public static async Task<IActionResult> GetExperience(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "experience")] HttpRequest req,
            [Table("aramizexperience", Connection = "AzureWebJobsStorage")] CloudTable experienceTable,
            ILogger log)
        {
            log.LogInformation("Getting aramiz experience entities");

            var query = new TableQuery<ExperienceEntity>();
            var segment = await experienceTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(ExpericenceMappings.ExperienceEntity));
        }
        #endregion
        #region GetExperienceById
        [FunctionName("GetExperienceById")]
        public static IActionResult GetExperienceById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "experience/{expId}")] HttpRequest req,
            [Table("aramizexperience", "experience", "{expId}", Connection = "AzureWebJobsStorage")] ExperienceEntity experienceTable,
            ILogger log, string expId)
        {
            log.LogInformation("Getting aramiz experience entity by id");

            if(experienceTable == null)
            {
                log.LogInformation($"Experience entity {expId} not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(experienceTable.ExperienceEntity());
        }
        #endregion GetExperienceById
        #region UpdateExperienceById
        [FunctionName("UpdateExperience")]
        public static async Task<IActionResult> UpdateExperience(
             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "experience/{expId}")] HttpRequest req,
             [Table("aramizexperience", Connection = "AzureWebJobsStorage")] CloudTable experienceTable,
             ILogger log, string expId)
        {
            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if ((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ExperienceUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ExperienceEntity>("experience", expId);
            var findResult = await experienceTable.ExecuteAsync(findOperation);

            if(findResult == null)
            {
                return new NotFoundResult();
            }

            var existingRow = (ExperienceEntity)findResult.Result;

            /*if(!string.IsNullOrEmpty(updated.expCompany) &&
               !string.IsNullOrEmpty(updated.expCompanyLogoUri) &&
               !string.IsNullOrEmpty(updated.expEmployeeTitle) &&
               !string.IsNullOrEmpty(updated.expLocation) &&
               !string.IsNullOrEmpty(updated.expWorkDescription) &&
               !string.IsNullOrEmpty(updated.expWorkSubject)
               )
            {*/
                log.LogInformation($"Updating experience entity: {expId} successfully");
                existingRow.expCompany = updated.expCompany;
                existingRow.expCompanyLogoUri = updated.expCompanyLogoUri;
                existingRow.expModifiyDate = updated.expModifiyDate;
                existingRow.expStartDate = updated.expStartDate;
                existingRow.expEndDate = updated.expEndDate;
                existingRow.expCurrentWorkRole = updated.expCurrentWorkRole;
                existingRow.expEmployeeTitle = updated.expEmployeeTitle;
                existingRow.expWorkDescription = updated.expWorkDescription;
                existingRow.expWorkLinks = updated.expWorkLinks;
                existingRow.expWorkSubject = updated.expWorkSubject;
                existingRow.expLocation = updated.expLocation;               
            //}

            var replaceOperation = TableOperation.Replace(existingRow);
            await experienceTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ExperienceEntity());
        }
        #endregion
        #region DeleteExperience
        [FunctionName("DeleteExperience")]
        public static async Task<IActionResult> UpdateExperienceById(
             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "experience/{expId}")] HttpRequest req,
             [Table("aramizexperience", Connection = "AzureWebJobsStorage")] CloudTable experienceTable,
             ILogger log, string expId)
        {
            ClaimsPrincipal principal;
            AuthenticationHeaderValue.TryParse(req.Headers[HeaderNames.Authorization], out var authHeader);
            if ((principal = await Security.ValidateTokenAsync(authHeader)) == null)
            {
                return new UnauthorizedResult();
            }

            var deleteOperation = TableOperation.Delete(new TableEntity()
            { PartitionKey = "experience", RowKey = expId, ETag = "*" });

            try
            {
                var deleteResult = await experienceTable.ExecuteAsync(deleteOperation);
                log.LogInformation($"Deleted experience entity: {expId} successfully");
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                log.LogInformation($"Something went wrong when trying to delete experience entity {expId}");
                log.LogInformation($"Source: {e.Source} Exception Message: {e.Message}");
                return new NotFoundResult();
            }

            return new OkResult();
        }
        #endregion
    }
}

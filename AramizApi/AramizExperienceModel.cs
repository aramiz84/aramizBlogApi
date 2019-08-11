using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;

namespace Aramiz
{
    public class ExperienceModel
    {
        public string expId { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime expCreatedDate { get; set; } = DateTime.Now;
        public DateTime? expModifiyDate { get; set; }
        public string expCompanyLogoUri { get; set; }
        public string expEmployeeTitle { get; set; }
        public string expCompany { get; set; }
        public string expLocation { get; set; }
        public bool expCurrentWorkRole { get; set; } = false;
        public DateTime expStartDate { get; set; }
        public DateTime expEndDate { get; set; }
        public string expWorkSubject { get; set; }
        public string expWorkDescription { get; set; }
        public string[] expWorkLinks { get; set; }
    }

    public class ExperienceCreateModel
    {
        public string expEmployeeTitle { get; set; }
        public string expCompanyLogoUri { get; set; }
        public string expCompany { get; set; }
        public string expLocation { get; set; }
        public bool expCurrentWorkRole { get; set; }
        public DateTime expStartDate { get; set; }
        public DateTime expEndDate { get; set; }
        public string expWorkSubject { get; set; }
        public string expWorkDescription { get; set; }
        public string[] expWorkLinks { get; set; }
    }

    public class ExperienceUpdateModel
    {
        public DateTime expModifiyDate { get; set; } = DateTime.Now;
        public string expEmployeeTitle { get; set; }
        public string expCompanyLogoUri { get; set; }
        public string expCompany { get; set; }
        public string expLocation { get; set; }
        public bool expCurrentWorkRole { get; set; }
        public DateTime expStartDate { get; set; }
        public DateTime expEndDate { get; set; }
        public string expWorkSubject { get; set; }
        public string expWorkDescription { get; set; }
        public string[] expWorkLinks { get; set; }
    }

    public class ExperienceEntity : TableEntity
    {
        public DateTime expCreatedDate { get; set; }
        public DateTime? expModifiyDate { get; set; }
        public string expCompanyLogoUri { get; set; }
        public string expEmployeeTitle { get; set; }
        public string expCompany { get; set; }
        public string expLocation { get; set; }
        public bool expCurrentWorkRole { get; set; }
        public DateTime expStartDate { get; set; }
        public DateTime expEndDate { get; set; }
        public string expWorkSubject { get; set; }
        public string expWorkDescription { get; set; }
        public string[] expWorkLinks { get; set; }
    }

    public static class ExpericenceMappings
    {
        public static ExperienceEntity ExperienceTableEntity(this ExperienceModel experience)
        {
            return new ExperienceEntity
            {
                PartitionKey = "experience",
                RowKey = experience.expId,
                expCreatedDate = experience.expCreatedDate,
                expCompanyLogoUri = experience.expCompanyLogoUri,
                expEmployeeTitle = experience.expEmployeeTitle,
                expCompany = experience.expCompany,
                expLocation = experience.expLocation,
                expCurrentWorkRole = experience.expCurrentWorkRole,
                expStartDate = experience.expStartDate,
                expEndDate = experience.expEndDate,
                expWorkSubject = experience.expWorkSubject,
                expWorkDescription = experience.expWorkDescription,
                expWorkLinks = experience.expWorkLinks
            };
        }

        public static ExperienceModel ExperienceEntity(this ExperienceEntity experience)
        {
            return new ExperienceModel()
            {
                expId = experience.RowKey,
                expCreatedDate = experience.expCreatedDate,
                expModifiyDate = experience.expModifiyDate,
                expCompanyLogoUri = experience.expCompanyLogoUri,
                expEmployeeTitle = experience.expEmployeeTitle,
                expCompany = experience.expCompany,
                expLocation = experience.expLocation,
                expCurrentWorkRole = experience.expCurrentWorkRole,
                expStartDate = experience.expStartDate,
                expEndDate = experience.expEndDate,
                expWorkSubject = experience.expWorkSubject,
                expWorkDescription = experience.expWorkDescription,
                expWorkLinks = experience.expWorkLinks
            };
        }
    }
}

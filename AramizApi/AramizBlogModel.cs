using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aramiz
{
    public class Blog
    {
        public string BlogId { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime BlogCreatedDate { get; set; } = DateTime.Now;
        public string BlogCategory { get; set; }
        public string BlogSubject { get; set; }
        public string BlogBody { get; set; }
        public DateTime? BlogModifiedDate { get; set; }
    }

    public class BlogCreateModel
    {
        public string BlogCategory { get; set; }
        public string BlogSubject { get; set; }
        public string BlogBody { get; set; }
    }

    public class BlogUpdateModel
    {
        public string BlogCategory { get; set; }
        public string BlogSubject { get; set; }
        public string BlogBody { get; set; }
        public DateTime BlogModifiedDate { get; set; } = DateTime.Now;
    }

    public class BlogEntity : TableEntity
    {
        public DateTime BlogCreatedDate { get; set; }
        public string BlogCategory { get; set; }
        public string BlogSubject { get; set; }
        public string BlogBody { get; set; }
        public DateTime? BlogModifiedDate { get; set; }
    }

    public static class Mappings
    { 
        public static BlogEntity BlogTableEntity(this Blog blog)
        {
            return new BlogEntity
            {
                PartitionKey = "blog",
                RowKey = blog.BlogId,
                BlogCreatedDate = blog.BlogCreatedDate,
                BlogCategory = blog.BlogCategory,
                BlogSubject = blog.BlogSubject,
                BlogBody = blog.BlogBody
            };
        }

        public static Blog BlogEntity(this BlogEntity blog)
        {
            return new Blog()
            {
                BlogId = blog.RowKey,
                BlogCreatedDate = blog.BlogCreatedDate,
                BlogCategory = blog.BlogCategory,
                BlogSubject = blog.BlogSubject,
                BlogBody = blog.BlogBody,
                BlogModifiedDate = blog.BlogModifiedDate
            };
        }
    }
}

﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SampleStore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace SampleStore.Controllers
{
    public class SamplesController : ApiController
    {
        private const String partitionName = "Samples_Partition_1";

        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;

        public SamplesController()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Samples");
        }

        /// <summary>
        /// Get all samples
        /// </summary>
        /// <returns></returns>
        // GET: api/Samples
        public IEnumerable<Sample> Get()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<SampleEntity> entityList = new List<SampleEntity>(table.ExecuteQuery(query));

            // Basically create a list of Sample from the list of SampleEntity with a 1:1 object relationship, filtering data as needed
            IEnumerable<Sample> sampleList = from e in entityList
                                               select new Sample()
                                               {
                                                   SampleID = e.RowKey,
                                                   Title = e.Title,
                                                   Artist = e.Artist,
                                                   SampleMp3URL = e.SampleMp3URL
                                               };
            return sampleList;
        }

        // GET: api/Samples/5
        /// <summary>
        /// Get a sample
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Sample))]
        public IHttpActionResult GetSample(string id)
        {
            // Create a retrieve operation that takes a sample entity.
            TableOperation getOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult getOperationResult = table.Execute(getOperation);

            // Construct response including a new DTO as apprporiatte
            if (getOperationResult.Result == null) return NotFound();
            else
            {
                SampleEntity sampleEntity = (SampleEntity)getOperationResult.Result;
                Sample s = new Sample()
                {
                    SampleID = sampleEntity.RowKey,
                    Title = sampleEntity.Title,
                    Artist = sampleEntity.Artist,
                    SampleMp3URL = sampleEntity.SampleMp3URL
                };
                return Ok(s);
            }
        }

        // POST: api/Samples
        /// <summary>
        /// Create a new sample
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        [ResponseType(typeof(Sample))]
        public IHttpActionResult PostSamplel(Sample sample)
        {
            SampleEntity sampleEntity = new SampleEntity()
            {
                RowKey = getNewMaxRowKeyValue(),
                PartitionKey = partitionName,
                Title = sample.Title,
                Artist = sample.Artist,
                CreatedDate = DateTime.Now,
                Mp3Blob = null,
                SampleMp3Blob = null,
                SampleMp3URL = sample.SampleMp3URL,
                SampleDate = null
            };

            // Create the TableOperation that inserts the sample entity.
            var insertOperation = TableOperation.Insert(sampleEntity);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return CreatedAtRoute("DefaultApi", new { id = sampleEntity.RowKey }, sampleEntity);
        }

        // TODO PUT - UPDATE

        // TODO DELETE

        private String getNewMaxRowKeyValue()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

            int maxRowKeyValue = 0;
            foreach (SampleEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }

    }
}
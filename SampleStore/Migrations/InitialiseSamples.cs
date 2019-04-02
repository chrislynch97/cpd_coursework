using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using SampleStore.Models;

namespace SampleStore.Migrations
{
    public static class InitialiseSamples
    {
        public static void go()
        {
            const String partitionName = "Samples_Partition_1";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("Samples");

            // If table doesn't already exist in storage then create and populate it with some initial values, otherwise do nothing
            if (!table.Exists())
            {
                // Create table if it doesn't exist already
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                // Create a sample entity and add it to the table.
                SampleEntity sample1 = new SampleEntity(partitionName, "1");
                sample1.Title = "Sample 1 Title";
                sample1.Artist = "Sample 1 Artist";
                sample1.CreatedDate = DateTime.Now;
                sample1.Mp3Blob = null;
                sample1.SampleMp3Blob = null;
                sample1.SampleMp3URL = null;
                sample1.SampleDate = null;

                // Create a sample entity and add it to the table.
                SampleEntity sample2 = new SampleEntity(partitionName, "2");
                sample2.Title = "Sample 2 Title";
                sample2.Artist = "Sample 2 Artist";
                sample2.CreatedDate = DateTime.Now;
                sample2.Mp3Blob = null;
                sample2.SampleMp3Blob = null;
                sample2.SampleMp3URL = null;
                sample2.SampleDate = null;

                // Create a sample entity and add it to the table.
                SampleEntity sample3 = new SampleEntity(partitionName, "3");
                sample3.Title = "Sample 3 Title";
                sample3.Artist = "Sample 3 Artist";
                sample3.CreatedDate = DateTime.Now;
                sample3.Mp3Blob = null;
                sample3.SampleMp3Blob = null;
                sample3.SampleMp3URL = null;
                sample3.SampleDate = null;

                // Create a sample entity and add it to the table.
                SampleEntity sample4 = new SampleEntity(partitionName, "4");
                sample4.Title = "Sample 4 Title";
                sample4.Artist = "Sample 4 Artist";
                sample4.CreatedDate = DateTime.Now;
                sample4.Mp3Blob = null;
                sample4.SampleMp3Blob = null;
                sample4.SampleMp3URL = null;
                sample4.SampleDate = null;

                // Create a sample entity and add it to the table.
                SampleEntity sample5 = new SampleEntity(partitionName, "5");
                sample5.Title = "Sample 5 Title";
                sample5.Artist = "Sample 5 Artist";
                sample5.CreatedDate = DateTime.Now;
                sample5.Mp3Blob = null;
                sample5.SampleMp3Blob = null;
                sample5.SampleMp3URL = null;
                sample5.SampleDate = null;

                // Create a sample entity and add it to the table.
                SampleEntity sample6 = new SampleEntity(partitionName, "6");
                sample6.Title = "Sample 6 Title";
                sample6.Artist = "Sample 6 Artist";
                sample6.CreatedDate = DateTime.Now;
                sample6.Mp3Blob = null;
                sample6.SampleMp3Blob = null;
                sample6.SampleMp3URL = null;
                sample6.SampleDate = null;

                // Add sample entities to the batch insert operation.
                batchOperation.Insert(sample1);
                batchOperation.Insert(sample2);
                batchOperation.Insert(sample3);
                batchOperation.Insert(sample4);
                batchOperation.Insert(sample5);
                batchOperation.Insert(sample6);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }
    }
}
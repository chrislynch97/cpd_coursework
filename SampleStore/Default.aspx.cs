using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using SampleStore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;

// Remember: code behind is run at the server.

namespace SampleStore
{
    public partial class _Default : System.Web.UI.Page
    {
        // accessor variables and methods for blob containers and queues
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudQueueService _queueStorageService = new CloudQueueService();

        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;

        public _Default()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Samples");
        }

        private CloudBlobContainer getSampleGalleryContainer()
        {
            return _blobStorageService.getCloudBlobContainer();
        }

        private CloudQueue getSampleMakerQueue()
        {
            return _queueStorageService.getCloudQueue();
        }

        private string GetMimeType(string Filename)
        {
            try
            {
                string ext = Path.GetExtension(Filename).ToLowerInvariant();
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (key != null)
                {
                    string contentType = key.GetValue("Content Type") as String;
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
            }
            return "application/octet-stream";
        }

        // User clicked the "Submit" button
        protected void submitButton_Click(object sender, EventArgs e)
        {
            if (upload.HasFile)
            {
                // Get the file name specified by the user. 
                var ext = Path.GetExtension(upload.FileName);

                // Add more information to it so as to make it unique
                // within all the files in that blob container
                var name = string.Format("{0}{1}", Guid.NewGuid(), ext);

                // Upload mp3 to the cloud. Store it in a new 
                // blob in the specified blob container. 

                // Go to the container, instantiate a new blob
                // with the descriptive name
                String path = "mp3s/" + name;

                var blob = getSampleGalleryContainer().GetBlockBlobReference(path);

                // The blob properties object (the label on the bucket)
                // contains an entry for MIME type. Set that property.
                blob.Properties.ContentType = GetMimeType(upload.FileName);

                // Add Title Metadata to blob
                blob.Metadata.Add("Title", upload.FileName.Substring(0, upload.FileName.Length - 4));

                // Actually upload the data to the
                // newly instantiated blob
                blob.UploadFromStream(upload.FileContent);

                // Place a message in the queue to tell the worker
                // role that a new mp3 blob exists, which will 
                // cause it to create a sample blob of that mp3
                //var message = new CloudQueueMessage(upload.FileName, "123");
                //message.SetMessageContent(System.Text.Encoding.UTF8.GetBytes(name));
                //getSampleMakerQueue().AddMessage(message);

                const String partitionName = "Samples_Partition_1";

                TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

                List<SampleEntity> entityList = new List<SampleEntity>(table.ExecuteQuery(query));
                SampleEntity entity = entityList[entityList.Count - 1];

                entity.Mp3Blob = name;

                var message = new JavaScriptSerializer().Serialize(entity);

                getSampleMakerQueue().AddMessage(new CloudQueueMessage(System.Text.Encoding.UTF8.GetBytes(message)));

                System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", path));
            }
        }
    }
}

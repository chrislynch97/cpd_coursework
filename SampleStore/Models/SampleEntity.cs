// Entity class for Azure table
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SampleStore.Models
{

    public class SampleEntity : TableEntity
    {
        public string Title { get; set; } // Title of sample
        public string Artist { get; set; } // Name of artist
        public DateTime CreatedDate { get; set; } // Creation date/time of entity
        public string Mp3Blob { get; set; } // Name of uploaded blob in blob storage
        public string SampleMp3Blob { get; set; } // Name of sample blob in blob storage
        public string SampleMp3URL { get; set; } // Web service resource URL of mp3 sample
        public DateTime? SampleDate { get; set; } // Creation date/time of sample blob

        public SampleEntity(string partitionKey, string sampleID)
        {
            PartitionKey = partitionKey;
            RowKey = sampleID;
        }

        public SampleEntity() { }

    }
}

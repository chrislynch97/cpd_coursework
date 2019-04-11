using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using Microsoft.WindowsAzure.Storage.Table;
using SampleStore.Models;
using SampleStore;

namespace SampleStore_WebJob
{
    public class Functions
    {

        // This class contains the application-specific WebJob code consisting of event-driven
        // methods executed when messages appear in queues with any supporting code.

        // Trigger method  - run when new message detected in queue. "samplemaker" is name of queue.
        // "samplegallery" is name of storage container; "mp3s" and "samples" are folder names. 
        // "{queueTrigger}" is an inbuilt variable taking on value of contents of message automatically;
        // the other variables are valued automatically.
        //public static void GenerateSample(
        //[QueueTrigger("samplemaker")] String blobInfo,
        //[Blob("samplegallery/mp3s/{queueTrigger}")] CloudBlockBlob inputBlob,
        //[Blob("samplegallery/samples/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        //{
        //    inputBlob.FetchAttributes();

        //    //use log.WriteLine() rather than Console.WriteLine() for trace output
        //    logger.WriteLine("GenerateSample() started...");
        //    logger.WriteLine("Input blob is: " + blobInfo);

        //    // Open streams to blobs for reading and writing as appropriate.
        //    // Pass references to application specific methods
        //    using (Stream input = inputBlob.OpenRead())
        //    using (Stream output = outputBlob.OpenWrite())
        //    {
        //        CreateSample(input, output, 20);
        //        outputBlob.Properties.ContentType = "audio/mpeg3";
        //        outputBlob.Metadata.Add("Title", inputBlob.Metadata["Title"]);
        //    }
        //    logger.WriteLine("GenerateSample() completed...");
        //}

        public static void GenerateSample(
        [QueueTrigger("audiosamplemaker")] SampleEntity sampleInQueue,
        [Table("Samples", "{PartitionKey}", "{RowKey}")] SampleEntity sampleInTable,
        [Table("Samples")] CloudTable tableBinding, TextWriter logger)
        {
            //use log.WriteLine() rather than Console.WriteLine() for trace output
            logger.WriteLine("GenerateSample() started...");

            //var inputBlob = getSampleGalleryContainer().GetBlockBlobReference("mp3/" + sampleInQueue.Mp3Blob);
            //var outputBlob = getSampleGalleryContainer().GetBlockBlobReference("samples/" + sampleInQueue.Mp3Blob);

            //// Open streams to blobs for reading and writing as appropriate.
            //// Pass references to application specific methods
            //using (Stream input = inputBlob.OpenRead())
            //using (Stream output = outputBlob.OpenWrite())
            //{
            //    CreateSample(input, output, 20);
            //    outputBlob.Properties.ContentType = "audio/mpeg3";
            //    outputBlob.Metadata.Add("Title", inputBlob.Metadata["Title"]);
            //}

            sampleInTable.Mp3Blob = sampleInQueue.Mp3Blob;
            //sampleInTable.SampleMp3Blob = sampleInQueue.Mp3Blob;
            //sampleInTable.SampleDate = DateTime.Now;
            //sampleInTable.SampleMp3URL = "URL";

            TableOperation updateOperation = TableOperation.InsertOrReplace(sampleInTable);

            logger.WriteLine("GenerateSample() completed...");
        }

        private static void CreateSample(Stream input, Stream output, int duration)
        {
            using (var reader = new Mp3FileReader(input, wave => new NLayer.NAudioSupport.Mp3FrameDecompressor(wave)))
            {
                Mp3Frame frame;
                frame = reader.ReadNextFrame();
                int frameTimeLength = (int)(frame.SampleCount / (double)frame.SampleRate * 1000.0);
                int framesRequired = (int)(duration / (double)frameTimeLength * 1000.0);

                int frameNumber = 0;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    frameNumber++;

                    if (frameNumber <= framesRequired)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else break;
                }
            }
        }
    }
}

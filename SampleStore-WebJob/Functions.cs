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

        public static void GenerateSample(
        [QueueTrigger("audiosamplemaker")] SampleEntity sampleInQueue,
        [Blob("audiosamplegallery/mp3s/{Mp3Blob}")] CloudBlockBlob inputBlob,
        [Blob("audiosamplegallery/samples/{Mp3Blob}")] CloudBlockBlob outputBlob,
        [Table("Samples", "{PartitionKey}", "{RowKey}")] SampleEntity sampleInTable,
        [Table("Samples")] CloudTable tableBinding, TextWriter logger)
        {
            //use log.WriteLine() rather than Console.WriteLine() for trace output
            logger.WriteLine("GenerateSample() started...");

            // Open streams to blobs for reading and writing as appropriate.
            // Pass references to application specific methods
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                CreateSample(input, output, 20);
                outputBlob.Properties.ContentType = "audio/mpeg3";
                outputBlob.Metadata.Add("Title", inputBlob.Metadata["Title"]);
            }

            sampleInTable.Mp3Blob = sampleInQueue.Mp3Blob;
            sampleInTable.SampleMp3Blob = sampleInQueue.Mp3Blob;
            sampleInTable.SampleDate = DateTime.Now;
            sampleInTable.SampleMp3URL = "http://127.0.0.1:10000" + outputBlob.Uri.AbsolutePath;
            //sampleInTable.SampleMp3URL = "https://clstor050319.blob.core.windows.net" + outputBlob.Uri.AbsolutePath;

            TableOperation updateOperation = TableOperation.InsertOrReplace(sampleInTable);
            tableBinding.Execute(updateOperation);

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

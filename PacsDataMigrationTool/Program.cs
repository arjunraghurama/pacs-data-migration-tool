using Dicom;
using Dicom.Network;
using Dicom.Network.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PacsDataMigrationTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            
            DicomFileQueue dicomFileQueue = new DicomFileQueue();
           
            PacsDataSender pacsDataSender = new PacsDataSender(config, dicomFileQueue);
            
            Task dataSender = new Task(() => _ = pacsDataSender.SendToPacs());
            dataSender.Start();

            PacsDataQuerier pacsDataQuerier = new PacsDataQuerier(config, dicomFileQueue);
            _ = pacsDataQuerier.DataQuery();
            Thread.Sleep(Timeout.Infinite);
            return;
        }
    }
}

using Dicom;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PacsDataMigrationTool
{
    class PacsDataSender
    {
        private readonly IConfiguration _configuration;

        private readonly Dicom.Network.Client.DicomClient _clientSend;

        private DicomFileQueue _dicomFileQueue;

        private int dcmImagesSent = 0;
        
        public PacsDataSender(IConfiguration configuration, DicomFileQueue dicomFileQueue)
        {
            _configuration = configuration;
            var host = _configuration.GetSection("Destination:Host").Value;
            var scpAeTitle = _configuration.GetSection("Destination:AeTitle").Value;
            var port = Int32.Parse(_configuration.GetSection("Destination:Port").Value);
            _clientSend = new Dicom.Network.Client.DicomClient(host, port, false, "SCU", scpAeTitle);
            _dicomFileQueue = dicomFileQueue;
        }

        public async Task<bool> SendToPacs()
        {
            try
            {
                while(true)
                {
                    var dataset = _dicomFileQueue.Dequeue();
                    var file = new DicomFile(dataset);
                    ++dcmImagesSent;
                    Console.WriteLine("Number of DCM images transferred : "+ dcmImagesSent);
                    // Send Data to Dicom Server
                    await _clientSend.AddRequestAsync(new Dicom.Network.DicomCStoreRequest(file));
                    await _clientSend.SendAsync();
                }
                return (true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

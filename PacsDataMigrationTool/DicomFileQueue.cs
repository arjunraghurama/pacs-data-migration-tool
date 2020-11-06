using Dicom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PacsDataMigrationTool
{
    class DicomFileQueue
    {
        ManualResetEventSlim isDataAvailable = new ManualResetEventSlim(false);

        private ConcurrentQueue<DicomDataset> DicomDataSetFileQueue { get; set; } = new ConcurrentQueue<DicomDataset>();

        public bool Enqueue(DicomDataset dicomDataSet)
        {
            try
            {
                DicomDataSetFileQueue.Enqueue(dicomDataSet);
                isDataAvailable.Set();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DicomDataset Dequeue()
        {
            try
            {
                if (DicomDataSetFileQueue.Count == 0)
                {
                    isDataAvailable.Reset();
                    isDataAvailable.Wait();
                }

                DicomDataset dicomDataSet;
                DicomDataSetFileQueue.TryDequeue(out dicomDataSet);
                return dicomDataSet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}

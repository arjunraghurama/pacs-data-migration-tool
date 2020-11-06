using Dicom;
using Dicom.Network;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PacsDataMigrationTool
{
    class PacsDataQuerier
    {
        private readonly IConfiguration _configuration;

        private Dicom.Network.Client.DicomClient clientQuery;

        private DicomFileQueue _dicomFileQueue;

        public PacsDataQuerier(IConfiguration configuration, DicomFileQueue dicomFileQueue)
        {
            _configuration = configuration;
            _dicomFileQueue = dicomFileQueue;
        }

        public async Task<bool> DataQuery()
        {
            try
            {
                var host = _configuration.GetSection("Source:Host").Value;
                var scpAeTitle = _configuration.GetSection("Source:AeTitle").Value;
                var port = Int32.Parse(_configuration.GetSection("Source:Port").Value);
                
                clientQuery = new Dicom.Network.Client.DicomClient(host, port, false, "SCU", scpAeTitle);

                var cFindStudy = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

                cFindStudy.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");

                cFindStudy.OnResponseReceived = async (DicomCFindRequest rq, DicomCFindResponse rp) => {

                    if (null != rp.Dataset)
                    {
                        var cFindSeries = new DicomCFindRequest(DicomQueryRetrieveLevel.Series, DicomPriority.Medium);
                        string studyUid = rp.Dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID);
                        cFindSeries.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyUid);
                        cFindSeries.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
                        cFindSeries.OnResponseReceived = async (DicomCFindRequest req, DicomCFindResponse rep) => {

                            if (null != rep.Dataset)
                            {
                                var cFindImage = new DicomCFindRequest(DicomQueryRetrieveLevel.Image, DicomPriority.Medium);
                                var seriesUid = rep.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID);

                                cFindImage.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyUid);
                                cFindImage.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, seriesUid);
                                cFindImage.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, "");

                                cFindImage.OnResponseReceived = async (DicomCFindRequest reqi, DicomCFindResponse repi) =>
                                {
                                    if (null != repi.Dataset)
                                    {
                                        var imageUid = repi.Dataset.GetString(DicomTag.SOPInstanceUID);
                                        var clientCGet = new Dicom.Network.Client.DicomClient(host, port, false, "SCU", scpAeTitle);

                                        clientCGet.OnCStoreRequest += (DicomCStoreRequest reqs) =>
                                        {
                                            _dicomFileQueue.Enqueue(reqs.Dataset);
                                            return Task.FromResult(new DicomCStoreResponse(reqs, DicomStatus.Success));
                                        };
                                        var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                                                  DicomStorageCategory.Image,
                                                  DicomTransferSyntax.ExplicitVRLittleEndian,
                                                  DicomTransferSyntax.ImplicitVRLittleEndian,
                                                  DicomTransferSyntax.ImplicitVRBigEndian);
                                        clientCGet.AdditionalPresentationContexts.AddRange(pcs);
                                        var cGetRequest = new DicomCGetRequest(studyUid, seriesUid, imageUid);
                                        await clientCGet.AddRequestAsync(cGetRequest);
                                        await clientCGet.SendAsync();
                                    }
                                };
                                await clientQuery.AddRequestAsync(cFindImage);
                                await clientQuery.SendAsync();
                            }
                        };
                        await clientQuery.AddRequestAsync(cFindSeries);
                        await clientQuery.SendAsync();
                    }
                };

                await clientQuery.AddRequestAsync(cFindStudy);
                await clientQuery.SendAsync();
                Thread.Sleep(Timeout.Infinite);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

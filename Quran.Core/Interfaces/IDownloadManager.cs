using System.Collections.Generic;
using Quran.Core.Common;

namespace Quran.Core.Interfaces
{
    public interface IDownloadManager
    {
        ITransferRequest DownloadAsync(string from, string to);
        ITransferRequest DownloadMultipleAsync(string[] from, string to);
        ITransferRequest GetRequest(string serverUri);
        void Cancel(ITransferRequest request);
        void FinalizeRequest(ITransferRequest request);
        IEnumerable<ITransferRequest> GetAllRequests();
        IEnumerable<string> GetAllStuckFiles();
        void Dispose();
    }
}

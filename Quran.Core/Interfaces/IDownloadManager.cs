using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quran.Core.Common;

namespace Quran.Core.Interfaces
{
    public interface IDownloadManager : IDisposable
    {
        Task<ITransferRequest> DownloadAsync(string from, string to, CancellationToken token = default(CancellationToken));
        Task<ITransferRequest> DownloadMultipleAsync(string[] from, string to, CancellationToken token = default(CancellationToken));
        Task<ITransferRequest> GetRequest(string serverUri);
        Task Cancel(ITransferRequest request);
        void FinalizeRequest(ITransferRequest request);
        Task<IEnumerable<ITransferRequest>> GetAllRequests(CancellationToken token = default(CancellationToken));
        IEnumerable<string> GetAllStuckFiles();
    }
}

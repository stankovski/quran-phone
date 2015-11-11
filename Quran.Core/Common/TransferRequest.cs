using System;
using System.Security;

namespace Quran.Core.Common
{
    public interface ITransferRequest
    {
        /// <summary>
        /// Gets the unique identifier associated with the request.
        /// </summary>
        /// 
        /// <returns>
        /// The unique identifier of the request.
        /// </returns>
        string RequestId { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the target URI associated with the request.
        /// </summary>
        /// 
        /// <returns>
        /// The target URI associated with the request.
        /// </returns>
        Uri RequestUri { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets or sets the location to which the requested file will be downloaded.
        /// </summary>
        /// 
        /// <returns>
        /// The location to which the requested file will be downloaded.
        /// </returns>
        string DownloadLocation { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the status of the request.
        /// </summary>
        /// 
        /// <returns>
        /// The status of the request.
        /// </returns>
        FileTransferStatus TransferStatus { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the total number of bytes that will be downloaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The total number of bytes that will be downloaded for the request.
        /// </returns>
        ulong TotalBytesToReceive { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the number of bytes that have been downloaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes that have been downloaded for the request.
        /// </returns>
        ulong BytesReceived { [SecuritySafeCritical] get; }

        /// <summary>
        /// Occurs when the progress of the transfer changes.
        /// </summary>
        event EventHandler<TransferEventArgs> TransferProgressChanged;
    }
}

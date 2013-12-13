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
        /// Gets or sets additional data associated with the request.
        /// </summary>
        /// 
        /// <returns>
        /// Additional data associated with the request.
        /// </returns>
        string Tag { [SecuritySafeCritical] get; [SecuritySafeCritical] set; }

        /// <summary>
        /// Gets or sets the location to which the requested file will be downloaded.
        /// </summary>
        /// 
        /// <returns>
        /// The location to which the requested file will be downloaded.
        /// </returns>
        Uri DownloadLocation { [SecuritySafeCritical] get; [SecuritySafeCritical] set; }

        /// <summary>
        /// Gets or sets the location from which the requested file will be uploaded.
        /// </summary>
        /// 
        /// <returns>
        /// The location from which the requested file will be uploaded
        /// </returns>
        Uri UploadLocation { [SecuritySafeCritical] get; [SecuritySafeCritical] set; }

        /// <summary>
        /// Gets the status of the request.
        /// </summary>
        /// 
        /// <returns>
        /// The status of the request.
        /// </returns>
        FileTransferStatus TransferStatus { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the exception associated with a failed background transfer request. A transfer request can have a <see cref="P:Microsoft.Phone.BackgroundTransfer.BackgroundTransferRequest.TransferStatus"/> of <see cref="F:Microsoft.Phone.BackgroundTransfer.TransferStatus.Completed"/> whether or not the transfer was completed successfully. If a completed transfer was successful, TransferError will be null.
        /// </summary>
        /// 
        /// <returns>
        /// The exception associated with a failed background transfer request.
        /// </returns>
        Exception TransferError { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the total number of bytes that will be downloaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The total number of bytes that will be downloaded for the request.
        /// </returns>
        long TotalBytesToReceive { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the total number of bytes that will be uploaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The total number of bytes that will be uploaded for the request.
        /// </returns>
        long TotalBytesToSend { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the number of bytes that have been downloaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes that have been downloaded for the request.
        /// </returns>
        long BytesReceived { [SecuritySafeCritical] get; }

        /// <summary>
        /// Gets the number of bytes that have been uploaded for the request.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes that have been uploaded for the request.
        /// </returns>
        long BytesSent { [SecuritySafeCritical] get; }

        bool IsCancelled { get; }

        void Cancel();

        /// <summary>
        /// Occurs when the <see cref="P:Microsoft.Phone.BackgroundTransfer.BackgroundTransferRequest.TransferStatus"/> property of a request changes.
        /// </summary>
        event EventHandler<TransferEventArgs> TransferStatusChanged;

        /// <summary>
        /// Occurs when the progress of the transfer changes.
        /// </summary>
        event EventHandler<TransferEventArgs> TransferProgressChanged;
    }
}

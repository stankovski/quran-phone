namespace Quran.Core.Common
{
    public enum FileTransferStatus
    {
        None,
        Transferring,
        Waiting,
        WaitingForWiFi,
        WaitingForExternalPower,
        WaitingForExternalPowerDueToBatterySaverMode,
        WaitingForNonVoiceBlockingNetwork,
        Paused,
        Completed,
        Unknown,
        Cancelled
    }
}

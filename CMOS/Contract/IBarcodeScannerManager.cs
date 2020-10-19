using Symbol.XamarinEMDK.Barcode;
using System;

namespace CMOS.Contract
{
    public interface IBarcodeScannerManager : IDisposable
    {
        event EventHandler<Scanner.DataEventArgs> ScanReceived;
        bool IsScannerEnabled { get; }
        void Enable();
        void Disable();
    }
}
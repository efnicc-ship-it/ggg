namespace TeknikServis.Domain.Enums;

public enum ServiceStatus
{
    Received = 1,           // Teslim Alındı
    Diagnosing = 2,         // Arıza Tespiti
    AwaitingParts = 3,      // Parça Bekleniyor
    InRepair = 4,           // Onarımda
    AwaitingApproval = 5,   // Onay Bekleniyor
    Ready = 6,              // Hazır
    Delivered = 7,          // Teslim Edildi
    Cancelled = 8,          // Müşteri İsteğiyle İptal
    Unrepairable = 9,       // Onarım Mümkün Değil — cihaz iade edildi
    ReturnedUnrepaired = 10 // İade (onarılmadan iade)
}

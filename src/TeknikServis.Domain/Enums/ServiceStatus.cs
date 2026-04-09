namespace TeknikServis.Domain.Enums;

public enum ServiceStatus
{
    // ─── 1. Kabul ve Giriş ───────────────────────────────────────────────────
    Received = 1,               // Cihaz Teslim Alındı (standart kabul)
    CargoWaiting = 2,           // Kargo Bekleniyor (uzaktan müşteri/bayi gönderimi yolda)

    // ─── Bayi Akışı (Dealer Flow) ────────────────────────────────────────────
    DealerRegistered = 20,      // Bayi Tarafından Kayıt Edildi (fiziksel teslim henüz yok)
    DealerInTransit = 21,       // Bayiden Transfer Aşamasında (bayi servise kargo/kurye gönderdi)
    BatchAcceptancePending = 22,// Toplu Kabul / Ayrıştırma Bekliyor (kargo merkeze geldi, barkod eşleme bekliyor)

    // ─── 2. Teşhis ve Onay ───────────────────────────────────────────────────
    DiagnosisWaiting = 3,       // Arıza Tespiti Bekleniyor (teknisyen sırasında)
    Diagnosing = 4,             // Arıza Tespiti Aşamasında (teknisyen aktif inceliyor)
    AwaitingApproval = 5,       // Müşteri Onayı Bekleniyor (fiyat/risk/işlem onayı)
    AwaitingDealerApproval = 23,// Bayi Onayı Bekleniyor (bayinin müşterisini arayıp onay alacağı durum)

    // ─── 3. Operasyon ve Onarım ─────────────────────────────────────────────
    ApprovedWaitingRepair = 6,  // Onaylandı / İşlem Sırasında Bekliyor
    InRepair = 7,               // Onarım Aşamasında / Aktif İşlem Görüyor
    AwaitingParts = 8,          // Yedek Parça Bekleniyor (tedarik başlatıldı)
    ExternalService = 9,        // Dış Servise (Taşerona) Yönlendirildi

    // ─── 4. Sonuçlanma ve Kalite Kontrol ────────────────────────────────────
    QualityControl = 10,        // Kalite Kontrol / Test Aşamasında
    Ready = 11,                 // Onarım Tamamlandı / Teslime Hazır

    // ─── Bayi Çıkış Havuzu ───────────────────────────────────────────────────
    DealerPool = 24,            // Bayi Sevkiyat Havuzunda Bekliyor (toplu gönderim bekleniyor)
    DealerCreditHold = 25,      // Cari Limit Engelinde / Ödeme Bekliyor (çıkış durduruldu)
    ShippedToDealer = 26,       // Bayiye Sevk Edildi (toplu irsaliye kesildi)

    // ─── 5. Kapanış ──────────────────────────────────────────────────────────
    Delivered = 12,             // Müşteriye Elden Teslim Edildi (kapandı)
    ShippedBack = 13,           // Müşteriye Kargoya Verildi

    // ─── İptal ve Özel Durumlar ──────────────────────────────────────────────
    Cancelled = 14,             // Müşteri İsteğiyle İptal / Onay Verilmedi
    Unrepairable = 15,          // Onarım Mümkün Değil — cihaz iade edilecek
    ReturnedUnrepaired = 16,    // Onarılmadan İade Edildi (fiziksel teslim tamamlandı)
    ScrapPending = 17,          // Hurdaya Ayrılıyor (müşteri feragat etti veya donör olarak alındı)
}

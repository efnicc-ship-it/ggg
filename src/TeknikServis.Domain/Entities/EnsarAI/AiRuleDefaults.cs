namespace TeknikServis.Domain.Entities.EnsarAI;

/// <summary>
/// Sistem kurulumunda otomatik oluşturulan varsayılan AI kuralları.
/// Hangfire tarafından çalıştırılır, konfigüre edilebilir.
/// </summary>
public static class AiRuleDefaults
{
    public static readonly IReadOnlyList<(string Name, string TriggerType, string? Cron, string Description, string ConditionJson, string ActionsJson)> Rules = new[]
    {
        (
            "Teknisyen 24 Saat Hareketsiz",
            "Scheduled",
            "0 * * * *",  // Her saat
            "Teknisyen cihaza 24 saat dokunmamışsa hatırlatma gönder, 48 saatte müdüre bildir",
            """{"EntityType":"ServiceRecord","Condition":"LastTechnicianActivity < Now - 24h AND Status IN [Diagnosing,InRepair,AwaitingParts]"}""",
            """[{"At24h":{"Action":"Notify","Target":"AssignedTechnician","Channel":"InApp,SMS","Message":"Cihaza 24 saattir müdahale edilmedi: {RecordNumber}"}},{"At48h":{"Action":"Notify","Target":"BranchManager","Channel":"InApp,SMS,Email","Message":"TEKNİSYEN UYARISI: {RecordNumber} 48 saattir bekliyor"}}]"""
        ),
        (
            "Parça Siparişi 4 Saat Verilmedi",
            "Scheduled",
            "0 * * * *",
            "Parça siparişi 4 saat hâlâ verilmemişse teknisyen + şube müdürüne uyarı",
            """{"EntityType":"ServiceRecord","Condition":"Status == AwaitingParts AND PartOrderNotPlaced AND CreatedAt < Now - 4h"}""",
            """[{"Action":"Notify","Targets":["AssignedTechnician","BranchManager"],"Channel":"InApp,SMS","Message":"Parça siparişi verilmedi: {RecordNumber}"}]"""
        ),
        (
            "Kullanıcı Çoklu Silme",
            "Event",
            null,
            "Bir kullanıcı gün içinde 5+ kayıt silmişse acil: müdüre e-posta + sistem bildirimi",
            """{"EntityType":"AuditLog","Condition":"Action == Delete AND UserId == CurrentUser AND Count >= 5 AND TimeWindow == Today"}""",
            """[{"Action":"Notify","Targets":["BranchManager","GeneralManager"],"Channel":"InApp,Email","Severity":"Critical","Message":"GÜVENLIK: {UserName} bugün {Count} kayıt sildi"}]"""
        ),
        (
            "Stok Negatife Düştü",
            "Event",
            null,
            "Stok negatife düştüğünde anlık stok sorumlusuna uyarı",
            """{"EntityType":"StockItem","Condition":"AvailableQuantity < 0"}""",
            """[{"Action":"Notify","Target":"StockManager","Channel":"InApp,SMS","Severity":"Critical","Message":"KRİTİK: Stok negatife düştü - {ItemName}"}]"""
        ),
        (
            "Garanti Dışı Servis Bildirimi",
            "Event",
            null,
            "Garanti süresi dolan cihaz için servis kaydı açıldığında müşteriye otomatik bildirim",
            """{"EntityType":"ServiceRecord","Condition":"RelatedWarrantyRecord != null AND WarrantyRecord.Status == Expired"}""",
            """[{"Action":"NotifyCustomer","Channel":"SMS,WhatsApp","Message":"Cihazınızın garantisi dolmuştur. Ücretli servis kaydınız oluşturulmuştur: {RecordNumber}"}]"""
        ),
        (
            "Bayi 30 Gün Ödeme Yok",
            "Scheduled",
            "0 9 * * MON",  // Pazartesi 09:00
            "Bayi 30 gün ödeme yapmamışsa finans sorumlusuna haftalık özet",
            """{"EntityType":"DealerReceivable","Condition":"IsSettled == false AND DueDate < Now - 30d"}""",
            """[{"Action":"Notify","Target":"FinanceManager","Channel":"InApp,Email","Message":"BAYİ BORCU: {DealerName} - {TotalAmount} TL - {DayCount} gün gecikmiş"}]"""
        ),
        (
            "Cihaz 60 Gün Satılmadı",
            "Scheduled",
            "0 9 * * *",   // Her gün 09:00
            "Cihaz 60 gün satılmadan bekliyorsa fiyat düşürme önerisi",
            """{"EntityType":"DeviceInventory","Condition":"Status == Available AND CreatedAt < Now - 60d"}""",
            """[{"Action":"Notify","Target":"SalesManager","Channel":"InApp,Email","Message":"FİYAT ÖNERİSİ: {Brand} {Model} - {DayCount} gündür satılmadı. Mevcut fiyat: {Price} TL"}]"""
        ),
        (
            "Mesai Dışı Giriş",
            "Event",
            null,
            "Teknisyen çalışma saatleri dışında sisteme girişi: güvenlik logu + müdür bildirimi",
            """{"EntityType":"AuditLog","Condition":"Action == Login AND UserRole == Technician AND (Hour < 8 OR Hour > 20)"}""",
            """[{"Action":"SecurityLog","Severity":"Warning"},{"Action":"Notify","Target":"BranchManager","Channel":"InApp,SMS","Message":"GÜVENLİK: {UserName} mesai dışı giriş yaptı ({LoginTime})"}]"""
        ),
        (
            "IMEI Kontrolsüz Satın Alma",
            "Event",
            null,
            "IMEI kontrolsüz satın alma denemesi: sistem bloke eder + müdüre bildirir",
            """{"EntityType":"PurchaseRecord","Condition":"ImeiChecked == false AND AttemptedSave == true"}""",
            """[{"Action":"Block","Message":"IMEI kontrolü zorunludur"},{"Action":"Notify","Target":"BranchManager","Channel":"InApp","Message":"IMEI kontrolsüz alış denemesi: {UserName}"}]"""
        ),
        (
            "Kara Listeli Müşteri Girişi",
            "Event",
            null,
            "Kara listeli müşteri kaydı açıldığında anlık uyarı + işlem durdur",
            """{"EntityType":"Customer","Condition":"IsBlacklisted == true AND NewRecordAttempt == true"}""",
            """[{"Action":"Block","Message":"Bu müşteri kara listede. İşlem durduruldu."},{"Action":"Notify","Target":"BranchManager","Channel":"InApp,SMS","Severity":"Critical","Message":"KRİTİK: Kara listeli müşteri - {CustomerName} ({Phone})"}]"""
        ),
        (
            "Fire Maliyeti Artışı",
            "Event",
            null,
            "Servis kaydında parça fire nedeniyle maliyet artışı: ilgili müdüre bilgi ver",
            """{"EntityType":"StockWriteOff","Condition":"LinkedServiceRecordId != null"}""",
            """[{"Action":"Notify","Target":"BranchManager","Channel":"InApp","Message":"MALİYET ARTIŞI: {RecordNumber} - {PartName} takılırken kırıldı. Ek maliyet: {Cost} TL"}]"""
        ),
        (
            "İkinci El Onarım Tamamlandı",
            "Event",
            null,
            "Arızalı alınan ikinci el cihazın otomatik servis kaydı tamamlandığında cihaz envanterine işle",
            """{"EntityType":"ServiceRecord","Condition":"SourcePurchaseType == Damaged AND Status == Ready AND LinkedPurchaseRecordId != null"}""",
            """[{"Action":"UpdateDeviceInventory","Message":"Onarım tamamlandı, cihaz stoğa eklendi"},{"Action":"Notify","Target":"AssignedTechnician","Channel":"InApp","Message":"Onarım tamamlandı: {RecordNumber} - İkinci el stoğa eklendi"}]"""
        )
    }.ToList().AsReadOnly();
}

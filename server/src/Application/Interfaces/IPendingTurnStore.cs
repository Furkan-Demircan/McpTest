namespace Application.AI;

/// <summary>
/// İstemci aksiyonlarını bekleyen turların deposu. Demo süreç içi bellekte tutar.
/// CRM entegrasyonunda yapılacaklar (bilinçli olarak ertelendi):
/// - Birden fazla sunucu örneği / deploy için dağıtık depo (örn. Redis, IDistributedCache)
/// - Turun oturum açmış kullanıcıya (ve kiracıya) bağlanması; başka kullanıcının devam isteği reddedilir
/// - Kullanıcı başına tek açık tur ve toplam üst sınır
/// </summary>
public interface IPendingTurnStore
{
    string Save(PendingTurn turn);

    /// <summary>Turu alır ve depodan siler (tek kullanımlık); yoksa veya süresi dolduysa null.</summary>
    PendingTurn? Take(string continuationId);
}

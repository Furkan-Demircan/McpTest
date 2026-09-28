using MCP.Server.Manifest;

namespace API.Forms;

public static class AppPages
{
    public const string Module = "Okul";

    // Ana sayfa (path "/") menüsü, NavLabel'ı olan tüm sayfalardan otomatik oluşturulur.
    public static void Configure(AppPageRegistry pages) => pages
        .Add(new PageDefinition
        {
            Id = "home",
            Path = "/",
            Title = "Ana Sayfa",
            Description = "Uygulamanın giriş noktası; tüm formlara ve kayıt listesine buradan gidilir.",
            Module = Module
        })
        .Add(new PageDefinition
        {
            Id = "users",
            Path = "/users",
            Aliases = ["/kayitlar", "/liste"],
            Title = "Kayıtlı Kullanıcılar",
            Description = "Kayıtlı tüm öğrenci ve öğretmenlerin listesi; arama yapılabilir.",
            Module = Module,
            NavLabel = "Kayıtlı Kullanıcıları Gör",
            Elements =
            [
                new PageElement { Id = "usersSearch", Label = "Ad, soyad, TC No veya e-posta ile ara", Kind = "input" }
            ]
        });
}

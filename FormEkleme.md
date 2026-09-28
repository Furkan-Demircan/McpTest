# Yeni Form Ekleme Rehberi

Formlar **backend'den türetilir**. Yeni bir form için bir DTO ve `[AppForm]` ile işaretlenmiş bir controller action'ı yazarsın.
İstemcide kod yazılmaz, manifest dosyası düzenlenmez, export adımı yoktur.
Örnek olarak **Ders Ekleme** formu kullanılıyor.

## Nasıl çalışır?

1. Sunucu açılışta `[AppForm]` işaretli action'ları bulur (ApiExplorer) ve manifest'i üretir ([AppManifestBuilder.cs](server/src/API/Forms/AppManifestBuilder.cs)):
   - Alanlar action'ın `[FromBody]` DTO'sundan gelir.
   - Submit adresi action'ın route'undan gelir.
2. Manifest `GET /api/app-manifest` ile sunulur. MCP tool'ları da aynı manifest'i kullanır.
3. İstemci manifest'i açılışta çeker ([ManifestProvider.tsx](client/src/app/ManifestProvider.tsx)). Route'ları, ana sayfa menüsünü, kayıt listesindeki "ekle" linklerini ve formları ([GenericFormPage.tsx](client/src/pages/GenericFormPage.tsx)) buradan kurar.

**Opt-in:** Sadece `[AppForm]` taşıyan action'lar forma dönüşür. İşaretlenmemiş endpoint'ler (listeleme, detay, rapor, dahili servisler) manifest'e, istemciye veya modele hiç gitmez. Form ile endpoint arasındaki bağı attribute'un yazıldığı action belirler; tahmin yapılmaz.

| | Nasıl gelir |
|---|---|
| Route, alias yönlendirmeleri, ana sayfa menüsü | Otomatik |
| Form ekranı, etiketler, zorunluluk, validasyon, hata mesajları | Otomatik (DTO'dan) |
| Submit endpoint'i ve sunucu hatalarının gösterimi | Otomatik (action'dan) |
| Asistan: `navigate_to_page`, `list_app_pages`, `get_page_schema`, `fill_form`, `highlight_element`, ekran özeti | Otomatik |
| DTO + action | **Senin yazdığın** |
| Varlığın geri kalan backend'i (entity, servis, repository, tablo) | **Senin yazdığın** |
| Süreç rehberi (Knowledge) | İsteğe bağlı, önerilir |

---

## 1. DTO: alanlar ve validasyon

```csharp
using System.ComponentModel.DataAnnotations;
using Application.Forms;

public record CreateCourseDto
{
    [Display(Name = "Ders Adı", Order = 10)]
    [Required(ErrorMessage = "Ders adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ders adı en fazla 100 karakter olabilir.")]
    public string CourseName { get; init; } = default!;

    [Display(Name = "Ders Kodu", Order = 20, Description = "Örn: MAT101")]
    [Required(ErrorMessage = "Ders kodu zorunludur.")]
    [RegularExpression(@"^[A-Z]{3}\d{3}$", ErrorMessage = "Ders kodu 3 harf + 3 rakam olmalıdır.")]
    public string CourseCode { get; init; } = default!;

    [Display(Name = "Seviye", Order = 30)]
    [Suggestions("Başlangıç", "Orta", "İleri")]
    public string? Level { get; init; }

    [Display(Name = "Başlangıç Tarihi", Order = 40)]
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    public DateOnly StartDate { get; init; }
}
```

| Attribute | Manifest'te | UI'da |
|---|---|---|
| `[Display(Name)]` | `label` | Alan etiketi. Asistan da bu etiketle konuşur. |
| `[Display(Order)]` | Alan sırası | Kalıtımda sırayı korumak için **her alana ver**. |
| `[Display(Description)]` | `hint` | Alanın altındaki ipucu; asistana da gider. |
| `[Required]` | `required`, `requiredMessage` | Yıldız ve "zorunlu" mesajı |
| `[StringLength]` | `minLength` / `maxLength` kuralları | Karakter sınırı ve mesajı |
| `[RegularExpression]` | `pattern` kuralı | Desen kontrolü (tüm değer eşlenir). `\d{N}` desenlerinde rakam dışı karakterler otomatik ayıklanır. |
| `[EmailAddress]` | `type: email`, `email` kuralı | E-posta girişi |
| `[Suggestions(...)]` | `options` | Öneri listesi (datalist); serbest giriş kısıtlanmaz |
| `DateOnly` / `DateTime` | `type: date` | Tarih seçici |

- **Mesajlar tek kaynaktır.** `ErrorMessage` hem istemci validasyonunda hem sunucunun `400` cevabında aynı metinle görünür.
- **Alan adı** (`name`) property'nin camelCase halidir (`courseCode`). Asistan `fill_form` ile bu adla yazar.
- **Ekran kimliği** `{formId}-{name}` olarak üretilir (`courseForm-courseCode`). Submit ve reset butonları `{formId}-submit` / `{formId}-reset` olur.
- Bir alanı formdan gizlemek için `[JsonIgnore]` kullan.

## 2. Action: formu işaretle

```csharp
using API.Forms;

[HttpPost]
[AppForm("courseForm",
    PageId = "course-create",
    Path = "/course",
    Aliases = ["/ders-ekle"],
    Title = "Ders Ekleme Formu",
    Description = "Yeni ders kaydı oluşturulur.",
    Module = "Akademik",
    NavLabel = "Ders Ekle",
    SubmitLabel = "Dersi Kaydet")]
public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken cancellationToken)
{
    var created = await _courseService.CreateAsync(dto, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

| Özellik | Anlamı |
|---|---|
| `Id` (zorunlu) | Form kimliği. Asistan `fill_form(target)` ile kullanır. |
| `PageId` | Sayfa kimliği; boşsa form kimliği kullanılır. Rehberler bu kimlikle referans verir. |
| `Path` (zorunlu), `Aliases` | Kanonik adres ve oraya yönlenen ek adresler |
| `Title` (zorunlu), `Description` | Sayfa başlığı ve açıklaması |
| `Module` | Büyük uygulamada sayfaları gruplar. Asistan önce modülü, sonra sayfayı bulur (`list_app_pages`). |
| `NavLabel` | Doluysa ana sayfa menüsünde ve kayıt listesindeki "ekle" linklerinde görünür. |
| `SubmitLabel` | Kaydet butonunun etiketi (varsayılan "Kaydet") |

- Action'ın bir `[FromBody]` DTO parametresi olmalı. Yoksa form üretilmez ve açılışta hata loglanır.
- Cevap gövdesi başarı kartında gösterilir. Alan adları DTO etiketleriyle, `id` ve `createdAt` sabit etiketlerle yazılır.
- İş kuralı ihlali (örneğin "bu ders kodu zaten mevcut") için `DomainException` fırlat. `GlobalExceptionMiddleware` bunu `400 { message }` olarak döner, formun üstündeki kırmızı bantta görünür.

## 3. Varlığın geri kalanı

Bunlar formdan bağımsız, normal backend işidir; mevcut `User` örneğini izle:

| Katman | Referans |
|---|---|
| Domain entity ve repository arayüzü | [User.cs](server/src/Domain/Entities/User.cs) |
| Servis | [UserService.cs](server/src/Application/Services/UserService.cs) |
| Repository, EF konfigürasyonu, `DbSet` | [ApplicationDbContext.cs](server/src/Infrastructure/Persistence/ApplicationDbContext.cs) |
| DI kayıtları | [Application](server/src/Application/DependencyInjection.cs), [Infrastructure](server/src/Infrastructure/DependencyInjection.cs) |

> **Tablo otomatik oluşmaz.** Uygulama `EnsureCreated()` kullanıyor; bu sadece veritabanı hiç yoksa çalışır, var olan veritabanına yeni tablo **eklemez**. Demo için Postgres volume'ünü sıfırla (`docker compose down -v`) ya da EF migration'a geç.

## 4. Süreç rehberi (önerilir)

`server/src/MCP/Knowledge/ders-ekleme.md`:

```markdown
---
id: ders-ekleme
title: Yeni ders nasıl eklenir?
pages: home, course-create
keywords: ders, kurs, ekle, ekleme, ders kodu, nasıl
---
Yeni ders, Ders Ekleme Formu üzerinden kaydedilir.

## Adımlar
1. Ana sayfada [@nav-course-create] butonuna tıklayın.
2. Formdaki alanları doldurun. İlk alan [@courseForm-courseName].
3. [@courseForm-submit] butonuna basın.

## Bilinmesi gerekenler
- Aynı ders kodu ile ikinci bir ders açılamaz.
```

- **Sadece süreç bilgisi yaz:** adım sırası, iş kuralları, bilinen kısıtlar. Alan listesi ve validasyon kuralları yazılmaz; asistan onları `get_page_schema` ile DTO'dan gelen manifest'ten alır.
- **Ekran elemanlarına `[@elemanId]` ile referans ver.** Sunucu bunları güncel etikete çevirir. Kimlikler: `nav-{pageId}`, `{formId}-{alan}`, `{formId}-submit`, `{formId}-reset`.
- **Uygulamada olmayan bir şeyi açıkça yaz** ("detay ekranı yoktur"). Rehber sessiz kalırsa model boşluğu doldurabilir.
- **Doğrulama açılışta yapılır.** Bilinmeyen bir sayfa ya da eleman referansı API logunda `Bilgi tabanı doğrulaması` hatası olarak görünür.

## 5. Dene

1. API'yi yeniden başlat. Manifest ve rehberler açılışta üretilir; `dotnet watch` yeni sınıfları hot reload ile almaz.
2. Logda `Manifest üretildi: N sayfa, M form` satırını ve doğrulama hatası olmadığını kontrol et.
3. `GET /api/app-manifest/forms/courseForm` ile formun alanlarına bak.
4. Uygulamada: ana sayfada "Ders Ekle" linki çıkmalı, `/course` formu açmalı.
5. Asistana sor ve cevapların altındaki 🔍 trace panelinden çağrılan tool'lara bak:
   - "Ders eklemem lazım, nasıl yapılır?" → `search_app_knowledge` → `navigate_to_page` → `highlight_element`
   - (Ana sayfadayken) "Matematik dersi, kodu MAT101, formu doldur" → `navigate_to_page` + `fill_form` (target: `courseForm`)
   - "Ders kodunun bir kuralı var mı?" → `get_page_schema`

## Özel ekran gereken formlar

Genel renderer yetmiyorsa (çok adımlı akış, özel bileşen) kendi sayfanı yazıp [App.tsx](client/src/App.tsx) içindeki `PAGE_COMPONENTS`'e `pageId` ile ekleyebilirsin; o sayfa için genel renderer yerine senin bileşenin kullanılır. Manifest, menü ve asistan tarafı aynen çalışır. Bileşende `id` ve `name` değerlerini manifest'teki `elementId` / `name` ile aynı ver, etiketleri ve validasyonu `useManifest()` + `validateFormData` ile manifest'ten oku.

## Bilinen tuzaklar

| Belirti | Neden | Çözüm |
|---|---|---|
| Form menüde/manifest'te yok | Action'da `[AppForm]` yok ya da `[FromBody]` DTO'su yok | Attribute'u ekle; açılış logundaki hataya bak |
| Alan sırası karışık | Kalıtımda türetilmiş sınıfın alanları önce gelir | Her alana `Display(Order)` ver |
| Asistan yeni formu bilmiyor | Sunucu yeniden başlatılmadı | API'yi yeniden başlat |
| Logda "Tekrarlanan path/alias" | İki form aynı `Path` / `Aliases` değerini kullanıyor | Birini değiştir |
| Logda "Bilgi tabanı doğrulaması: bilinmeyen eleman" | Rehberdeki `[@id]` bir alanın eski adını gösteriyor | Referansı üretilen kimliğe güncelle |
| Kayıt sırasında "relation does not exist" | `EnsureCreated` var olan veritabanına tablo eklemiyor | Volume'ü sıfırla veya migration kullan |
| Asistan rehberi bulamıyor | Soru kelimeleri `keywords` ile eşleşmiyor | Kullanıcıların kullandığı kelimeleri ekle |

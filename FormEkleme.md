# Bir Ekranı Asistana Tanıtmak

Bu rehber, uygulamadaki **mevcut ya da yeni** bir ekranın (örneğin "Ders Ekleme") asistan tarafından tanınmasını anlatır.
Frontend normal bir React uygulaması olarak kalır. Asistan için UI yeniden yazılmaz, formlar genel bir bileşenle çizilmez, uygulamanın state'ine dokunulmaz.

## Asistan uygulamayı nereden bilir?

| İhtiyaç | Kaynak | Senin yapman gereken |
|---|---|---|
| Şu an ekranda ne var (etiketler, değerler, zorunluluklar) | Canlı ekran özeti (DOM'dan, her istekte) | Normal HTML: input'larda `name`/`id`, `<label for>` |
| Hangi sayfalar var, nereye gidilir | Sayfa kataloğu ([aiPages.ts](client/src/app/aiPages.ts)) | Sayfa başına tek kayıt |
| Başka bir sayfanın alanları ve kuralları | O sayfanın gönderdiği endpoint'in **Swagger** şeması | Hiçbir şey (Swagger zaten üretiliyor) |
| İş nasıl yapılır | Süreç rehberleri (`Knowledge/*.md`) | İsteğe bağlı, önerilir |
| Ekranda işlem (doldurma, işaretleme, gezinme) | DOM aksiyonları | Hiçbir şey |

Asistan alanlara **DOM üzerinden** yazar: değeri atar ve `input` / `change` event'lerini tetikler. Böylece sayfanın kendi `onChange`'i, sanitizasyonu (TC'de sadece rakam gibi) ve validasyonu aynen çalışır. Form `useState`, react-hook-form ya da başka bir kütüphaneyle yazılmış olabilir; asistan bunu bilmek zorunda değildir.

---

## 1. Sayfayı normal şekilde yaz

Sayfa, route ve backend endpoint'i her zamanki gibi yazılır. Asistan açısından tek bir konvansiyon var:

**Input'un `name`'i, endpoint'in DTO alan adıyla (Swagger'daki camelCase adla) aynı olsun.**

```tsx
<label htmlFor="courseName">Ders Adı <span className="required-star">*</span></label>
<input id="courseName" name="courseName" value={form.courseName} onChange={handleChange} />
```

- Ekrandaki alan, Swagger alanıyla **`data-ai-field` → `name` → `id`** sırasıyla eşlenir.
- `name` farklıysa (örneğin `name="course_title"`) sadece o input'a işaret ekle: `data-ai-field="courseName"`.
- **`<label for>`** (ya da input'u saran `<label>`) etiketin kaynağıdır. Asistan kullanıcıyla ekrandaki etiketlerle konuşur.
- Zorunlu alanlar için `required` attribute'u ya da etikette `required-star` sınıfı, ekran özetinde "zorunlu" olarak görünür.
- `id`'ler farklı sayfalarda farklı olabilir (öğretmen formunda `id="teacherFirstName"`, `name="firstName"`). Eşleme `name` üzerinden yürür.

## 2. Katalog kaydı ekle

[client/src/app/aiPages.ts](client/src/app/aiPages.ts):

```ts
{
  id: 'course-create',
  path: '/course',
  aliases: ['/ders-ekle'],
  title: 'Ders Ekleme Formu',
  description: 'Yeni ders kaydı oluşturulur.',
  module: 'Akademik',
  endpoint: 'POST /api/courses',
  elements: [
    { id: 'courseSubmit', label: 'Dersi Kaydet', kind: 'button' },
    { id: 'homeCourseLink', label: 'Ders Ekle', kind: 'link' }, // menü linki ana sayfadaysa 'home' kaydına eklenir
  ],
},
```

| Alan | Anlamı |
|---|---|
| `id` | Sayfa kimliği. Asistan `navigate_to_page` ve `get_page_schema`'da, rehberler `pages:`'da kullanır. |
| `path`, `aliases` | `App.tsx`'teki route'larla aynı olmalı (kontrol edilir). |
| `title`, `description` | Asistanın sayfayı tanıması ve araması için |
| `module` | Büyük uygulamada sayfaları gruplar (`list_app_pages`) |
| `endpoint` | Sayfanın gönderdiği istek, Swagger'daki path ile (`POST /api/courses`). Formun alanları ve kuralları buradan çözülür. Formsuz sayfada boş bırakılır. |
| `elements` | Alan olmayan ama asistanın işaret edebilmesi istenen elemanlar (butonlar, linkler, arama kutusu). JSX'te aynı `id` ile bulunmalı. **Form alanları yazılmaz**, onlar Swagger'dan gelir. |

Sonra:

```bash
cd client
npm run pages         # server/src/MCP/Manifest/app-pages.json'u günceller (commit'le)
npm run pages:check   # CI'da da çalıştırılabilir
```

`pages:check` şunları yakalar:
- Katalogdaki path'in `App.tsx`'te olmaması
- Eleman kimliğinin JSX'te olmaması
- Tekrarlanan kimlik veya path
- Rehberde bilinmeyen sayfa
- Bayat JSON

## 3. Backend'de ek iş yok

Sunucu `endpoint`'in request body şemasını uygulamanın kendi Swagger dokümanından okur ([SwaggerFormSchemaProvider.cs](server/src/API/Forms/SwaggerFormSchemaProvider.cs)). Swashbuckle DataAnnotations'ı zaten şemaya yazar:

| DTO'da | Asistanın gördüğü |
|---|---|
| `[Required]` | zorunlu |
| `[StringLength]` / `[MaxLength]` / `[MinLength]` | `maxLength` / `minLength` kuralı |
| `[RegularExpression]` | `pattern` kuralı |
| `[EmailAddress]` | e-posta formatı |
| `DateOnly` / `DateTime` | tarih |
| Kalıtım (`record CreateTeacherDto : CreateUserDto`) | Temel sınıfın alanları dahil |
| `[Display(Name, Description)]` (isteğe bağlı) | Etiket ve ipucu ([DisplaySchemaFilter.cs](server/src/API/Forms/DisplaySchemaFilter.cs)). Yoksa etiket alan adı olur. Kullanıcı o sayfadayken etiketler zaten ekrandan gelir. |

Swagger'da olmayan ya da JSON body'si olmayan bir endpoint açılışta API loguna `Manifest doğrulaması` hatası olarak düşer.

## 4. Süreç rehberi (önerilir)

`server/src/MCP/Knowledge/ders-ekleme.md`:

```markdown
---
id: ders-ekleme
title: Yeni ders nasıl eklenir?
pages: home, course-create
keywords: ders, kurs, ekle, ekleme, ders kodu, nasıl
---
## Adımlar
1. Ana sayfada [@homeCourseLink] butonuna tıklayın.
2. Formu doldurun. İlk alan [@courseName].
3. [@courseSubmit] butonuna basın.

## Bilinmesi gerekenler
- Aynı ders kodu ile ikinci bir ders açılamaz.
```

- **Sadece süreç bilgisi yaz:** adım sırası, iş kuralları, bilinen kısıtlar. Alan listesi ve kurallar yazılmaz; asistan onları Swagger'dan alır.
- **`[@...]` ile referans ver:** katalogdaki eleman kimliklerine veya Swagger alan adlarına. Alan adları farklı formlarda tekrar edebildiği için (`firstName`) referans, rehberin `pages:` listesindeki sayfalarda aranır.
- **Uygulamada olmayan bir şeyi açıkça yaz** ("detay ekranı yoktur"). Rehber sessiz kalırsa model boşluğu doldurabilir.
- **Doğrulama açılışta yapılır.** Geçersiz referanslar API loguna `Bilgi tabanı doğrulaması` hatası olarak düşer.

## 5. Dene

1. API'yi yeniden başlat (katalog, Swagger şeması ve rehberler açılışta okunur).
2. Logda `Manifest üretildi: N sayfa, M form` satırını ve doğrulama hatası olmadığını kontrol et.
3. `GET /api/app-manifest` çıktısında sayfanın alanlarının geldiğini gör.
4. Asistana sor, cevapların altındaki 🔍 trace panelinden tool'lara bak:
   - "Ders eklemem lazım, nasıl yapılır?" → `search_app_knowledge` → `navigate_to_page` → `highlight_element`
   - (Ana sayfadayken) "Matematik dersi, kodu MAT101, forma yaz" → `navigate_to_page` + `fill_fields`
   - "Ders kodunun bir kuralı var mı?" → `get_page_schema`
5. Asistanın doldurduğu değerlerin gerçekten form state'ine geçtiğini **kaydet'e basarak** doğrula.

## Bilinen sınırlar ve tuzaklar

| Belirti | Neden | Çözüm |
|---|---|---|
| Asistan alanı "ekranda yok" diyor | Input'ta `name`/`id` yok ya da DTO adıyla eşleşmiyor | `name`'i DTO alanıyla aynı yap veya `data-ai-field` ekle |
| Asistan yazdı ama değer kaydedilmedi | Özel bileşen (tarih seçici, custom select, maskeli input) native input kullanmıyor | [writeValue.ts](client/src/assistant/dom/writeValue.ts)'teki `registerFieldWriter` ile o bileşene özel yazıcı ekle |
| Başka sayfanın alanları boş geliyor | Katalogda `endpoint` yok ya da Swagger path'i farklı | Logdaki `Manifest doğrulaması` hatasına bak; endpoint'i Swagger'daki path ile yaz |
| Etiketler alan adı olarak görünüyor (başka sayfa için) | DTO'da `[Display]` yok | İsteğe bağlı olarak `[Display(Name = ...)]` ekle; kullanıcı o sayfadayken etiket zaten ekrandan gelir |
| `pages:check` "App.tsx route'larında yok" diyor | Katalog path'i ile route farklı | İkisini eşitle |
| Asistan yeni sayfayı bilmiyor | JSON güncellenmedi veya API yeniden başlatılmadı | `npm run pages`, API'yi yeniden başlat |
| Asistan rehberi bulamıyor | Soru kelimeleri `keywords` ile eşleşmiyor | Kullanıcıların kullandığı kelimeleri ekle |

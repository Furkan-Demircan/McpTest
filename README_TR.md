# 🎓 Yapay Zeka Asistanlı (MCP) Okul & Kullanıcı Yönetim Sistemi

[![Dokümantasyon](https://img.shields.io/badge/Dokümantasyon-Türkçe-red.svg)](#) [![English](https://img.shields.io/badge/Language-English-blue.svg)](README.md)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4.svg?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.0-61dafb.svg?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178c6.svg?logo=typescript)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169e1.svg?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ed.svg?logo=docker)](https://www.docker.com/)
[![Model Context Protocol](https://img.shields.io/badge/MCP-Standard-purple.svg)](https://modelcontextprotocol.io/)

> 🇬🇧 **For the English documentation, please see [README.md](README.md).**

---

## 🌟 Genel Bakış

**Okul & Kullanıcı Yönetim Sistemi**, kurumsal düzeyde **Clean Architecture** (Temiz Mimari) prensiplerini, modern **React 19** arayüzünü ve **Model Context Protocol (MCP)** standartlarını bir araya getiren kapsamlı bir web uygulamasıdır.

Sistem, **Öğrenci** ve **Öğretmen** kayıtlarının hatasız ve doğrulamalı (11 haneli TC No, e-posta benzersizliği, tarih kontrolleri vb.) bir şekilde alınmasını sağlar. Ayrıca, tarayıcı üzerinden Türkçe sesle yazma (**Web Speech API - Dikte**) yapabilen, kullanıcının sesli veya yazılı komutlarını MCP araçları üzerinden doğrudan ekrandaki form alanlarına aktaran ve sayfalar arası yönlendirme yapabilen bir **akıllı yapay zeka asistanı** barındırır.

```mermaid
flowchart LR
    subgraph Client ["Frontend (React 19 + TypeScript)"]
        UI["Web Arayüzü & Formlar"]
        STT["Web Speech API (Sesli Dikte)"]
        Widget["Akıllı Asistan Widget'ı"]
        ActionReg["Action Registry (Form Patch / Navigasyon)"]
    end

    subgraph Backend [".NET 10 Clean Architecture API"]
        API["REST Denetleyicileri"]
        AI_Svc["AI Asistan Servisi"]
        User_Svc["Kullanıcı Servisi"]
        EF["Entity Framework Core"]
    end

    subgraph MCP_Server ["MCP Sunucusu (.NET 10)"]
        MCP_Tools["MCP Araçları (FormTools, NavTools)"]
    end

    subgraph External ["Dış Servisler & Veritabanı"]
        DeepSeek["DeepSeek AI API"]
        PG[("PostgreSQL 16 DB")]
    end

    UI --> API
    Widget -->|Ses / Metin Sohbeti| AI_Svc
    STT --> Widget
    AI_Svc -->|Araç Çağrısı Döngüsü| DeepSeek
    AI_Svc -->|MCP Araçlarını Çalıştır| MCP_Server
    MCP_Server -->|Yapılandırılmış Aksiyonlar| AI_Svc
    AI_Svc -->|Form Doldurma & Navigasyon| ActionReg
    ActionReg --> UI
    API --> User_Svc
    User_Svc --> EF
    EF --> PG
```

---

## 🚀 Öne Çıkan Özellikler

1. **Öğrenci Ekleme Sayfası (`/form`, `/ogrenci`, `/student`)**:
   - Ad, soyad, 11 haneli TC kimlik numarası, e-posta, anne adı, baba adı ve doğum tarihi alanları.
   - İstemci ve sunucu (domain) katmanlarında çift yönlü doğrulama.
   - Başarılı kayıt sonrası sistem kayıt ID'si (GUID) ve özet bilgilerini içeren onay kartı.

2. **Öğretmen Ekleme Sayfası (`/teacher`, `/ogretmen`)**:
   - Kimlik ve iletişim bilgilerine ek olarak **Branş / Uzmanlık Alanı** (*Matematik, Fizik, Kimya, Biyoloji, Türkçe/Edebiyat, Tarih, Coğrafya, İngilizce, Bilişim Teknolojileri vb.*) seçimi.
   - Hazır branş öneri listesi (`datalist` & `select`) veya özel branş yazabilme imkanı.
   - Bağımsız form durumu ve yapay zeka araç entegrasyonu.

3. **Kayıt Listeleme ve Arama Sayfası (`/users`)**:
   - Ad, soyad, TC No, e-posta veya ebeveyn bilgilerine göre anlık canlı arama/filtreleme.
   - Toplam kayıt sayısı istatistiği ve anlık yenileme butonu.

4. **Sağ Altta Sabit Akıllı Asistan & Sesli Dikte**:
   - **Sesle Yazma (Speech-to-Text):** Tarayıcının Web Speech API altyapısını kullanarak Türkçe (`tr-TR`) sesli dikte desteği. Duraksamalarda dinlemeyi sürdürür ve konuşmayı metne çevirir.
   - **MCP Araç Çağırma (Tool Calling) Döngüsü:** Kullanıcı *"Öğrencinin adını Ahmet, soyadını Kaya, doğum tarihini 2004-06-18 yap"* dediğinde, DeepSeek AI backend üzerinden MCP sunucusundaki `fill_fields` aracını çağırır. Dönen `fill_fields` aksiyonu değerleri istemcide DOM üzerinden ekrandaki input'lara yazar; sayfanın kendi `onChange`'i ve validasyonu çalışır.
   - **Sayfa Yönlendirme (Navigation Action):** Kullanıcı *"Beni öğretmen ekleme sayfasına götür"* dediğinde, asistan `navigate_to_page` aracı ile kullanıcıyı istemci tarafında `/teacher` sayfasına yönlendirir.

5. **Model Context Protocol (MCP) Sunucusu**:
   - Port `5001` üzerinde Streamable HTTP transport ile `/mcp` uç noktasında çalışan bağımsız MCP servisi.
   - Sayfa tanıma, form şeması okuma, form doluluk durumu kontrolü ve form verilerini normalize ederek yama (patch) üretme araçları.

---

## 🏛️ Mimari ve Teknoloji Yığını

### A. Frontend (İstemci Katmanı)
* **Framework:** React 19, TypeScript, Vite
* **Yönlendirme:** `react-router-dom` v7 (`/form`, `/ogrenci`, `/teacher`, `/ogretmen`, `/users` vb.)
* **Durum Yönetimi:** Sayfalar kendi yerel state'lerini (`useState`) tutar; asistan uygulama state'ine hiç dokunmaz, sadece DOM üzerinden çalışır
* **Ses Tanıma:** Web Speech API (`SpeechRecognition` / `webkitSpeechRecognition`)
* **Tasarım:** Modern, cam efektli (glassmorphism), responsive CSS ve CSS değişkenleri

### B. Backend (.NET 10 Clean Architecture)
Clean Architecture prensiplerine göre ayrılmış 4 katman:
* **`Domain Katmanı`**: Dış bağımlılığı olmayan çekirdek katman.
  * Varlıklar: `User` entity'si ve domain kuralları (11 hane TC kontrolü, e-posta formatı, gelecek tarih engelleme).
  * Arayüzler: `IUserRepository`.
  * Hata Sınıfları: `DomainException`.
* **`Application Katmanı`**: İş mantığı, kullanım senaryoları ve AI orkestrasyonu.
  * DTO Modelleri: `CreateUserDto`, `UserResponseDto`.
  * Servisler: `UserService`, `AiAssistantService` (MCP araç çağrı döngüsünü yönetir).
  * Bağlam Çözümleyiciler: `ToolContextResolver`, `IToolContextRegistry`.
* **`Infrastructure Katmanı`**: Veritabanı ve dış servis entegrasyonları.
  * PostgreSQL (Npgsql) ve Entity Framework Core.
  * `DeepSeekAiProvider`: DeepSeek API entegrasyonu ve Function Calling desteği.
  * `McpClientService`: Resmi `ModelContextProtocol.Client` kütüphanesi ile MCP sunucusu bağlantısı.
* **`API Katmanı`**: HTTP sunum katmanı.
  * Denetleyiciler: `UsersController`, `AiAssistantController`, `McpController`.
  * Global Exception Middleware (merkezi hata yakalama).
  * Kök dizinde çalışan Swagger / OpenAPI dokümantasyonu (`/`).

### C. MCP Sunucusu (`server/src/MCP`)
* Bağımsız ASP.NET Core servisi.
* `ModelContextProtocol.Server` altyapısı ile Streamable HTTP transport üzerinde araçlar sağlar.

### D. Veritabanı & Konteynerizasyon
* **Veritabanı:** PostgreSQL 16 Alpine
* **Docker Compose:** Veritabanı, Backend API ve MCP sunucusunu bağımlılık ve sağlık kontrolleriyle tek komutla ayağa kaldıran orkestrasyon dosyası (`docker-compose.yml`).

---

## 📁 Proje Dizin Yapısı

```text
McpTest/
├── docker-compose.yml              # PostgreSQL, API ve MCP orkestrasyonu
├── .env.example                    # Ortam değişkenleri şablonu
├── README.md                       # İngilizce dokümantasyon
├── README_TR.md                    # Türkçe dokümantasyon (bu dosya)
├── PROMPT.md                       # Orijinal mimari gereksinimler belgesi
│
├── client/                         # Frontend Uygulaması (React 19 + TypeScript + Vite)
│   ├── src/
│   │   ├── app/
│   │   │   └── aiPages.ts          # Asistan için sayfa kataloğu (sayfalar, path'ler, endpoint'ler)
│   │   ├── assistant/              # İstemci tarafı asistan (uygulama state'ine dokunmaz)
│   │   │   ├── actions/            # fill_fields, highlight, navigation handlers
│   │   │   ├── dom/                # DOM adaptörü: findAiElement, writeValue
│   │   │   └── screenSnapshot.ts   # Her istekte gönderilen canlı ekran özeti
│   │   ├── components/
│   │   │   ├── AssistantWidget.tsx # Sesli dikte destekli AI sohbet penceresi
│   │   │   └── AssistantWidget.css
│   │   ├── pages/                  # Elle yazılmış normal React sayfaları (yerel state)
│   │   │   ├── HomePage.tsx        # Karşılama ve hızlı yönlendirme sayfası
│   │   │   ├── FormPage.tsx        # Öğrenci ekleme formu
│   │   │   ├── TeacherFormPage.tsx # Öğretmen ekleme formu
│   │   │   └── UsersListPage.tsx   # Filtrelenebilir kayıt listesi tablosu
│   │   ├── services/
│   │   │   ├── api.ts              # /api/users için REST bağlantı servisi
│   │   │   └── assistantApi.ts     # /api/assistant/chat için asistan bağlantısı
│   │   ├── App.tsx                 # Rota tanımları
│   │   └── main.tsx                # İstemci başlangıç noktası
│   ├── package.json
│   └── vite.config.ts
│
└── server/                         # .NET 10 Temiz Mimari Backend Çözümü
    ├── UserManagement.sln
    └── src/
        ├── Domain/                 # Varlıklar, Domain Doğrulamaları, Repository Arayüzleri
        ├── Application/            # DTO'lar, Servisler, AI & MCP İletişim Mantığı
        ├── Infrastructure/         # EF Core, PostgreSQL, DeepSeek AI İstemcisi, MCP İstemcisi
        ├── API/                    # Web API Controller'ları & Middleware'ler
        └── MCP/                    # Model Context Protocol Sunucusu & Araçlar
```

---

## 🛠️ Tanımlı MCP Araçları

MCP Sunucusu tarafından dışa açılan ve DeepSeek AI Asistanı tarafından kullanılan araçlar:

| Araç Adı | Parametreler | Açıklama | Hedef Çıktı |
|---|---|---|---|
| `fill_fields` | `values` | Kullanıcının ekranındaki alanlara (form alanı, arama kutusu, filtre) DOM üzerinden yazar; sayfanın kendi `onChange`'i çalışır. Anahtarlar ekran özetine, navigasyondan sonra ise hedef sayfanın Swagger alanlarına göre doğrulanır. | `type: "fill_fields"` |
| `navigate_to_page` | `page` (sayfa kimliği, path veya alias) | Kullanıcıyı katalogdaki bir sayfaya yönlendirir; istemciye her zaman kanonik path gider. | `type: "navigation"` |
| `highlight_element` | `elementId`, `message?` | Ekrandaki bir elemanı (id, `name` veya `data-ai-field`) kaydırıp vurgular, yanında kısa not gösterir. Eleman kullanıcının göreceği ekranda değilse reddedilir. | `type: "highlight"` |
| `search_app_knowledge` | `query` | `server/src/MCP/Knowledge/*.md` süreç rehberlerinde arar. | Rehberler |
| `get_page_schema` | `page` | Sayfanın katalogdaki elemanlarını ve sayfa bir endpoint'e gönderiyorsa o endpoint'in Swagger şemasındaki alanları ve kuralları döner. | Şema nesnesi |
| `list_app_pages` | `query?`, `module?` | Sayfaları konuya veya modüle göre arar (en fazla 10). Parametresiz çağrıda modül listesini döner (sayfa azsa sayfaları da). | Sayfa listesi |
| `get_current_page` | `currentPage` (enjekte edilir) | Bulunulan sayfanın kimliğini, adını ve formunu döner. | Sayfa nesnesi |

**Asistan uygulamayı değiştirmeden tanır:** frontend normal bir React uygulaması olarak kalır. Asistan bulunulan ekranı DOM'dan okur (etiketler ve değerlerle canlı ekran özeti), diğer sayfaları küçük bir sayfa kataloğundan bilir ([`client/src/app/aiPages.ts`](client/src/app/aiPages.ts): kimlik, path, başlık, modül ve sayfanın gönderdiği endpoint), formların alanlarını ve kurallarını da o endpoint'in **Swagger** şemasından alır (backend'de işaretleme gerekmez). Swagger **yalnızca şema/sözleşme kaynağı** olarak kullanılır; kullanım rehberi (nasıl yapılır, iş kuralları, kullanıcıya dönük adlandırmalar) ayrı Application Knowledge katmanındadır (`Knowledge/*.md`, `search_app_knowledge`). Ekrandaki alanlar Swagger alanlarıyla `data-ai-field` → `name` → `id` sırasıyla eşlenir. `npm run pages` kataloğu sunucuya aktarır, `npm run pages:check` route'lara ve JSX'e karşı doğrular; sunucu açılışta endpoint'leri ve rehber referanslarını doğrular. Ayrıntılar: [FormEkleme.md](FormEkleme.md).

---

## 📡 REST API Endpoint Özeti

### Kullanıcı Yönetimi (`/api/users`)
| Metot | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/users` | Yeni kullanıcı/öğrenci/öğretmen kaydeder (11 haneli TC, benzersizlik ve tarih doğrulamalarıyla). |
| `GET` | `/api/users` | Veritabanında kayıtlı tüm kullanıcıları listeler. |
| `GET` | `/api/users/{id}` | Belirtilen GUID ID'ye sahip kullanıcıyı getirir. |
| `GET` | `/api/users/by-tc/{tcNo}` | Belirtilen TC Kimlik Numarasına sahip kullanıcıyı getirir. |

### AI Asistanı (`/api/assistant`)
| Metot | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/assistant/chat` | Konuşma geçmişini, mevcut sayfayı ve aktif form verisini alır. DeepSeek function-calling ve MCP döngüsünü çalıştırıp metin cevabı ile UI aksiyonlarını döner. |

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- [Docker & Docker Desktop](https://www.docker.com/) (Önerilen)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) veya [Bun](https://bun.sh/)
- Geçerli bir [DeepSeek API Key](https://platform.deepseek.com/)

---

### Seçenek 1: Docker Compose ile Başlatma (Önerilen)

1. **Ortam Değişkenlerini Hazırlayın:**
   Kök dizindeki `.env.example` dosyasını `.env` olarak kopyalayın:
   ```bash
   cp .env.example .env
   ```
   DeepSeek API anahtarınızı girin:
   ```env
   POSTGRES_DB=usermanagement_db
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=postgrespassword
   DEEPSEEK_API_KEY=your_deepseek_api_key_here
   DEEPSEEK_BASE_URL=https://api.deepseek.com/
   DEEPSEEK_MODEL=deepseek-chat
   ```

2. **Konteynerleri Başlatın:**
   ```powershell
   docker compose up --build -d
   ```

   Servisler hazır olduğunda:
   - **PostgreSQL 16:** `localhost:5432`
   - **Backend Web API:** `http://localhost:5000` (Swagger UI: [http://localhost:5000](http://localhost:5000))
   - **MCP Server:** `http://localhost:5000/mcp`

3. **Frontend Uygulamasını Başlatın:**
   ```powershell
   cd client
   bun install   # veya: npm install
   bun run dev   # veya: npm run dev
   ```
   Tarayıcınızda **`http://localhost:5173`** adresine gidin.

---

### Seçenek 2: Docker Olmadan Yerel Ortamda Başlatma

#### 1. PostgreSQL Servisini Başlatın
PostgreSQL'in yerel olarak `5432` portunda çalıştığından ve `usermanagement_db` veritabanının mevcut olduğundan emin olun.

#### 2. Backend API ve Dahili MCP Uç Noktasını Başlatın
```powershell
cd server/src/API
dotnet run --launch-profile API
```
*API `http://localhost:5000` adresinde, MCP uç noktası ise `http://localhost:5000/mcp` adresinde kullanılabilir.*
#### 3. Frontend Uygulamasını Başlatın
```powershell
cd client
bun install
bun run dev
```
*`http://localhost:5173` üzerinde açılır.*

---

## 🎙️ Sesli Dikte (Speech-to-Text) Kullanımı

Sağ alttaki akıllı asistan widget'ında bulunan mikrofon ikonu **Web Speech API** kullanır:
- **Destekleyen Tarayıcılar:** Google Chrome, Microsoft Edge ve Chromium tabanlı tarayıcılar.
- **Dil:** Türkçe (`tr-TR`).
- **Sürekli Dinleme:** Konuşma durakladığında dahi oturumu açık tutarak kesintisiz dikte sağlar.
- **Örnek Kullanım:** Mikrofona tıklayın ve söyleyin:
  > *"Öğrenci adı Ayşe, soyadı Yılmaz, TC kimlik numarası 11223344556, doğum tarihi 12 Nisan 2003 olsun"*
  
  Asistan söylediklerinizi analiz eder, MCP `fill_fields` aracını çağırır ve ekrandaki formu anında otomatik olarak doldurur!

---

## 🧪 Derleme ve Doğrulama Komutları

Frontend ve Backend projelerini test etmek için:

```powershell
# .NET Çözümünü derleyin
dotnet build server/UserManagement.sln

# React istemcisini tip kontrolünden geçirin ve derleyin
cd client
bun run build   # veya: npm run build
```

---

## 📄 Lisans

Bu proje MIT Lisansı ile lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakabilirsiniz.

# Kişisel Bilgi ve Kullanıcı Yönetim Sistemi

Bu doküman; geliştirilen **Kişisel Bilgi ve Kullanıcı Yönetim Sistemi**'nin mimarisini, amacını, bileşenlerini ve kullanım yönergelerini kapsamlı bir şekilde açıklamaktadır.

---

## 🎯 1. Projenin Amacı

Bu projenin temel amacı; kullanıcıların kişisel bilgilerini (Ad, Soyad, TC Kimlik Numarası, E-posta, Anne Adı, Baba Adı ve Doğum Tarihi) modern, kullanıcı dostu ve doğrulamalı bir web arayüzü üzerinden toplamak; bu verileri kurumsal düzeyde kabul gören **Clean Architecture** (Temiz Mimari) prensiplerine sahip bir **.NET 8 REST API** servisi üzerinden doğrulayıp **PostgreSQL** veritabanında güvenli ve tutarlı bir şekilde saklamak ve gerektiğinde anlık arama-filtreleme yetenekleriyle listeleyebilmektir.

---

## 🏛️ 2. Mimari Yapı ve Teknoloji Yığını

Sistem, modern kurumsal yazılım standartları gözetilerek **Frontend**, **Backend**, **Veritabanı** ve **DevOps/Container** olmak üzere 4 temel katmanda kurgulanmıştır:

### A. Frontend (İstemci Katmanı)
* **Framework:** React 19, Vite, TypeScript
* **Yönlendirme (Routing):** `react-router-dom`
* **Stil & Tasarım:** Özel CSS (Açık/Koyu tema değişkenleri, responsive grid ve cam efektleri)
* **Paket Yöneticisi:** Bun / npm

### B. Backend (Sunucu Katmanı - Clean Architecture)
* **Framework:** .NET 8 (C# - ASP.NET Core Web API)
* **Mimari Prensipleri:** Clean Architecture (Onion / Hexagonal Architecture yaklaşımı)
  * **Domain Katmanı (`src/Domain`):** Dış dünyadan tamamen bağımsız çekirdek katman. `User` Entity'si, domain kural doğrulamaları (TC No formatı, gelecek tarih engelleme vb.) ve `IUserRepository` arayüzü.
  * **Application Katmanı (`src/Application`):** İş kuralları ve kullanım senaryoları (Use Cases). DTO modelleri (`CreateUserDto`, `UserResponseDto`), servis arayüzleri ve `UserService` (TC No ve E-posta benzersizlik kontrolleri).
  * **Infrastructure Katmanı (`src/Infrastructure`):** Veritabanı erişimi. Entity Framework Core, PostgreSQL (Npgsql) sürücüsü, `ApplicationDbContext`, Fluent API konfigürasyonları ve `UserRepository` implementasyonu.
  * **API Katmanı (`src/API`):** HTTP isteklerini karşılayan sunum katmanı. `UsersController`, Swagger/OpenAPI dokümantasyonu, Global Exception Middleware (merkezi hata yakalama) ve CORS yapılandırması.

### C. Veritabanı Katmanı
* **RDBMS:** PostgreSQL 16
* **Tablo & İndeksler:**
  * `Users` tablosu
  * `TcNo` üzerinde benzersiz (`UNIQUE`) indeks (mükerrer kimlik kaydını engeller)
  * `Email` üzerinde benzersiz (`UNIQUE`) indeks

### D. Konteynerleştirme & DevOps
* **Docker:** Multi-stage build optimize edilmiş .NET API imajı
* **Docker Compose:** PostgreSQL ve API servislerini tek komutla, sağlık kontrolü (`healthcheck`) ile sırayla ayağa kaldıran orkestrasyon dosyası

---

## 🖥️ 3. Sistem Bileşenleri ve Sayfalar

### 1. Ana Sayfa (`/`)
* Kullanıcıyı karşılayan modern bir karşılama ekranı.
* Form doldurma sayfasına yönlendiren **"Form Sayfasına Git →"** butonu.
* Veritabanındaki tüm kayıtları incelemeyi sağlayan **"📋 Kayıtlı Kullanıcıları Gör"** butonu.

### 2. Form Sayfası (`/form`)
* İstenen kişisel bilgi alanlarını eksiksiz içerir:
  * **Ad & Soyad:** Boş geçilemez metin alanları.
  * **TC Kimlik Numarası:** Sadece rakam kabul eden ve 11 hane kontrolü yapan özel giriş alanı.
  * **E-posta:** Regex tabanlı e-posta format doğrulaması.
  * **Anne Adı & Baba Adı:** Metin alanları.
  * **Doğum Tarihi:** Tarih seçici (HTML5 date picker).
* **Backend Entegrasyonu:** Form gönderildiğinde veriler `POST /api/users` endpoint'ine iletilir.
* **Hata Yönetimi:** Veritabanında aynı TC No veya E-posta varsa, backend'den dönen hata mesajı form üzerinde kırmızı uyarı kutusuyla gösterilir.
* **Onay Ekranı:** Başarılı kayıt sonrası backend'in ürettiği benzersiz Sistem ID'si (GUID) ve kayıt tarihiyle birlikte bilgilerin özet kartı sunulur.

### 3. Kayıt Listeleme Sayfası (`/users`)
* Veritabanındaki tüm kullanıcıları listeleyen dinamik tablo arayüzü.
* **Canlı Arama / Filtreleme:** Arama kutusuna girilen değere göre ad, soyad, TC No veya e-postaya göre anında filtreleme yapar.
* **Yenileme:** Tek tıkla veritabanındaki en güncel kayıtları çeker.
* **İstatistik:** Toplam kayıtlı kullanıcı sayısını gösterir.

### 4. Sağ Alt Akıllı Asistan Widget'ı (Floating Assistant)
* Sayfanın sağ alt köşesinde sabit duran, yanıp sönen durum ışığına sahip yuvarlak asistan butonu.
* Tıklandığında açılan şık bir sohbet penceresi.
* Hızlı soru butonları (*"Hangi bilgiler gerekli?"*, *"Kayıtları nasıl görürüm?"*, *"TC No güvenli mi?"* vb.) ve kullanıcıdan gelen sorulara anlık yardımcı olan chat akışı.

---

## 🚀 4. Kurulum ve Çalıştırma Adımları

### Seçenek 1: Docker Compose ile Tek Komutla Başlatma (Önerilen)

Tüm sistemi (PostgreSQL veritabanı + .NET Backend API) Docker üzerinde ayağa kaldırmak için projenin kök dizininde şu komutu çalıştırın:

```powershell
docker compose up --build -d
```

Servisler hazır olduğunda:
* **Backend Swagger Arayüzü:** [http://localhost:5000](http://localhost:5000)
* **PostgreSQL:** `localhost:5432` (DB: `usermanagement_db`, Kullanıcı: `postgres`)

Frontend uygulamasını başlatmak için:
```powershell
cd client\form
bun install
bun run dev
```
* **Frontend Web Sitesi:** [http://localhost:5173](http://localhost:5173)

---

### Seçenek 2: Yerel Geliştirici Ortamında Başlatma

#### 1. Backend Servisini Başlatma
```powershell
cd server\src\API
dotnet run
```
API varsayılan olarak `http://localhost:5000` (veya `appsettings.json`'da tanımlı port) üzerinde dinlemeye başlar ve Swagger arayüzü devreye girer.

#### 2. Frontend İstemcisini Başlatma
```powershell
cd client\form
bun run dev
```

---

## 📡 5. REST API Endpoint Özeti

| Metot | URL | Açıklama |
|---|---|---|
| `POST` | `/api/users` | Yeni kullanıcı kaydeder (Zorunlu alanlar, TC ve Email benzersizlik kontrolü) |
| `GET` | `/api/users` | Kayıtlı tüm kullanıcıları listeler |
| `GET` | `/api/users/{id}` | Belirtilen GUID ID'ye sahip kullanıcıyı getirir |
| `GET` | `/api/users/by-tc/{tcNo}` | Belirtilen TC Kimlik Numarasına sahip kullanıcıyı getirir |

---

## 📁 6. Dizin Yapısı

```
McpTest/
├── docker-compose.yml          # PostgreSQL ve Backend orkestrasyonu
├── .gitignore                  # Git dışlama kuralları
├── PROMPT.md                   # Proje vizyonu, mimarisi ve kullanım kılavuzu
│
├── client/                     # Frontend Uygulaması
│   └── form/
│       ├── src/
│       │   ├── components/
│       │   │   ├── AssistantWidget.tsx   # Sağ alttaki açılır asistan
│       │   │   └── AssistantWidget.css
│       │   ├── pages/
│       │   │   ├── HomePage.tsx          # Karşılama ve yönlendirme sayfası
│       │   │   ├── FormPage.tsx          # Kişisel bilgi giriş formu
│       │   │   └── UsersListPage.tsx     # Kayıtların listelendiği tablo sayfası
│       │   ├── services/
│       │   │   └── api.ts                # Backend REST API bağlantı katmanı
│       │   ├── App.tsx                   # Rota tanımları (/ , /form , /users)
│       │   └── main.tsx
│       └── package.json
│
└── server/                     # Clean Architecture Backend Servisi (.NET 8)
    ├── UserManagement.sln
    └── src/
        ├── Domain/             # Varlıklar, arayüzler ve domain kuralları
        ├── Application/        # DTO'lar, servisler ve iş mantığı
        ├── Infrastructure/     # EF Core, PostgreSQL ve Repository'ler
        └── API/                # Controllers, Middleware, Swagger ve Dockerfile
```

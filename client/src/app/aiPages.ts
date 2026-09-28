/**
 * Asistan için sayfa kataloğu: uygulamada hangi sayfaların olduğu ve her sayfanın
 * hangi endpoint'e gönderdiği. Frontend'in çalışmasını etkilemez; route'lar ve sayfalar
 * normal şekilde yazılır (App.tsx), bu dosya sadece asistana tarif eder.
 *
 * - Formun alanları ve kuralları yazılmaz: sunucu `endpoint`'in request body'sini
 *   Swagger'dan okur (server/src/API/Catalog/SwaggerFormSchemaProvider.cs).
 * - Ekranda alan, Swagger alan adıyla eşlenir: data-ai-field → name → id. Input'un
 *   name'i DTO alanıyla aynıysa ek işaret gerekmez.
 * - `elements`: alan olmayan ama asistanın işaret edebilmesi istenen elemanlar
 *   (butonlar, menü linkleri, arama kutusu); JSX'te aynı id ile bulunmalı.
 *
 * `npm run pages` bunu server/src/MCP/Catalog/app-pages.json'a aktarır.
 * Node'un type stripping'i ile çalıştırıldığı için başka modül import etmez.
 */

export interface AiPageElement {
  id: string
  label: string
  kind: 'button' | 'link' | 'input'
}

export interface AiPage {
  id: string
  path: string
  aliases: string[]
  title: string
  description: string
  module: string
  /** Sayfanın gönderdiği endpoint, örn. "POST /api/Users" (Swagger'daki path ile) */
  endpoint?: string
  elements: AiPageElement[]
}

export const aiPages: AiPage[] = [
  {
    id: 'home',
    path: '/',
    aliases: [],
    title: 'Ana Sayfa',
    description: 'Giriş noktası; öğrenci ekleme, öğretmen ekleme ve kayıt listesine buradan gidilir.',
    module: 'Okul',
    elements: [
      { id: 'homeStudentLink', label: 'Öğrenci Ekle', kind: 'link' },
      { id: 'homeTeacherLink', label: 'Öğretmen Ekle', kind: 'link' },
      { id: 'homeUsersLink', label: 'Kayıtlı Kullanıcıları Gör', kind: 'link' },
    ],
  },
  {
    id: 'student-create',
    path: '/form',
    aliases: ['/ogrenci', '/ogrenci-ekle', '/student', '/student-form', '/forma'],
    title: 'Öğrenci Ekleme Formu',
    description: 'Yeni öğrenci kaydı oluşturulur.',
    module: 'Okul',
    endpoint: 'POST /api/Users',
    elements: [
      { id: 'studentSubmit', label: 'Öğrenciyi Kaydet', kind: 'button' },
      { id: 'studentReset', label: 'Temizle', kind: 'button' },
    ],
  },
  {
    id: 'teacher-create',
    path: '/teacher',
    aliases: ['/teacher-form', '/ogretmen', '/ogretmen-ekle'],
    title: 'Öğretmen Ekleme Formu',
    description: 'Yeni öğretmen kaydı oluşturulur.',
    module: 'Okul',
    endpoint: 'POST /api/Users/teachers',
    elements: [
      { id: 'teacherSubmit', label: 'Öğretmeni Kaydet', kind: 'button' },
      { id: 'teacherReset', label: 'Temizle', kind: 'button' },
    ],
  },
  {
    id: 'users',
    path: '/users',
    aliases: ['/kayitlar', '/liste'],
    title: 'Kayıtlı Kullanıcılar',
    description: 'Kayıtlı tüm öğrenci ve öğretmenlerin listesi; arama yapılabilir.',
    module: 'Okul',
    elements: [{ id: 'usersSearch', label: 'Ad, soyad, TC No veya e-posta ile ara', kind: 'input' }],
  },
]

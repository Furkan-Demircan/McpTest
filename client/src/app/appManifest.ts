/**
 * Uygulama manifest'i: sayfalar, formlar, alanlar ve ekran elemanları için TEK kaynak.
 *
 * - İstemci: route'lar, form kayıtları, alan etiketleri ve validasyon buradan beslenir.
 * - Sunucu: `npm run manifest` bu dosyayı server/src/MCP/Manifest/app-manifest.json'a
 *   aktarır; MCP tool'ları (navigate_to_page, get_page_schema...) oradan okur.
 * - Bilgi tabanı: Knowledge/*.md dosyaları sayfa/eleman kimliklerine referans verir,
 *   alan listesi kopyalamaz. `npm run manifest:check` referansları doğrular.
 *
 * Node'un type stripping'i ile doğrudan çalıştırılabilmesi için bu dosya
 * başka modül import etmez ve yalnızca silinebilir TypeScript sözdizimi kullanır.
 */

export type FieldRule =
  | { kind: 'length'; value: number; message: string }
  | { kind: 'email'; message: string }

export interface FieldDefinition {
  name: string
  label: string
  type: 'text' | 'email' | 'date'
  elementId: string
  required: boolean
  requiredMessage?: string
  rules?: FieldRule[]
  hint?: string
}

export interface FormDefinition {
  id: string
  title: string
  pageId: string
  submitElementId: string
  resetElementId: string
  fields: FieldDefinition[]
}

export interface PageElement {
  id: string
  label: string
  kind: 'button' | 'link' | 'input'
}

export interface PageDefinition {
  id: string
  path: string
  aliases: string[]
  title: string
  description: string
  formId?: string
  elements: PageElement[]
}

export interface AppManifest {
  pages: PageDefinition[]
  forms: FormDefinition[]
}

const TC_RULE: FieldRule = {
  kind: 'length',
  value: 11,
  message: 'TC Kimlik Numarası 11 haneli olmalıdır.',
}

const EMAIL_RULE: FieldRule = {
  kind: 'email',
  message: 'Geçerli bir e-posta adresi giriniz.',
}

export const appManifest: AppManifest = {
  pages: [
    {
      id: 'home',
      path: '/',
      aliases: [],
      title: 'Ana Sayfa',
      description: 'Öğrenci ekleme, öğretmen ekleme ve kayıt listesine giriş noktası.',
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
      formId: 'studentForm',
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
      formId: 'teacherForm',
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
      elements: [
        { id: 'usersSearch', label: 'Ad, soyad, TC No veya e-posta ile ara', kind: 'input' },
      ],
    },
  ],
  forms: [
    {
      id: 'studentForm',
      title: 'Öğrenci Ekleme Formu',
      pageId: 'student-create',
      submitElementId: 'studentSubmit',
      resetElementId: 'studentReset',
      fields: [
        { name: 'firstName', label: 'Öğrenci Adı', type: 'text', elementId: 'firstName', required: true, requiredMessage: 'Öğrenci adı zorunludur.' },
        { name: 'lastName', label: 'Öğrenci Soyadı', type: 'text', elementId: 'lastName', required: true, requiredMessage: 'Öğrenci soyadı zorunludur.' },
        { name: 'tcNo', label: 'TC Kimlik Numarası', type: 'text', elementId: 'tcNo', required: true, requiredMessage: 'TC Kimlik Numarası zorunludur.', rules: [TC_RULE], hint: 'Sadece rakam kabul eder, 11 hanede durur.' },
        { name: 'email', label: 'E-posta Adresi', type: 'email', elementId: 'email', required: true, requiredMessage: 'E-posta adresi zorunludur.', rules: [EMAIL_RULE] },
        { name: 'motherName', label: 'Anne Adı', type: 'text', elementId: 'motherName', required: true, requiredMessage: 'Anne adı zorunludur.' },
        { name: 'fatherName', label: 'Baba Adı', type: 'text', elementId: 'fatherName', required: true, requiredMessage: 'Baba adı zorunludur.' },
        { name: 'birthDate', label: 'Doğum Tarihi', type: 'date', elementId: 'birthDate', required: true, requiredMessage: 'Doğum tarihi seçimi zorunludur.', hint: 'Takvimden seçilir; YYYY-MM-DD.' },
      ],
    },
    {
      id: 'teacherForm',
      title: 'Öğretmen Ekleme Formu',
      pageId: 'teacher-create',
      submitElementId: 'teacherSubmit',
      resetElementId: 'teacherReset',
      fields: [
        { name: 'firstName', label: 'Öğretmen Adı', type: 'text', elementId: 'teacherFirstName', required: true, requiredMessage: 'Öğretmen adı zorunludur.' },
        { name: 'lastName', label: 'Öğretmen Soyadı', type: 'text', elementId: 'teacherLastName', required: true, requiredMessage: 'Öğretmen soyadı zorunludur.' },
        { name: 'tcNo', label: 'TC Kimlik Numarası', type: 'text', elementId: 'teacherTcNo', required: true, requiredMessage: 'TC Kimlik Numarası zorunludur.', rules: [TC_RULE], hint: 'Sadece rakam kabul eder, 11 hanede durur.' },
        { name: 'email', label: 'E-posta Adresi', type: 'email', elementId: 'teacherEmail', required: true, requiredMessage: 'E-posta adresi zorunludur.', rules: [EMAIL_RULE] },
        { name: 'branch', label: 'Branş / Uzmanlık Alanı', type: 'text', elementId: 'teacherBranch', required: true, requiredMessage: 'Öğretmen branş / uzmanlık alanı zorunludur.', hint: 'Elle yazılabilir veya yanındaki hazır branş listesinden seçilebilir. Şu an veritabanına kaydedilmez.' },
        { name: 'motherName', label: 'Anne Adı', type: 'text', elementId: 'teacherMotherName', required: true, requiredMessage: 'Anne adı zorunludur.' },
        { name: 'fatherName', label: 'Baba Adı', type: 'text', elementId: 'teacherFatherName', required: true, requiredMessage: 'Baba adı zorunludur.' },
        { name: 'birthDate', label: 'Doğum Tarihi', type: 'date', elementId: 'teacherBirthDate', required: true, requiredMessage: 'Doğum tarihi seçimi zorunludur.', hint: 'Takvimden seçilir; YYYY-MM-DD.' },
      ],
    },
  ],
}

// --- Yardımcılar ---

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function getForm(formId: string): FormDefinition {
  const form = appManifest.forms.find((item) => item.id === formId)
  if (!form) throw new Error(`Manifest'te form yok: ${formId}`)
  return form
}

export function getPage(pageId: string): PageDefinition {
  const page = appManifest.pages.find((item) => item.id === pageId)
  if (!page) throw new Error(`Manifest'te sayfa yok: ${pageId}`)
  return page
}

export function fieldLabel(formId: string, name: string): string {
  return getForm(formId).fields.find((field) => field.name === name)?.label ?? name
}

export function elementLabel(pageId: string, elementId: string): string {
  return getPage(pageId).elements.find((element) => element.id === elementId)?.label ?? elementId
}

export function createInitialData(formId: string): Record<string, string> {
  return Object.fromEntries(getForm(formId).fields.map((field) => [field.name, '']))
}

/** Manifest kurallarına göre formu doğrular; alan adı → hata mesajı döner. */
export function validateFormData(
  formId: string,
  data: object
): Record<string, string> {
  const values = data as Record<string, unknown>
  const errors: Record<string, string> = {}

  for (const field of getForm(formId).fields) {
    const value = String(values[field.name] ?? '').trim()

    if (!value) {
      if (field.required) {
        errors[field.name] = field.requiredMessage ?? `${field.label} zorunludur.`
      }
      continue
    }

    for (const rule of field.rules ?? []) {
      const failed =
        (rule.kind === 'length' && value.length !== rule.value) ||
        (rule.kind === 'email' && !EMAIL_PATTERN.test(value))

      if (failed) {
        errors[field.name] = rule.message
        break
      }
    }
  }

  return errors
}

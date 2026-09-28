/**
 * Uygulama manifest'i sözleşmesi: sayfalar, formlar, alanlar ve ekran elemanları.
 *
 * Kaynak backend'dir. Sunucu manifest'i [AppForm] işaretli controller action'larından
 * ve DTO attribute'larından üretir (server/src/API/Forms/AppManifestBuilder.cs) ve
 * GET /api/app-manifest ile sunar; istemci açılışta çeker (ManifestProvider).
 * Bu dosyada veri yoktur, sadece tipler ve saf yardımcılar vardır.
 */

export type FieldRule =
  | { kind: 'minLength' | 'maxLength'; value: number; message: string }
  | { kind: 'pattern'; pattern: string; message: string }
  | { kind: 'email'; message: string }

export interface FieldDefinition {
  name: string
  label: string
  type: 'text' | 'email' | 'date'
  elementId: string
  required: boolean
  requiredMessage?: string | null
  rules?: FieldRule[] | null
  options?: string[] | null
  hint?: string | null
}

export interface FormDefinition {
  id: string
  title: string
  pageId: string
  module: string
  submit: { method: string; url: string }
  submitElementId: string
  resetElementId: string
  fields: FieldDefinition[]
}

export interface PageElement {
  id: string
  label: string
  kind: 'button' | 'link' | 'input'
  targetPageId?: string | null
}

export interface PageDefinition {
  id: string
  path: string
  aliases: string[]
  title: string
  description: string
  module: string
  navLabel?: string | null
  formId?: string | null
  elements: PageElement[]
}

export interface AppManifest {
  pages: PageDefinition[]
  forms: FormDefinition[]
}

// --- Yardımcılar ---

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function findPage(manifest: AppManifest, pageId: string): PageDefinition | undefined {
  return manifest.pages.find((page) => page.id === pageId)
}

export function findForm(manifest: AppManifest, formId: string): FormDefinition | undefined {
  return manifest.forms.find((form) => form.id === formId)
}

export function elementLabel(page: PageDefinition | undefined, elementId: string): string {
  return page?.elements.find((element) => element.id === elementId)?.label ?? elementId
}

/** Ana sayfa menüsü: path'i "/" olan sayfanın hedefli link elemanları. */
export function navLinks(manifest: AppManifest): { id: string; label: string; page: PageDefinition }[] {
  const home = manifest.pages.find((page) => page.path === '/')

  return (home?.elements ?? []).flatMap((element) => {
    const page = element.targetPageId ? findPage(manifest, element.targetPageId) : undefined
    return page ? [{ id: element.id, label: element.label, page }] : []
  })
}

export function createInitialData(form: FormDefinition): Record<string, string> {
  return Object.fromEntries(form.fields.map((field) => [field.name, '']))
}

/** Manifest kurallarına göre formu doğrular; alan adı → hata mesajı döner. */
export function validateFormData(form: FormDefinition, data: object): Record<string, string> {
  const values = data as Record<string, unknown>
  const errors: Record<string, string> = {}

  for (const field of form.fields) {
    const value = String(values[field.name] ?? '').trim()

    if (!value) {
      if (field.required) {
        errors[field.name] = field.requiredMessage ?? `${field.label} zorunludur.`
      }
      continue
    }

    const failed = (field.rules ?? []).find((rule) => {
      switch (rule.kind) {
        case 'minLength':
          return value.length < rule.value
        case 'maxLength':
          return value.length > rule.value
        case 'pattern':
          // .NET RegularExpressionAttribute tüm değeri eşler
          return !new RegExp(`^(?:${rule.pattern})$`).test(value)
        case 'email':
          return !EMAIL_PATTERN.test(value)
      }
    })

    if (failed) {
      errors[field.name] = failed.message
    }
  }

  return errors
}

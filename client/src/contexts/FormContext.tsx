import { createContext } from 'react'
import {
  createInitialData,
  findPage,
  type AppManifest,
} from '../app/appManifest'

export interface FormRegistration<T extends Record<string, unknown> = Record<string, unknown>> {
  id: string
  paths: string[]
  initialData: T
  title?: string
  sanitizeField?: (key: string, value: unknown) => unknown
}

export type FormStateDictionary = Record<string, Record<string, unknown>>

export interface FormContextType {
  // Tüm formların anlık verilerini formId bazında tutan evrensel sözlük
  forms: FormStateDictionary

  // Yeni bir formu sisteme kaydetme (manifest dışı, elle yazılmış formlar için)
  registerForm: <T extends Record<string, unknown>>(config: FormRegistration<T>) => void

  // Form kaydını kaldırma
  unregisterForm: (id: string) => void

  // URL rotasına (pathname) göre form ID'si bulma
  getFormIdByPath: (pathname: string) => string | undefined

  // Belirli bir form ID veya pathname için form verisini getirme
  getFormData: (formIdOrPath: string) => Record<string, unknown> | undefined

  // Belirli bir forma kısmi yama (patch) uygulama (AI tarafından gelen verileri aktarma)
  patchFormData: (formIdOrPath: string, patch: Record<string, unknown>) => void

  // Belirli bir formun tüm verilerini güncelleme
  updateFormData: <T extends Record<string, unknown>>(
    formIdOrPath: string,
    dataOrUpdater: T | ((prev: T) => T)
  ) => void

  // Kayıtlı tüm form konfigürasyonları
  formConfigs: Record<string, FormRegistration>
}

/**
 * Manifest'teki formların kayıtları. Alias'lar route seviyesinde kanonik path'e
 * yönlendirildiği için formun tek path'i, bağlı olduğu sayfanın path'idir.
 */
export function formRegistrationsFrom(manifest: AppManifest): Record<string, FormRegistration> {
  return Object.fromEntries(
    manifest.forms.map((form) => [
      form.id,
      {
        id: form.id,
        paths: [findPage(manifest, form.pageId)?.path ?? ''],
        initialData: createInitialData(form),
        title: form.title,
      },
    ])
  )
}

export const FormContext =
  createContext<FormContextType | undefined>(undefined)

import {
  createContext,
  type Dispatch,
  type SetStateAction,
} from 'react'

export interface FormData {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string
}

export type StudentFormData = FormData

export interface TeacherFormData {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  branch: string
  motherName: string
  fatherName: string
  birthDate: string
}

export const initialFormData: FormData = {
  firstName: '',
  lastName: '',
  tcNo: '',
  email: '',
  motherName: '',
  fatherName: '',
  birthDate: '',
}

export const initialStudentFormData: FormData = initialFormData

export const initialTeacherFormData: TeacherFormData = {
  firstName: '',
  lastName: '',
  tcNo: '',
  email: '',
  branch: '',
  motherName: '',
  fatherName: '',
  birthDate: '',
}

export interface FormRegistration<T extends Record<string, unknown> = Record<string, unknown>> {
  id: string
  paths: string[]
  initialData: T
  title?: string
  sanitizeField?: (key: string, value: unknown) => unknown
}

export type FormStateDictionary = Record<string, Record<string, unknown>>

export interface FormContextType {
  // --- GLOBAL / DİNAMİK FORM YÖNETİMİ ---
  // Tüm formların anlık verilerini formId bazında tutan evrensel sözlük
  forms: FormStateDictionary

  // Yeni bir formu sisteme kaydetme (sayfası açıldığında veya önceden)
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

  // --- GERİYE DÖNÜK UYUMLULUK (Mevcut sayfalar için) ---
  formData: FormData
  setFormData: Dispatch<SetStateAction<FormData>>
  studentFormData: FormData
  setStudentFormData: Dispatch<SetStateAction<FormData>>
  teacherFormData: TeacherFormData
  setTeacherFormData: Dispatch<SetStateAction<TeacherFormData>>
}

export const DEFAULT_FORMS: Record<string, FormRegistration> = {
  studentForm: {
    id: 'studentForm',
    paths: ['/form', '/ogrenci', '/ogrenci-ekle', '/student', '/student-form', '/forma'],
    initialData: initialStudentFormData as unknown as Record<string, unknown>,
    title: 'Öğrenci Ekleme Formu',
  },
  teacherForm: {
    id: 'teacherForm',
    paths: ['/teacher', '/teacher-form', '/ogretmen', '/ogretmen-ekle'],
    initialData: initialTeacherFormData as unknown as Record<string, unknown>,
    title: 'Öğretmen Ekleme Formu',
  },
}

export const FormContext =
  createContext<FormContextType | undefined>(undefined)
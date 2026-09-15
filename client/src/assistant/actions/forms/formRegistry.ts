import type { Dispatch, SetStateAction } from 'react'
import type {
  FormData,
  TeacherFormData,
} from '../../../contexts/FormContext'
import { createGlobalFormHandler } from './globalFormHandler'
import type { AiActionHandler } from '../types'
import type { AiAction } from '../../../services/assistantApi'

export type SupportedFormData = Record<string, unknown>

interface FormRegistryItem {
  id: string
  type: string
  paths: string[]
  getFormData: () => SupportedFormData
  getFormHandler: () => AiActionHandler
}

export interface CreateFormRegistryOptions {
  studentFormData?: FormData
  teacherFormData?: TeacherFormData
  setStudentFormData?: Dispatch<SetStateAction<FormData>>
  setTeacherFormData?: Dispatch<SetStateAction<TeacherFormData>>

  // Dinamik ve global form desteği
  getFormData?: (formIdOrPath: string) => Record<string, unknown> | undefined
  patchFormData?: (formIdOrPath: string, patch: Record<string, unknown>) => void
  getFormIdByPath?: (pathname: string) => string | undefined
}

export function createFormRegistry(options: CreateFormRegistryOptions) {
  // Eğer yeni dinamik FormContext metotları verildiyse onları kullan
  if (options.patchFormData && options.getFormData && options.getFormIdByPath) {
    const { patchFormData, getFormData, getFormIdByPath } = options

    const globalHandler = createGlobalFormHandler({
      patchFormData,
      getActivePath: () => window.location.pathname,
      getFormIdByPath,
    })

    return {
      getFormData(pathname: string): SupportedFormData | null {
        return getFormData(pathname) ?? null
      },
      getFormType(pathname: string): string | null {
        return getFormIdByPath(pathname) ?? null
      },
      getFormHandler(_pathname: string): AiActionHandler {
        return globalHandler
      },
    }
  }

  // Geriye dönük uyumluluk modu (fallback)
  const studentFormHandler = (action: AiAction) => {
    if (options.setStudentFormData && action.data) {
      options.setStudentFormData((curr) => ({
        ...curr,
        ...action.data,
        ...(action.data.tcNo ? { tcNo: String(action.data.tcNo).replace(/\D/g, '').slice(0, 11) } : {}),
      }))
    }
  }

  const teacherFormHandler = (action: AiAction) => {
    if (options.setTeacherFormData && action.data) {
      options.setTeacherFormData((curr) => ({
        ...curr,
        ...action.data,
        ...(action.data.tcNo ? { tcNo: String(action.data.tcNo).replace(/\D/g, '').slice(0, 11) } : {}),
      }))
    }
  }

  const forms: FormRegistryItem[] = [
    {
      id: 'studentForm',
      paths: ['/form', '/ogrenci', '/ogrenci-ekle', '/student', '/student-form', '/forma'],
      type: 'student',
      getFormData: () => (options.studentFormData ?? {}) as unknown as SupportedFormData,
      getFormHandler: () => studentFormHandler,
    },
    {
      id: 'teacherForm',
      paths: ['/teacher', '/teacher-form', '/ogretmen', '/ogretmen-ekle'],
      type: 'teacher',
      getFormData: () => (options.teacherFormData ?? {}) as unknown as SupportedFormData,
      getFormHandler: () => teacherFormHandler,
    },
  ]

  return {
    getFormData(pathname: string): SupportedFormData | null {
      const form = forms.find((item) => item.paths.includes(pathname))
      return form?.getFormData() ?? null
    },
    getFormType(pathname: string) {
      const form = forms.find((item) => item.paths.includes(pathname))
      return form?.type ?? null
    },
    getFormHandler(pathname: string) {
      const form = forms.find((item) => item.paths.includes(pathname))
      return form?.getFormHandler() ?? null
    },
  }
}
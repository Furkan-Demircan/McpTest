import { useContext, useEffect } from 'react'
import {
  FormContext,
  type FormContextType,
  type FormRegistration,
} from './FormContext'

export const useFormContext = (): FormContextType => {
  const context = useContext(FormContext)

  if (!context) {
    throw new Error('useFormContext must be used within a FormProvider')
  }

  return context
}

export interface UseRegisteredFormReturn<T extends Record<string, unknown>> {
  data: T
  setData: (updater: T | ((prev: T) => T)) => void
  patchData: (patch: Partial<T>) => void
  setField: <K extends keyof T>(key: K, value: T[K]) => void
  resetData: () => void
}

/**
 * Sayfada kullanılan herhangi bir formu otomatik olarak sisteme ve AI Asistan'a kaydeder.
 * Sayfada 10 farklı form dahi olsa, her form yalnızca bu hook ile kendisini sisteme tanıtır.
 *
 * @example
 * const { data, patchData, setField } = useRegisteredForm({
 *   id: 'courseForm',
 *   paths: ['/course', '/ders-ekle'],
 *   initialData: { courseName: '', credits: 3 }
 * })
 */
export function useRegisteredForm<T extends Record<string, unknown>>(
  config: FormRegistration<T>
): UseRegisteredFormReturn<T> {
  const { registerForm, forms, patchFormData, updateFormData } = useFormContext()

  useEffect(() => {
    registerForm(config)
  }, [config.id, registerForm])

  const data = (forms[config.id] as T) ?? config.initialData

  const setData = (updater: T | ((prev: T) => T)) => {
    updateFormData(config.id, updater)
  }

  const patchData = (patch: Partial<T>) => {
    patchFormData(config.id, patch as Record<string, unknown>)
  }

  const setField = <K extends keyof T>(key: K, value: T[K]) => {
    patchFormData(config.id, { [key as string]: value })
  }

  const resetData = () => {
    updateFormData(config.id, { ...config.initialData })
  }

  return {
    data,
    setData,
    patchData,
    setField,
    resetData,
  }
}
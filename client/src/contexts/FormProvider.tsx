import React, { useState, useCallback, useMemo, type ReactNode } from 'react'
import {
  FormContext,
  formRegistrationsFrom,
  type FormRegistration,
  type FormStateDictionary,
} from './FormContext'
import { useManifest } from '../app/useManifest'

interface FormProviderProps {
  children: ReactNode
}

export const FormProvider: React.FC<FormProviderProps> = ({ children }) => {
  // ManifestProvider manifest yüklenmeden render etmediği için ilk state'te hazırdır.
  const manifest = useManifest()
  const [formConfigs, setFormConfigs] = useState<Record<string, FormRegistration>>(() =>
    formRegistrationsFrom(manifest)
  )

  const [forms, setForms] = useState<FormStateDictionary>(() => {
    const initial: FormStateDictionary = {}
    for (const [key, config] of Object.entries(formRegistrationsFrom(manifest))) {
      initial[key] = { ...config.initialData }
    }
    return initial
  })

  const registerForm = useCallback(<T extends Record<string, unknown>>(config: FormRegistration<T>) => {
    setFormConfigs((prev) => ({
      ...prev,
      [config.id]: config as unknown as FormRegistration,
    }))

    setForms((prev) => {
      if (prev[config.id]) {
        return prev
      }
      return {
        ...prev,
        [config.id]: { ...config.initialData },
      }
    })
  }, [])

  const unregisterForm = useCallback((id: string) => {
    setFormConfigs((prev) => {
      const next = { ...prev }
      delete next[id]
      return next
    })
  }, [])

  const getFormIdByPath = useCallback(
    (pathname: string): string | undefined => {
      const normalizedPath = pathname.trim().toLowerCase()
      for (const config of Object.values(formConfigs)) {
        if (config.paths.some((p) => p.toLowerCase() === normalizedPath)) {
          return config.id
        }
      }
      return undefined
    },
    [formConfigs]
  )

  const resolveFormId = useCallback(
    (formIdOrPath: string): string | undefined => {
      if (forms[formIdOrPath] !== undefined || formConfigs[formIdOrPath] !== undefined) {
        return formIdOrPath
      }
      return getFormIdByPath(formIdOrPath)
    },
    [forms, formConfigs, getFormIdByPath]
  )

  const getFormData = useCallback(
    (formIdOrPath: string): Record<string, unknown> | undefined => {
      const formId = resolveFormId(formIdOrPath)
      if (formId && forms[formId]) {
        return forms[formId]
      }
      return undefined
    },
    [forms, resolveFormId]
  )

  const patchFormData = useCallback(
    (formIdOrPath: string, patch: Record<string, unknown>) => {
      if (!patch || typeof patch !== 'object') return

      // Bilinmeyen hedefte sessizce başka bir forma yazmak yerine hata ver;
      // aksiyon trace'te "hata" olarak görünür.
      const formId = resolveFormId(formIdOrPath)
      if (!formId) {
        throw new Error(`Hedef form bulunamadı: '${formIdOrPath}'`)
      }
      const config = formConfigs[formId]

      setForms((prev) => {
        const currentData = prev[formId] || {}
        const updatedData: Record<string, unknown> = { ...currentData }

        for (const [key, rawValue] of Object.entries(patch)) {
          if (rawValue === undefined || rawValue === null || rawValue === '') {
            continue
          }

          let val: unknown = rawValue

          // Form konfigürasyonunda özel sanitize fonksiyonu varsa çalıştır
          if (config?.sanitizeField) {
            val = config.sanitizeField(key, val)
          }

          // Evrensel TC Kimlik No kuralı (11 hane ve sadece rakam)
          if (typeof val === 'string' && (key.toLowerCase().includes('tcno') || key.toLowerCase() === 'tc')) {
            val = val.replace(/\D/g, '').slice(0, 11)
          }

          updatedData[key] = val
        }

        return {
          ...prev,
          [formId]: updatedData,
        }
      })
    },
    [formConfigs, resolveFormId]
  )

  const updateFormData = useCallback(
    <T extends Record<string, unknown>>(
      formIdOrPath: string,
      dataOrUpdater: T | ((prev: T) => T)
    ) => {
      const formId = resolveFormId(formIdOrPath)
      if (!formId) {
        throw new Error(`Hedef form bulunamadı: '${formIdOrPath}'`)
      }

      setForms((prev) => {
        const current = (prev[formId] || {}) as unknown as T
        const next =
          typeof dataOrUpdater === 'function'
            ? (dataOrUpdater as (prev: T) => T)(current)
            : dataOrUpdater

        return {
          ...prev,
          [formId]: next,
        }
      })
    },
    [resolveFormId]
  )

  const contextValue = useMemo(
    () => ({
      forms,
      registerForm,
      unregisterForm,
      getFormIdByPath,
      getFormData,
      patchFormData,
      updateFormData,
      formConfigs,
    }),
    [
      forms,
      registerForm,
      unregisterForm,
      getFormIdByPath,
      getFormData,
      patchFormData,
      updateFormData,
      formConfigs,
    ]
  )

  return <FormContext.Provider value={contextValue}>{children}</FormContext.Provider>
}
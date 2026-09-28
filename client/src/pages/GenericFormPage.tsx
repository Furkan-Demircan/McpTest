import React, { useState } from 'react'
import { Link } from 'react-router-dom'
import {
  createInitialData,
  elementLabel,
  findForm,
  findPage,
  validateFormData,
  type FieldDefinition,
} from '../app/appManifest'
import { useManifest } from '../app/useManifest'
import { useFormContext } from '../contexts/useFormContext'
import { api } from '../services/api'
import './FormPage.css'

// Başarı kartında alan etiketi olmayan cevap alanları
const RESPONSE_LABELS: Record<string, string> = {
  id: 'Sistem Kayıt ID',
  createdAt: 'Kayıt Tarihi',
}

interface GenericFormPageProps {
  formId: string
}

/**
 * Manifest'teki bir formu render eder: alanlar, etiketler, zorunluluklar, validasyon ve
 * submit endpoint'i sunucunun DTO'dan ürettiği tanımdan gelir. Yeni form için istemci
 * kodu yazılmaz. Eleman kimlikleri manifest'ten geldiği için ekran özeti ve asistanın
 * işaretleme/doldurma aksiyonları ek bir iş gerektirmeden çalışır.
 */
function GenericFormPage({ formId }: GenericFormPageProps) {
  const manifest = useManifest()
  const form = findForm(manifest, formId)
  const page = form ? findPage(manifest, form.pageId) : undefined
  const { forms, updateFormData } = useFormContext()

  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isLoading, setIsLoading] = useState(false)
  const [backendError, setBackendError] = useState<string | null>(null)
  const [saved, setSaved] = useState<Record<string, unknown> | null>(null)

  if (!form) {
    return <div className="form-page-container">Form tanımı bulunamadı: {formId}</div>
  }

  const data = (forms[form.id] ?? createInitialData(form)) as Record<string, string>

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    // TC gibi "sadece rakam" desenli alanlarda rakam dışını ayıkla
    const field = form.fields.find((item) => item.name === name)
    const digitsOnly = field?.rules?.some((rule) => rule.kind === 'pattern' && /^\^?\\d\{\d+\}\$?$/.test(rule.pattern))
    const nextValue = digitsOnly ? value.replace(/\D/g, '') : value

    updateFormData<Record<string, string>>(form.id, (prev) => ({ ...prev, [name]: nextValue }))

    if (errors[name]) {
      setErrors((prev) => {
        const next = { ...prev }
        delete next[name]
        return next
      })
    }
    if (backendError) setBackendError(null)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const newErrors = validateFormData(form, data)
    setErrors(newErrors)
    if (Object.keys(newErrors).length > 0) {
      window.scrollTo({ top: 0, behavior: 'smooth' })
      return
    }

    setIsLoading(true)
    setBackendError(null)

    try {
      const payload = Object.fromEntries(
        form.fields.map((field) => [field.name, String(data[field.name] ?? '').trim()])
      )
      setSaved(await api.submitForm(form, payload))
    } catch (err: unknown) {
      setBackendError(err instanceof Error ? err.message : 'Sunucuya bağlanırken bir hata oluştu.')
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } finally {
      setIsLoading(false)
    }
  }

  const handleReset = () => {
    updateFormData(form.id, createInitialData(form))
    setErrors({})
    setBackendError(null)
    setSaved(null)
  }

  const labelFor = (key: string) =>
    form.fields.find((field) => field.name === key)?.label ?? RESPONSE_LABELS[key] ?? key

  return (
    <div className="form-page-container">
      <div className="form-page-header">
        <Link to="/" className="back-link">
          ← Ana Sayfaya Dön
        </Link>
        <h1>{form.title}</h1>
        {page?.description && <p className="form-description">{page.description}</p>}
      </div>

      {backendError && (
        <div className="backend-error-banner" role="alert">
          <span className="error-icon">⚠️</span>
          <span>{backendError}</span>
        </div>
      )}

      {saved ? (
        <div className="success-card">
          <div className="success-icon">✓</div>
          <h2>Kayıt Oluşturuldu!</h2>

          <div className="submitted-info-grid">
            {Object.entries(saved).map(([key, value]) => (
              <div className="info-item" key={key}>
                <span className="info-label">{labelFor(key)}:</span>
                <span className={`info-value ${key === 'id' ? 'id-badge' : ''}`}>
                  {key === 'createdAt' ? new Date(String(value)).toLocaleString('tr-TR') : String(value)}
                </span>
              </div>
            ))}
          </div>

          <div className="success-actions">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Yeni Kayıt
            </button>
            <Link to="/" className="btn-primary">
              Ana Sayfaya Git
            </Link>
          </div>
        </div>
      ) : (
        <form className="user-form" onSubmit={handleSubmit} noValidate>
          <div className="form-grid">
            {form.fields.map((field) => (
              <FormField
                key={field.name}
                field={field}
                value={data[field.name] ?? ''}
                error={errors[field.name]}
                disabled={isLoading}
                onChange={handleChange}
              />
            ))}
          </div>

          <div className="form-buttons">
            <button
              id={form.resetElementId}
              type="button"
              className="btn-secondary"
              onClick={handleReset}
              disabled={isLoading}
            >
              {elementLabel(page, form.resetElementId)}
            </button>
            <button id={form.submitElementId} type="submit" className="btn-primary" disabled={isLoading}>
              {isLoading ? 'Kaydediliyor...' : elementLabel(page, form.submitElementId)}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}

interface FormFieldProps {
  field: FieldDefinition
  value: string
  error?: string
  disabled: boolean
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void
}

function FormField({ field, value, error, disabled, onChange }: FormFieldProps) {
  const optionsId = field.options?.length ? `${field.elementId}-options` : undefined
  const maxLength = field.rules?.find((rule) => rule.kind === 'maxLength')
  const pattern = field.rules?.find((rule) => rule.kind === 'pattern')
  const fixedLength = pattern?.kind === 'pattern' ? /\{(\d+)\}\$?$/.exec(pattern.pattern)?.[1] : undefined

  return (
    <div
      className={`form-group ${field.type === 'date' || optionsId ? 'full-width' : ''} ${error ? 'has-error' : ''}`}
    >
      <label htmlFor={field.elementId}>
        {field.label} {field.required && <span className="required-star">*</span>}
      </label>
      <input
        id={field.elementId}
        name={field.name}
        type={field.type}
        list={optionsId}
        maxLength={
          maxLength?.kind === 'maxLength' ? maxLength.value : fixedLength ? Number(fixedLength) : undefined
        }
        value={value}
        onChange={onChange}
        disabled={disabled}
      />
      {optionsId && (
        <datalist id={optionsId}>
          {field.options?.map((option) => (
            <option key={option} value={option} />
          ))}
        </datalist>
      )}
      {field.hint && !error && <span className="field-hint">{field.hint}</span>}
      {error && <span className="error-text">{error}</span>}
    </div>
  )
}

export default GenericFormPage

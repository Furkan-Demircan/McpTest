import React, { useState } from 'react'
import { useFormContext } from '../contexts/useFormContext'
import { Link } from 'react-router-dom'
import {
  initialFormData,
} from '../contexts/FormContext'
import { api, type UserResponse } from '../services/api'
import './FormPage.css'


interface FormErrors {
  [key: string]: string
}

function FormPage() {
const { formData, setFormData } = useFormContext()
  const [errors, setErrors] = useState<FormErrors>({})
  const [isLoading, setIsLoading] = useState<boolean>(false)
  const [backendError, setBackendError] = useState<string | null>(null)
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false)
  const [savedUser, setSavedUser] = useState<UserResponse | null>(null)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target

    if (name === 'tcNo') {
      // Sadece rakam ve max 11 hane
      const numericValue = value.replace(/\D/g, '').slice(0, 11)
      setFormData((prev) => ({ ...prev, [name]: numericValue }))
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }))
    }

    // Değişiklik yapıldığında ilgili hatayı temizle
    if (errors[name]) {
      setErrors((prev) => {
        const next = { ...prev }
        delete next[name]
        return next
      })
    }
    if (backendError) {
      setBackendError(null)
    }
  }

  const validate = (): boolean => {
    const newErrors: FormErrors = {}

    const firstName = (formData.firstName || '').trim()
    const lastName = (formData.lastName || '').trim()
    const tcNo = (formData.tcNo || '').trim()
    const email = (formData.email || '').trim()
    const motherName = (formData.motherName || '').trim()
    const fatherName = (formData.fatherName || '').trim()
    const birthDate = (formData.birthDate || '').trim()

    if (!firstName) {
      newErrors.firstName = 'Ad alanı zorunludur.'
    }
    if (!lastName) {
      newErrors.lastName  = 'Soyad alanı zorunludur.'
    }

    if (!tcNo) {
      newErrors.tcNo = 'TC Kimlik Numarası zorunludur.'
    } else if (tcNo.length !== 11) {
      newErrors.tcNo = 'TC Kimlik Numarası 11 haneli olmalıdır.'
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!email) {
      newErrors.email = 'E-posta adresi zorunludur.'
    } else if (!emailRegex.test(email)) {
      newErrors.email = 'Geçerli bir e-posta adresi giriniz.'
    }

    if (!motherName) {
      newErrors.motherName = 'Anne adı zorunludur.'
    }

    if (!fatherName) {
      newErrors.fatherName = 'Baba adı zorunludur.'
    }

    if (!birthDate) {
      newErrors.birthDate = 'Doğum tarihi seçimi zorunludur.'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!validate()) {
      window.scrollTo({ top: 0, behavior: 'smooth' })
      return
    }

    setIsLoading(true)
    setBackendError(null)

    try {
      // Backend'e HTTP POST isteği
      const result = await api.createUser({
        firstName: (formData.firstName || '').trim(),
        lastName: (formData.lastName   || '').trim(),
        tcNo: (formData.tcNo || '').trim(),
        email: (formData.email || '').trim(),
        motherName: (formData.motherName || '').trim(),
        fatherName: (formData.fatherName || '').trim(),
        birthDate: formData.birthDate || '',
      })

      setSavedUser(result)
      setIsSubmitted(true)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Sunucuya bağlanırken bir hata oluştu.'
      setBackendError(message)
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } finally {
      setIsLoading(false)
    }
  }

  const handleReset = () => {
    setFormData(initialFormData)
    setErrors({})
    setBackendError(null)
    setIsSubmitted(false)
    setSavedUser(null)
  }

  return (
    <div className="form-page-container">
      <div className="form-page-header">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', flexWrap: 'wrap', gap: '10px' }}>
          <Link to="/" className="back-link" style={{ marginBottom: 0 }}>
            ← Ana Sayfaya Dön
          </Link>
          <Link to="/users" className="back-link" style={{ marginBottom: 0, background: 'var(--social-bg)', borderColor: 'var(--border)', color: 'var(--text-h)' }}>
            📋 Kayıtlı Kullanıcıları Gör
          </Link>
        </div>
        <h1>Kişisel Bilgi Formu</h1>
        <p className="form-description">
          Lütfen aşağıdaki alanları eksiksiz doldurunuz. Veriler PostgreSQL veritabanına kaydedilecektir.
        </p>
      </div>

      {backendError && (
        <div className="backend-error-banner" role="alert">
          <span className="error-icon">⚠️</span>
          <span>{backendError}</span>
        </div>
      )}

      {isSubmitted && savedUser ? (
        <div className="success-card">
          <div className="success-icon">✓</div>
          <h2>Kullanıcı Veritabanına Kaydedildi!</h2>
          <p>Kayıt bilgileri Clean Architecture backend servisi üzerinden başarıyla kaydedildi:</p>

          <div className="submitted-info-grid">
            <div className="info-item">
              <span className="info-label">Sistem Kayıt ID:</span>
              <span className="info-value id-badge">{savedUser.id}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Ad:</span>
              <span className="info-value">{savedUser.firstName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Soyad:</span>
              <span className="info-value">{savedUser.lastName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">TC Kimlik No:</span>
              <span className="info-value">{savedUser.tcNo}</span>
            </div>
            <div className="info-item">
              <span className="info-label">E-posta:</span>
              <span className="info-value">{savedUser.email}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Anne Adı:</span>
              <span className="info-value">{savedUser.motherName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Baba Adı:</span>
              <span className="info-value">{savedUser.fatherName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Doğum Tarihi:</span>
              <span className="info-value">{savedUser.birthDate}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Kayıt Tarihi:</span>
              <span className="info-value">{new Date(savedUser.createdAt).toLocaleString('tr-TR')}</span>
            </div>
          </div>

          <div className="success-actions">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Yeni Form Doldur
            </button>
            <Link to="/users" className="btn-secondary">
              📋 Kayıtları Gör
            </Link>
            <Link to="/" className="btn-primary">
              Ana Sayfaya Git
            </Link>
          </div>
        </div>
      ) : (
        <form className="user-form" onSubmit={handleSubmit} noValidate>
          <div className="form-grid">
            {/* Ad */}
            <div className={`form-group ${errors.firstName ? 'has-error' : ''}`}>
              <label htmlFor="firstName">
                Ad <span className="required-star">*</span>
              </label>
              <input
                id="firstName"
                type="text"
                name="firstName"
                placeholder="Örn: Ahmet"
                value={formData.firstName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.firstName && <span className="error-text">{errors.firstName}</span>}
            </div>

            {/* Soyad */}
            <div className={`form-group ${errors.lastName ? 'has-error' : ''}`}>
              <label htmlFor="lastName">
                Soyad <span className="required-star">*</span>
              </label>
              <input
                id="lastName"
                type="text"
                name="lastName"
                placeholder="Örn: Yılmaz"
                value={formData.lastName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.lastName && <span className="error-text">{errors.lastName}</span>}
            </div>

            {/* TC No */}
            <div className={`form-group ${errors.tcNo ? 'has-error' : ''}`}>
              <label htmlFor="tcNo">
                TC Kimlik Numarası <span className="required-star">*</span>
              </label>
              <input
                id="tcNo"
                type="text"
                name="tcNo"
                placeholder="11 haneli kimlik numarası"
                maxLength={11}
                value={formData.tcNo || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.tcNo && <span className="error-text">{errors.tcNo}</span>}
            </div>

            {/* E-posta */}
            <div className={`form-group ${errors.email ? 'has-error' : ''}`}>
              <label htmlFor="email">
                E-posta Adresi <span className="required-star">*</span>
              </label>
              <input
                id="email"
                type="email"
                name="email"
                placeholder="Örn: ahmet@example.com"
                value={formData.email || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.email && <span className="error-text">{errors.email}</span>}
            </div>

            {/* Anne Adı */}
            <div className={`form-group ${errors.motherName ? 'has-error' : ''}`}>
              <label htmlFor="motherName">
                Anne Adı <span className="required-star">*</span>
              </label>
              <input
                id="motherName"
                type="text"
                name="motherName"
                placeholder="Örn: Ayşe"
                value={formData.motherName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.motherName && <span className="error-text">{errors.motherName}</span>}
            </div>

            {/* Baba Adı */}
            <div className={`form-group ${errors.fatherName ? 'has-error' : ''}`}>
              <label htmlFor="fatherName">
                Baba Adı <span className="required-star">*</span>
              </label>
              <input
                id="fatherName"
                type="text"
                name="fatherName"
                placeholder="Örn: Mehmet"
                value={formData.fatherName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.fatherName && <span className="error-text">{errors.fatherName }</span>}
            </div>

            {/* Doğum Tarihi */}
            <div className={`form-group full-width ${errors.birthDate ? 'has-error' : ''}`}>
              <label htmlFor="birthDate">
                Doğum Tarihi <span className="required-star">*</span>
              </label>
              <input
                id="birthDate"
                type="date"
                name="birthDate"
                value={formData.birthDate || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.birthDate && <span className="error-text">{errors.birthDate}</span>}
            </div>
          </div>

          <div className="form-buttons">
            <button
              type="button"
              className="btn-secondary"
              onClick={handleReset}
              disabled={isLoading}
            >
              Temizle
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={isLoading}
            >
              {isLoading ? 'Kaydediliyor...' : 'Formu Kaydet'}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
export default FormPage

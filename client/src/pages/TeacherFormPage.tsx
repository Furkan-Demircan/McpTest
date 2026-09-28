import React, { useState } from 'react'
import { useFormContext } from '../contexts/useFormContext'
import { Link } from 'react-router-dom'
import {
  initialTeacherFormData,
} from '../contexts/FormContext'
import { api, type UserResponse } from '../services/api'
import {
  elementLabel,
  fieldLabel,
  validateFormData,
} from '../app/appManifest'
import './FormPage.css'

const FORM_ID = 'teacherForm'
const PAGE_ID = 'teacher-create'

interface FormErrors {
  [key: string]: string
}

const COMMON_BRANCHES = [
  'Matematik',
  'Fizik',
  'Kimya',
  'Biyoloji',
  'Türkçe ve Edebiyat',
  'Tarih',
  'Coğrafya',
  'İngilizce',
  'Bilişim Teknolojileri / Yazılım',
  'Müzik',
  'Görsel Sanatlar',
  'Beden Eğitimi',
  'Felsefe',
  'Rehberlik ve Psikolojik Danışmanlık',
  'Sınıf Öğretmenliği',
]

function TeacherFormPage() {
  const { teacherFormData: formData, setTeacherFormData: setFormData } = useFormContext()
  const [errors, setErrors] = useState<FormErrors>({})
  const [isLoading, setIsLoading] = useState<boolean>(false)
  const [backendError, setBackendError] = useState<string | null>(null)
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false)
  const [savedUser, setSavedUser] = useState<UserResponse | null>(null)
  const [savedBranch, setSavedBranch] = useState<string>('')

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
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
    const newErrors: FormErrors = validateFormData(FORM_ID, formData)
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
        lastName: (formData.lastName || '').trim(),
        tcNo: (formData.tcNo || '').trim(),
        email: (formData.email || '').trim(),
        motherName: (formData.motherName || '').trim(),
        fatherName: (formData.fatherName || '').trim(),
        birthDate: formData.birthDate || '',
      })

      setSavedBranch((formData.branch || '').trim())
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
    setFormData(initialTeacherFormData)
    setErrors({})
    setBackendError(null)
    setIsSubmitted(false)
    setSavedUser(null)
    setSavedBranch('')
  }

  return (
    <div className="form-page-container">
      <div className="form-page-header">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', flexWrap: 'wrap', gap: '10px' }}>
          <Link to="/" className="back-link" style={{ marginBottom: 0 }}>
            ← Ana Sayfaya Dön
          </Link>
          <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
            <Link to="/form" className="back-link" style={{ marginBottom: 0, background: 'var(--social-bg)', borderColor: 'var(--border)', color: 'var(--text-h)' }}>
              👨‍🎓 Öğrenci Ekleme Sayfası
            </Link>
            <Link to="/users" className="back-link" style={{ marginBottom: 0, background: 'var(--social-bg)', borderColor: 'var(--border)', color: 'var(--text-h)' }}>
              📋 Kayıtlı Kullanıcıları Gör
            </Link>
          </div>
        </div>
        <h1>👩‍🏫 Öğretmen Ekleme Formu</h1>
        <p className="form-description">
          Lütfen öğretmen kişisel ve branş bilgilerini eksiksiz doldurunuz. Veriler PostgreSQL veritabanına kaydedilecektir.
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
          <h2>Öğretmen Veritabanına Kaydedildi!</h2>
          <p>Öğretmen kayıt bilgileri Clean Architecture backend servisi üzerinden başarıyla kaydedildi:</p>

          <div className="submitted-info-grid">
            <div className="info-item">
              <span className="info-label">Sistem Kayıt ID:</span>
              <span className="info-value id-badge">{savedUser.id}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Kayıt Türü:</span>
              <span className="info-value">👩‍🏫 Öğretmen</span>
            </div>
            <div className="info-item">
              <span className="info-label">Öğretmen Adı:</span>
              <span className="info-value">{savedUser.firstName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Öğretmen Soyadı:</span>
              <span className="info-value">{savedUser.lastName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Branş / Uzmanlık:</span>
              <span className="info-value" style={{ color: 'var(--accent)', fontWeight: 700 }}>
                🎯 {savedBranch || 'Belirtilmedi'}
              </span>
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
            <div className="info-item full-width" style={{ gridColumn: 'span 2' }}>
              <span className="info-label">Kayıt Tarihi:</span>
              <span className="info-value">{new Date(savedUser.createdAt).toLocaleString('tr-TR')}</span>
            </div>
          </div>

          <div className="success-actions">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Yeni Öğretmen Ekle
            </button>
            <Link to="/form" className="btn-secondary">
              👨‍🎓 Öğrenci Ekle
            </Link>
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
              <label htmlFor="teacherFirstName">
                {fieldLabel(FORM_ID, 'firstName')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherFirstName"
                type="text"
                name="firstName"
                placeholder="Örn: Fatma"
                value={formData.firstName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.firstName && <span className="error-text">{errors.firstName}</span>}
            </div>

            {/* Soyad */}
            <div className={`form-group ${errors.lastName ? 'has-error' : ''}`}>
              <label htmlFor="teacherLastName">
                {fieldLabel(FORM_ID, 'lastName')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherLastName"
                type="text"
                name="lastName"
                placeholder="Örn: Kaya"
                value={formData.lastName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.lastName && <span className="error-text">{errors.lastName}</span>}
            </div>

            {/* TC No */}
            <div className={`form-group ${errors.tcNo ? 'has-error' : ''}`}>
              <label htmlFor="teacherTcNo">
                {fieldLabel(FORM_ID, 'tcNo')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherTcNo"
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
              <label htmlFor="teacherEmail">
                {fieldLabel(FORM_ID, 'email')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherEmail"
                type="email"
                name="email"
                placeholder="Örn: fatma.kaya@ogretmen.meb.gov.tr"
                value={formData.email || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.email && <span className="error-text">{errors.email}</span>}
            </div>

            {/* Branş / Uzmanlık Alanı */}
            <div className={`form-group full-width ${errors.branch ? 'has-error' : ''}`}>
              <label htmlFor="teacherBranch">
                {fieldLabel(FORM_ID, 'branch')} <span className="required-star">*</span>
              </label>
              <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
                <input
                  id="teacherBranch"
                  type="text"
                  name="branch"
                  list="branchSuggestions"
                  placeholder="Örn: Matematik, Fizik, Bilişim Teknolojileri..."
                  value={formData.branch || ''}
                  onChange={handleChange}
                  disabled={isLoading}
                  style={{ flex: '1 1 240px' }}
                />
                <datalist id="branchSuggestions">
                  {COMMON_BRANCHES.map((b) => (
                    <option key={b} value={b} />
                  ))}
                </datalist>
                <select
                  aria-label="Hazır Branş Seçimi"
                  value=""
                  onChange={(e) => {
                    if (e.target.value) {
                      setFormData((prev) => ({ ...prev, branch: e.target.value }))
                      if (errors.branch) {
                        setErrors((prev) => {
                          const next = { ...prev }
                          delete next.branch
                          return next
                        })
                      }
                    }
                  }}
                  disabled={isLoading}
                  style={{
                    padding: '12px 14px',
                    borderRadius: '8px',
                    border: '1.5px solid var(--border)',
                    background: 'var(--bg)',
                    color: 'var(--text-h)',
                    fontSize: '14px',
                    cursor: 'pointer',
                  }}
                >
                  <option value="">-- Hazır Branş Listesi --</option>
                  {COMMON_BRANCHES.map((b) => (
                    <option key={b} value={b}>
                      {b}
                    </option>
                  ))}
                </select>
              </div>
              {errors.branch && <span className="error-text">{errors.branch}</span>}
            </div>

            {/* Anne Adı */}
            <div className={`form-group ${errors.motherName ? 'has-error' : ''}`}>
              <label htmlFor="teacherMotherName">
                {fieldLabel(FORM_ID, 'motherName')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherMotherName"
                type="text"
                name="motherName"
                placeholder="Örn: Fatma"
                value={formData.motherName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.motherName && <span className="error-text">{errors.motherName}</span>}
            </div>

            {/* Baba Adı */}
            <div className={`form-group ${errors.fatherName ? 'has-error' : ''}`}>
              <label htmlFor="teacherFatherName">
                {fieldLabel(FORM_ID, 'fatherName')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherFatherName"
                type="text"
                name="fatherName"
                placeholder="Örn: Ali"
                value={formData.fatherName || ''}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.fatherName && <span className="error-text">{errors.fatherName}</span>}
            </div>

            {/* Doğum Tarihi */}
            <div className={`form-group full-width ${errors.birthDate ? 'has-error' : ''}`}>
              <label htmlFor="teacherBirthDate">
                {fieldLabel(FORM_ID, 'birthDate')} <span className="required-star">*</span>
              </label>
              <input
                id="teacherBirthDate"
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
              id="teacherReset"
              type="button"
              className="btn-secondary"
              onClick={handleReset}
              disabled={isLoading}
            >
              {elementLabel(PAGE_ID, 'teacherReset')}
            </button>
            <button
              id="teacherSubmit"
              type="submit"
              className="btn-primary"
              disabled={isLoading}
            >
              {isLoading ? 'Kaydediliyor...' : elementLabel(PAGE_ID, 'teacherSubmit')}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}

export default TeacherFormPage

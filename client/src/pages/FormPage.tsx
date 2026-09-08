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

    if (!formData.ad.trim()) {
      newErrors.ad = 'Ad alanı zorunludur.'
    }
    if (!formData.soyad.trim()) {
      newErrors.soyad = 'Soyad alanı zorunludur.'
    }

    if (!formData.tcNo.trim()) {
      newErrors.tcNo = 'TC Kimlik Numarası zorunludur.'
    } else if (formData.tcNo.length !== 11) {
      newErrors.tcNo = 'TC Kimlik Numarası 11 haneli olmalıdır.'
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!formData.email.trim()) {
      newErrors.email = 'E-posta adresi zorunludur.'
    } else if (!emailRegex.test(formData.email)) {
      newErrors.email = 'Geçerli bir e-posta adresi giriniz.'
    }

    if (!formData.anneAdi.trim()) {
      newErrors.anneAdi = 'Anne adı zorunludur.'
    }

    if (!formData.babaAdi.trim()) {
      newErrors.babaAdi = 'Baba adı zorunludur.'
    }

    if (!formData.dogumTarihi) {
      newErrors.dogumTarihi = 'Doğum tarihi seçimi zorunludur.'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!validate()) {
      return
    }

    setIsLoading(true)
    setBackendError(null)

    try {
      // Backend'e HTTP POST isteği
      const result = await api.createUser({
        firstName: formData.ad.trim(),
        lastName: formData.soyad.trim(),
        tcNo: formData.tcNo.trim(),
        email: formData.email.trim(),
        motherName: formData.anneAdi.trim(),
        fatherName: formData.babaAdi.trim(),
        birthDate: formData.dogumTarihi,
      })

      setSavedUser(result)
      setIsSubmitted(true)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setBackendError(err.message)
      } else {
        setBackendError('Sunucuya bağlanırken bir hata oluştu.')
      }
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
            <div className={`form-group ${errors.ad ? 'has-error' : ''}`}>
              <label htmlFor="ad">
                Ad <span className="required-star">*</span>
              </label>
              <input
                id="ad"
                type="text"
                name="ad"
                placeholder="Örn: Ahmet"
                value={formData.ad}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.ad && <span className="error-text">{errors.ad}</span>}
            </div>

            {/* Soyad */}
            <div className={`form-group ${errors.soyad ? 'has-error' : ''}`}>
              <label htmlFor="soyad">
                Soyad <span className="required-star">*</span>
              </label>
              <input
                id="soyad"
                type="text"
                name="soyad"
                placeholder="Örn: Yılmaz"
                value={formData.soyad}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.soyad && <span className="error-text">{errors.soyad}</span>}
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
                value={formData.tcNo}
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
                value={formData.email}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.email && <span className="error-text">{errors.email}</span>}
            </div>

            {/* Anne Adı */}
            <div className={`form-group ${errors.anneAdi ? 'has-error' : ''}`}>
              <label htmlFor="anneAdi">
                Anne Adı <span className="required-star">*</span>
              </label>
              <input
                id="anneAdi"
                type="text"
                name="anneAdi"
                placeholder="Örn: Ayşe"
                value={formData.anneAdi}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.anneAdi && <span className="error-text">{errors.anneAdi}</span>}
            </div>

            {/* Baba Adı */}
            <div className={`form-group ${errors.babaAdi ? 'has-error' : ''}`}>
              <label htmlFor="babaAdi">
                Baba Adı <span className="required-star">*</span>
              </label>
              <input
                id="babaAdi"
                type="text"
                name="babaAdi"
                placeholder="Örn: Mehmet"
                value={formData.babaAdi}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.babaAdi && <span className="error-text">{errors.babaAdi}</span>}
            </div>

            {/* Doğum Tarihi */}
            <div className={`form-group full-width ${errors.dogumTarihi ? 'has-error' : ''}`}>
              <label htmlFor="dogumTarihi">
                Doğum Tarihi <span className="required-star">*</span>
              </label>
              <input
                id="dogumTarihi"
                type="date"
                name="dogumTarihi"
                value={formData.dogumTarihi}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.dogumTarihi && <span className="error-text">{errors.dogumTarihi}</span>}
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

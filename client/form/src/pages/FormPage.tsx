import React, { useState } from 'react'
import { Link } from 'react-router-dom'
import './FormPage.css'

interface FormData {
  ad: string
  soyad: string
  tcNo: string
  email: string
  anneAdi: string
  babaAdi: string
  dogumTarihi: string
}

interface FormErrors {
  [key: string]: string
}

export const FormPage: React.FC = () => {
  const initialData: FormData = {
    ad: '',
    soyad: '',
    tcNo: '',
    email: '',
    anneAdi: '',
    babaAdi: '',
    dogumTarihi: '',
  }

  const [formData, setFormData] = useState<FormData>(initialData)
  const [errors, setErrors] = useState<FormErrors>({})
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false)
  const [submittedData, setSubmittedData] = useState<FormData | null>(null)

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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    if (validate()) {
      setSubmittedData(formData)
      setIsSubmitted(true)
    }
  }

  const handleReset = () => {
    setFormData(initialData)
    setErrors({})
    setIsSubmitted(false)
    setSubmittedData(null)
  }

  return (
    <div className="form-page-container">
      <div className="form-page-header">
        <Link to="/" className="back-link">
          ← Ana Sayfaya Dön
        </Link>
        <h1>Kişisel Bilgi Formu</h1>
        <p className="form-description">
          Lütfen aşağıdaki alanları eksiksiz doldurunuz.
        </p>
      </div>

      {isSubmitted && submittedData ? (
        <div className="success-card">
          <div className="success-icon">✓</div>
          <h2>Form Başarıyla Gönderildi!</h2>
          <p>Girmiş olduğunuz bilgiler aşağıdaki gibidir:</p>

          <div className="submitted-info-grid">
            <div className="info-item">
              <span className="info-label">Ad:</span>
              <span className="info-value">{submittedData.ad}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Soyad:</span>
              <span className="info-value">{submittedData.soyad}</span>
            </div>
            <div className="info-item">
              <span className="info-label">TC Kimlik No:</span>
              <span className="info-value">{submittedData.tcNo}</span>
            </div>
            <div className="info-item">
              <span className="info-label">E-posta:</span>
              <span className="info-value">{submittedData.email}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Anne Adı:</span>
              <span className="info-value">{submittedData.anneAdi}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Baba Adı:</span>
              <span className="info-value">{submittedData.babaAdi}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Doğum Tarihi:</span>
              <span className="info-value">{submittedData.dogumTarihi}</span>
            </div>
          </div>

          <div className="success-actions">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Yeni Form Doldur
            </button>
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
              />
              {errors.dogumTarihi && <span className="error-text">{errors.dogumTarihi}</span>}
            </div>
          </div>

          <div className="form-buttons">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Temizle
            </button>
            <button type="submit" className="btn-primary">
              Formu Kaydet
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
export default FormPage

import React, { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, type CourseResponse } from '../services/api'
import './FormPage.css'

const initialFormData = {
  courseName: '',
  courseCode: '',
  credits: '',
  description: '',
}

// Backend ile aynı kurallar (asıl doğrulama sunucuda: CreateCourseDto + Course entity)
const COURSE_CODE_PATTERN = /^[A-Z]{3}\d{3}$/
const MIN_CREDITS = 1
const MAX_CREDITS = 10
const MAX_DESCRIPTION = 500

interface FormErrors {
  [key: string]: string
}

function CourseFormPage() {
  const [formData, setFormData] = useState(initialFormData)
  const [errors, setErrors] = useState<FormErrors>({})
  const [isLoading, setIsLoading] = useState<boolean>(false)
  const [backendError, setBackendError] = useState<string | null>(null)
  const [savedCourse, setSavedCourse] = useState<CourseResponse | null>(null)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target

    // Ders kodu büyük harfle tutulur (MAT101)
    const nextValue = name === 'courseCode' ? value.toUpperCase().slice(0, 6) : value
    setFormData((prev) => ({ ...prev, [name]: nextValue }))

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

    const courseName = formData.courseName.trim()
    const courseCode = formData.courseCode.trim()
    const credits = Number(formData.credits)

    if (!courseName) {
      newErrors.courseName = 'Ders adı zorunludur.'
    }

    if (!courseCode) {
      newErrors.courseCode = 'Ders kodu zorunludur.'
    } else if (!COURSE_CODE_PATTERN.test(courseCode)) {
      newErrors.courseCode = 'Ders kodu 3 harf ve 3 rakamdan oluşmalıdır (örn. MAT101).'
    }

    if (!formData.credits) {
      newErrors.credits = 'Kredi zorunludur.'
    } else if (!Number.isInteger(credits) || credits < MIN_CREDITS || credits > MAX_CREDITS) {
      newErrors.credits = `Kredi ${MIN_CREDITS} ile ${MAX_CREDITS} arasında bir tam sayı olmalıdır.`
    }

    if (formData.description.length > MAX_DESCRIPTION) {
      newErrors.description = `Açıklama en fazla ${MAX_DESCRIPTION} karakter olabilir.`
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
      const result = await api.createCourse({
        courseName: formData.courseName.trim(),
        courseCode: formData.courseCode.trim(),
        credits: Number(formData.credits),
        description: formData.description.trim() || null,
      })
      setSavedCourse(result)
    } catch (err: unknown) {
      setBackendError(err instanceof Error ? err.message : 'Sunucuya bağlanırken bir hata oluştu.')
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } finally {
      setIsLoading(false)
    }
  }

  const handleReset = () => {
    setFormData(initialFormData)
    setErrors({})
    setBackendError(null)
    setSavedCourse(null)
  }

  return (
    <div className="form-page-container">
      <div className="form-page-header">
        <Link to="/" className="back-link">
          ← Ana Sayfaya Dön
        </Link>
        <h1>📚 Ders Ekleme Formu</h1>
        <p className="form-description">
          Yeni dersin bilgilerini giriniz. Ders kodu benzersiz olmalıdır.
        </p>
      </div>

      {backendError && (
        <div className="backend-error-banner" role="alert">
          <span className="error-icon">⚠️</span>
          <span>{backendError}</span>
        </div>
      )}

      {savedCourse ? (
        <div className="success-card">
          <div className="success-icon">✓</div>
          <h2>Ders Kaydedildi!</h2>

          <div className="submitted-info-grid">
            <div className="info-item">
              <span className="info-label">Sistem Kayıt ID:</span>
              <span className="info-value id-badge">{savedCourse.id}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Ders Adı:</span>
              <span className="info-value">{savedCourse.courseName}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Ders Kodu:</span>
              <span className="info-value">{savedCourse.courseCode}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Kredi:</span>
              <span className="info-value">{savedCourse.credits}</span>
            </div>
            {savedCourse.description && (
              <div className="info-item">
                <span className="info-label">Açıklama:</span>
                <span className="info-value">{savedCourse.description}</span>
              </div>
            )}
          </div>

          <div className="success-actions">
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Yeni Ders Ekle
            </button>
            <Link to="/" className="btn-primary">
              Ana Sayfaya Git
            </Link>
          </div>
        </div>
      ) : (
        <form className="user-form" onSubmit={handleSubmit} noValidate>
          <div className="form-grid">
            {/* Ders Adı */}
            <div className={`form-group ${errors.courseName ? 'has-error' : ''}`}>
              <label htmlFor="courseName">
                Ders Adı <span className="required-star">*</span>
              </label>
              <input
                id="courseName"
                type="text"
                name="courseName"
                placeholder="Örn: Matematik"
                maxLength={100}
                value={formData.courseName}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.courseName && <span className="error-text">{errors.courseName}</span>}
            </div>

            {/* Ders Kodu */}
            <div className={`form-group ${errors.courseCode ? 'has-error' : ''}`}>
              <label htmlFor="courseCode">
                Ders Kodu <span className="required-star">*</span>
              </label>
              <input
                id="courseCode"
                type="text"
                name="courseCode"
                placeholder="Örn: MAT101"
                maxLength={6}
                pattern="[A-Z]{3}[0-9]{3}"
                value={formData.courseCode}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.courseCode && <span className="error-text">{errors.courseCode}</span>}
            </div>

            {/* Kredi */}
            <div className={`form-group ${errors.credits ? 'has-error' : ''}`}>
              <label htmlFor="credits">
                Kredi <span className="required-star">*</span>
              </label>
              <input
                id="credits"
                type="number"
                name="credits"
                placeholder={`${MIN_CREDITS}–${MAX_CREDITS}`}
                min={MIN_CREDITS}
                max={MAX_CREDITS}
                step={1}
                value={formData.credits}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.credits && <span className="error-text">{errors.credits}</span>}
            </div>

            {/* Açıklama */}
            <div className={`form-group full-width ${errors.description ? 'has-error' : ''}`}>
              <label htmlFor="description">Açıklama</label>
              <textarea
                id="description"
                name="description"
                placeholder="İsteğe bağlı kısa açıklama"
                maxLength={MAX_DESCRIPTION}
                rows={3}
                value={formData.description}
                onChange={handleChange}
                disabled={isLoading}
              />
              {errors.description && <span className="error-text">{errors.description}</span>}
            </div>
          </div>

          <div className="form-buttons">
            <button
              id="courseReset"
              type="button"
              className="btn-secondary"
              onClick={handleReset}
              disabled={isLoading}
            >
              Temizle
            </button>
            <button
              id="courseSubmit"
              type="submit"
              className="btn-primary"
              disabled={isLoading}
            >
              {isLoading ? 'Kaydediliyor...' : 'Dersi Kaydet'}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}

export default CourseFormPage

import { zodResolver } from '@hookform/resolvers/zod'
import { format } from 'date-fns'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { z } from 'zod'
import { api, type UserResponse } from '../services/api'
import { AppFormDatePicker } from '../ui/Form/AppFormDatePicker'
import { AppFormSelect } from '../ui/Form/AppFormSelect'
import { AppFormTextField } from '../ui/Form/AppFormTextField'
import { birthDateBounds, birthDateError } from '../utils/birthDate'
import './FormPage.css'

// Bu sayfa CRM'deki formların kurulumunu taklit eder: react-hook-form + zod şeması +
// ortak AppForm* bileşenleri (MUI). Asistan alanlara bileşenlerin kaydettiği yazıcılarla
// (useAiField) yazar; DOM'a değil react-hook-form state'ine gider.

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

// Makul yaş aralığı (backend ile aynı; asıl doğrulama sunucuda)
const MIN_AGE = 18
const MAX_AGE = 70

const toIso = (date: Date) => format(date, 'yyyy-MM-dd')
const fromIso = (iso: string) => {
  const [year, month, day] = iso.split('-').map(Number)
  return new Date(year, month - 1, day)
}
const bounds = birthDateBounds(MIN_AGE, MAX_AGE)

const teacherSchema = z.object({
  firstName: z.string().trim().min(1, 'Öğretmen adı zorunludur.'),
  lastName: z.string().trim().min(1, 'Öğretmen soyadı zorunludur.'),
  tcNo: z.string().regex(/^\d{11}$/, 'TC Kimlik Numarası 11 haneli olmalıdır.'),
  email: z
    .string()
    .trim()
    .min(1, 'E-posta adresi zorunludur.')
    .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Geçerli bir e-posta adresi giriniz.'),
  branch: z
    .string()
    .nullable()
    .refine((value) => !!value, 'Öğretmen branş / uzmanlık alanı zorunludur.'),
  motherName: z.string().trim().min(1, 'Anne adı zorunludur.'),
  fatherName: z.string().trim().min(1, 'Baba adı zorunludur.'),
  birthDate: z
    .date()
    .nullable()
    .superRefine((value, ctx) => {
      if (!value) {
        ctx.addIssue({ code: 'custom', message: 'Doğum tarihi seçimi zorunludur.' })
        return
      }
      const problem = birthDateError(toIso(value), MIN_AGE, MAX_AGE, 'Öğretmen')
      if (problem) ctx.addIssue({ code: 'custom', message: problem })
    }),
})

type TeacherFormValues = z.infer<typeof teacherSchema>

const defaultValues: TeacherFormValues = {
  firstName: '',
  lastName: '',
  tcNo: '',
  email: '',
  branch: null,
  motherName: '',
  fatherName: '',
  birthDate: null,
}

function TeacherFormPage() {
  const { control, handleSubmit: submitForm, reset, formState } = useForm<TeacherFormValues>({
    resolver: zodResolver(teacherSchema),
    defaultValues,
  })
  const isLoading = formState.isSubmitting
  const [backendError, setBackendError] = useState<string | null>(null)
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false)
  const [savedUser, setSavedUser] = useState<UserResponse | null>(null)
  const [savedBranch, setSavedBranch] = useState<string>('')

  const handleSubmit = submitForm(
    async (values) => {
      setBackendError(null)
      try {
        const result = await api.createTeacher({
          firstName: values.firstName.trim(),
          lastName: values.lastName.trim(),
          tcNo: values.tcNo,
          email: values.email.trim(),
          motherName: values.motherName.trim(),
          fatherName: values.fatherName.trim(),
          birthDate: values.birthDate ? toIso(values.birthDate) : '',
          branch: values.branch ?? '',
        })
        setSavedBranch(result.branch)
        setSavedUser(result)
        setIsSubmitted(true)
      } catch (err: unknown) {
        setBackendError(err instanceof Error ? err.message : 'Sunucuya bağlanırken bir hata oluştu.')
        window.scrollTo({ top: 0, behavior: 'smooth' })
      }
    },
    () => window.scrollTo({ top: 0, behavior: 'smooth' })
  )

  const handleReset = () => {
    reset(defaultValues)
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
            <AppFormTextField control={control} name="firstName" label="Öğretmen Adı" required placeholder="Örn: Ayşe" disabled={isLoading} />
            <AppFormTextField control={control} name="lastName" label="Öğretmen Soyadı" required placeholder="Örn: Demir" disabled={isLoading} />
            <AppFormTextField
              control={control}
              name="tcNo"
              label="TC Kimlik Numarası"
              required
              placeholder="11 haneli kimlik numarası"
              transform={(value) => value.replace(/\D/g, '').slice(0, 11)}
              disabled={isLoading}
            />
            <AppFormTextField control={control} name="email" label="E-posta Adresi" required type="email" placeholder="Örn: ayse@okul.edu.tr" disabled={isLoading} />
            <div style={{ gridColumn: '1 / -1' }}>
              <AppFormSelect
                control={control}
                name="branch"
                label="Branş / Uzmanlık Alanı"
                required
                emptyOptionLabel="Seçiniz"
                options={COMMON_BRANCHES.map((branch) => ({ value: branch, label: branch }))}
                disabled={isLoading}
              />
            </div>
            <AppFormTextField control={control} name="motherName" label="Anne Adı" required placeholder="Örn: Fatma" disabled={isLoading} />
            <AppFormTextField control={control} name="fatherName" label="Baba Adı" required placeholder="Örn: Ali" disabled={isLoading} />
            <div style={{ gridColumn: '1 / -1' }}>
              <AppFormDatePicker
                control={control}
                name="birthDate"
                label="Doğum Tarihi"
                required
                minDate={fromIso(bounds.min)}
                maxDate={fromIso(bounds.max)}
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="form-buttons">
            <button id="teacherReset" type="button" className="btn-secondary" onClick={handleReset} disabled={isLoading}>
              Temizle
            </button>
            <button id="teacherSubmit" type="submit" className="btn-primary" disabled={isLoading}>
              {isLoading ? 'Kaydediliyor...' : 'Öğretmeni Kaydet'}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}

export default TeacherFormPage

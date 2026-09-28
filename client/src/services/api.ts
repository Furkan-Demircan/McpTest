import type { AppManifest, FormDefinition } from '../app/appManifest'

export interface UserResponse {
  id: string
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string
  createdAt: string
}

export interface ApiError {
  statusCode: number
  message: string
  timestamp?: string
}

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

/** Manifest'teki "/api/..." adreslerini yapılandırılmış API tabanına çevirir. */
export function resolveApiUrl(url: string): string {
  return API_BASE_URL + url.replace(/^\/api(?=\/|$)/i, '')
}

/** Sunucu hata cevabından okunabilir mesaj çıkarır ({ message } veya ModelState). */
async function readErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const errorData = await response.json()
    if (errorData.message) return errorData.message
    if (errorData.errors) return Object.values(errorData.errors).flat().join(' ')
  } catch {
    return `Sunucu hatası: ${response.status} ${response.statusText}`
  }
  return fallback
}

export const api = {
  /**
   * Koddan üretilen uygulama manifest'i (GET /api/app-manifest)
   */
  async getAppManifest(): Promise<AppManifest> {
    const response = await fetch(`${API_BASE_URL}/app-manifest`)
    if (!response.ok) {
      throw new Error(await readErrorMessage(response, 'Uygulama tanımı alınamadı.'))
    }
    return response.json()
  },

  /**
   * Formu manifest'teki submit endpoint'ine gönderir; cevap gövdesini döner.
   */
  async submitForm(form: FormDefinition, payload: Record<string, unknown>): Promise<Record<string, unknown>> {
    const response = await fetch(resolveApiUrl(form.submit.url), {
      method: form.submit.method,
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      throw new Error(await readErrorMessage(response, 'Kayıt sırasında bir hata oluştu.'))
    }

    return response.json()
  },

  /**
   * Tüm kayıtlı kullanıcıları getirir (GET /api/users)
   */
  async getUsers(): Promise<UserResponse[]> {
    const response = await fetch(`${API_BASE_URL}/users`)
    if (!response.ok) {
      throw new Error('Kullanıcılar yüklenirken bir hata oluştu.')
    }
    return response.json()
  },

  /**
   * DeepSeek AI Asistanı ile sohbet eder (POST /api/assistant/chat)
   */
  async askAssistant(message: string): Promise<string> {
    const response = await fetch(`${API_BASE_URL}/assistant/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ message }),
    })

    if (!response.ok) {
      let errorMsg = `AI servisi yanıt vermedi: ${response.statusText}`
      try {
        const errJson = await response.json()
        if (errJson.message) errorMsg = errJson.message
      } catch {
        // use default
      }
      throw new Error(errorMsg)
    }

    const data = await response.json()
    return data.message
  },
}

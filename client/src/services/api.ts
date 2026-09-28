export interface CreateUserPayload {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string // YYYY-MM-DD
}

export interface CreateTeacherPayload extends CreateUserPayload {
  branch: string
}

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

export interface TeacherResponse extends UserResponse {
  branch: string
}

export interface ApiError {
  statusCode: number
  message: string
  timestamp?: string
}

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

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

async function postJson<T>(path: string, payload: unknown, fallback: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response, fallback))
  }

  return response.json()
}

export const api = {
  /**
   * Yeni öğrenci ekler (POST /api/users)
   */
  async createUser(payload: CreateUserPayload): Promise<UserResponse> {
    return postJson('/users', payload, 'Kullanıcı kaydedilirken bir hata oluştu.')
  },

  /**
   * Yeni öğretmen ekler (POST /api/users/teachers)
   */
  async createTeacher(payload: CreateTeacherPayload): Promise<TeacherResponse> {
    return postJson('/users/teachers', payload, 'Öğretmen kaydedilirken bir hata oluştu.')
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

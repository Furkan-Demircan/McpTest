export interface CreateUserPayload {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string // YYYY-MM-DD
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

export interface ApiError {
  statusCode: number
  message: string
  timestamp?: string
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const api = {
  /**
   * Yeni kullanıcı ekler (POST /api/users)
   */
  async createUser(payload: CreateUserPayload): Promise<UserResponse> {
    const response = await fetch(`${API_BASE_URL}/users`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      let errorMessage = 'Kullanıcı kaydedilirken bir hata oluştu.'
      try {
        const errorData = await response.json()
        if (errorData.message) {
          errorMessage = errorData.message
        } else if (errorData.errors) {
          // Model validation errors
          const fieldErrors = Object.values(errorData.errors).flat()
          errorMessage = fieldErrors.join(' ')
        }
      } catch {
        errorMessage = `Sunucu hatası: ${response.status} ${response.statusText}`
      }
      throw new Error(errorMessage)
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
}

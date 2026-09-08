export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface AiChatRequest {
  messages: ChatMessage[]
}

export interface AiChatResponse {
  message: string

  formPatch: {
    firstName?: string
    lastName?: string
    tcNo?: string
    email?: string
    motherName?: string
    fatherName?: string
    birthDate?: string
  }

  missingFields: string[]
}

const API_BASE_URL =
  import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export async function sendAssistantMessage(
  messages: ChatMessage[]
): Promise<AiChatResponse> {
  const response = await fetch(
    `${API_BASE_URL}/assistant/chat`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        messages,
      }),
    }
  )

  if (!response.ok) {
    let errorMessage =
      `Asistan isteği başarısız oldu (${response.status})`

    try {
      const errorData = await response.json()

      if (errorData.message) {
        errorMessage = errorData.message
      }
    } catch {
      // JSON parse edilemezse varsayılan mesaj kalır
    }

    throw new Error(errorMessage)
  }

  return response.json()
}
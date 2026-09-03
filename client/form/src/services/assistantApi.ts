export interface AiChatRequest {
  message: string
}

export interface AiChatResponse {
  message: string
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export async function sendAssistantMessage(
  message: string
): Promise<AiChatResponse> {
  const response = await fetch(
    `${API_BASE_URL}/assistant/chat`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        message,
      }),
    }
  )

  if (!response.ok) {
    let errorMessage = `Asistan isteği başarısız oldu (${response.status})`
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
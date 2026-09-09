export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface FormData {
  ad: string
  soyad: string
  tcNo: string
  email: string
  anneAdi: string
  babaAdi: string
  dogumTarihi: string
}

export interface AiChatRequest {
  messages: ChatMessage[]
  formData: FormData
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

export async function sendAssistantMessage(
  messages: ChatMessage[],
  formData: FormData
): Promise<AiChatResponse> {
  const response = await fetch('/api/assistant/chat', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      messages,
      formData,
    }),
  })

  if (!response.ok) {
    throw new Error('Asistan mesajı gönderilemedi.')
  }

  return response.json()
}
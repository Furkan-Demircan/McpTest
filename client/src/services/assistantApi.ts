export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface FormData {
  firstName: string
  lastName: string
  tcNo: string
  email: string
  motherName: string
  fatherName: string
  birthDate: string
}

export interface AiChatRequest {
  messages: ChatMessage[]
  formData: FormData
}

export interface AiAction {
  type: string
  target?: string
  data: Record<string, unknown>
}

interface AiChatResponse {
  message: string
  missingFields: string[]
  actions: AiAction[]
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
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
  currentPage?: string
  messages: ChatMessage[]
  formData: FormData | Record<string, unknown>
}

export interface AiAction {
  type: string
  target?: string
  data: Record<string, unknown>
}

export interface AiTraceStep {
  kind: 'llm' | 'tool' | 'limit'
  iteration: number
  name?: string
  arguments?: string
  result?: string
  isError: boolean
  durationMs: number
}

interface AiChatResponse {
  message: string
  missingFields: string[]
  actions: AiAction[]
  trace: AiTraceStep[]
}

export async function sendAssistantMessage(
  messages: ChatMessage[],
  formData: FormData | Record<string, unknown>,
  currentPage?: string
): Promise<AiChatResponse> {
  const response = await fetch('/api/assistant/chat', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      currentPage,
      messages,
      formData,
    }),
  })

  if (!response.ok) {
    // GlobalExceptionMiddleware { message } döner; varsa onu göster
    const body = await response.json().catch(() => null)
    throw new Error(body?.message ?? 'Asistan mesajı gönderilemedi.')
  }

  return response.json()
}
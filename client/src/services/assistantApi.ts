import type { ScreenSnapshot } from '../assistant/screenSnapshot'

export interface ChatMessage {
  // 'event': istemcinin gözlediği uygulama olayı (sunucu modele sistem notu olarak iletir)
  role: 'user' | 'assistant' | 'event'
  content: string
}

export interface AiChatRequest {
  currentPage?: string
  // Kullanıcının gördüğü ekran (DOM); asistan uygulamanın state'ini görmez
  screen?: ScreenSnapshot
  messages: ChatMessage[]
}

export interface AiAction {
  type: string
  target?: string
  data: Record<string, unknown>
  // İstemci aksiyonun sonucunu bu tool çağrısına ait olarak geri bildirir
  toolCallId?: string
}

/** Uygulanan bir aksiyonun sunucuya bildirilen gerçek sonucu */
export interface AiActionResult {
  toolCallId: string
  status: 'applied' | 'partial' | 'failed'
  detail?: Record<string, unknown>
  error?: string
}

export interface AiTraceStep {
  kind: 'llm' | 'tool' | 'client' | 'limit'
  iteration: number
  name?: string
  arguments?: string
  result?: string
  isError: boolean
  durationMs: number
}

export interface AiChatResponse {
  // awaiting_client: actions uygulanıp sonuçları continueAssistant ile gönderilmeli
  status: 'completed' | 'awaiting_client'
  continuationId?: string
  message: string
  missingFields: string[]
  actions: AiAction[]
  trace: AiTraceStep[]
}

async function postAssistant(path: string, body: unknown): Promise<AiChatResponse> {
  const response = await fetch(`/api/assistant/${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    // GlobalExceptionMiddleware / 410 { message } döner; varsa onu göster
    const error = await response.json().catch(() => null)
    throw new Error(error?.message ?? 'Asistan mesajı gönderilemedi.')
  }

  return response.json()
}

export function sendAssistantMessage(request: AiChatRequest): Promise<AiChatResponse> {
  return postAssistant('chat', request)
}

/**
 * awaiting_client cevabındaki aksiyonların gerçek sonuçlarını ve aksiyonlardan
 * sonraki ekran özetini gönderir; sunucu turu bu sonuçlarla sürdürür.
 */
export function continueAssistant(
  continuationId: string,
  results: AiActionResult[],
  screen: ScreenSnapshot
): Promise<AiChatResponse> {
  return postAssistant('chat/continue', { continuationId, results, screen })
}
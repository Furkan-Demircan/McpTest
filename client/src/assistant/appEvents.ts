/**
 * Uygulamada gerçekten olanları asistan için gözler; sohbete "uygulama olayı" olarak girer.
 *
 * Kayıt istekleri (POST/PUT/PATCH/DELETE) ağ katmanında (fetch + XMLHttpRequest) izlenir:
 * sayfalara dokunmadan çalışır; CRM'deki axios da XHR kullandığı için aynen yakalanır.
 * Model böylece bir kaydın gönderildiğini, başarılı/başarısız olduğunu ve nedenini öğrenir;
 * önceki kaydın değerlerini yeni kayda taşımaması için kayıt sınırı olur.
 */

const MUTATING_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])
// Asistanın kendi istekleri olay değildir
const IGNORED_URL = /\/api\/assistant\//i
const MAX_MESSAGE_LENGTH = 200

export type AppEventListener = (text: string) => void

function describe(method: string, url: string, status: number, message?: string): string {
  const path = new URL(url, window.location.origin).pathname
  const outcome = status >= 200 && status < 300 ? 'başarılı' : 'başarısız'
  const reason = message ? `: ${message.slice(0, MAX_MESSAGE_LENGTH)}` : ''
  return `${window.location.pathname} sayfasında kayıt isteği ${outcome}: ${method} ${path} → ${status}${reason}`
}

function messageFrom(body: unknown): string | undefined {
  if (body && typeof body === 'object') {
    const record = body as { message?: unknown; errors?: Record<string, unknown> }
    if (typeof record.message === 'string') return record.message
    if (record.errors) return Object.values(record.errors).flat().join(' ')
  }
  return undefined
}

/** Gözlemi başlatır; dönen fonksiyon orijinal fetch/XHR'ı geri yükler. */
export function observeAppRequests(listener: AppEventListener): () => void {
  const originalFetch = window.fetch
  const originalOpen = XMLHttpRequest.prototype.open
  const originalSend = XMLHttpRequest.prototype.send

  window.fetch = async (input, init) => {
    const method = (init?.method ?? (input instanceof Request ? input.method : 'GET')).toUpperCase()
    const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url
    const response = await originalFetch(input, init)

    if (MUTATING_METHODS.has(method) && !IGNORED_URL.test(url)) {
      const failed = !response.ok
      const message = failed ? await response.clone().json().then(messageFrom).catch(() => undefined) : undefined
      listener(describe(method, url, response.status, message))
    }

    return response
  }

  type TrackedXhr = XMLHttpRequest & { __aiMethod?: string; __aiUrl?: string }

  XMLHttpRequest.prototype.open = function (this: TrackedXhr, method: string, url: string | URL, ...rest: unknown[]) {
    this.__aiMethod = method.toUpperCase()
    this.__aiUrl = String(url)
    return (originalOpen as (...args: unknown[]) => void).call(this, method, url, ...rest)
  } as typeof XMLHttpRequest.prototype.open

  XMLHttpRequest.prototype.send = function (this: TrackedXhr, body?: Document | XMLHttpRequestBodyInit | null) {
    const { __aiMethod: method, __aiUrl: url } = this
    if (method && url && MUTATING_METHODS.has(method) && !IGNORED_URL.test(url)) {
      this.addEventListener('loadend', () => {
        let message: string | undefined
        if (this.status < 200 || this.status >= 300) {
          try {
            message = messageFrom(JSON.parse(this.responseText))
          } catch {
            message = undefined
          }
        }
        listener(describe(method, url, this.status, message))
      })
    }
    return originalSend.call(this, body)
  }

  return () => {
    window.fetch = originalFetch
    XMLHttpRequest.prototype.open = originalOpen
    XMLHttpRequest.prototype.send = originalSend
  }
}

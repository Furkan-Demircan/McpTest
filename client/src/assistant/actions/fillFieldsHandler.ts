import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler, ActionOutcome } from './types'
import { waitForElement } from './waitForElement'
import { writeValue } from '../dom/writeValue'

/**
 * fill_fields aksiyonu: kullanıcının ekranındaki alanlara yazar (DOM).
 * Uygulamanın state'ine dokunmaz; sayfanın kendi onChange'i çalışır, böylece
 * sayfanın validasyonu/sanitizasyonu (örn. TC'de sadece rakam) aynen uygulanır.
 * Hangi alanın yazılıp hangisinin yazılamadığı modele geri bildirilir. Yazılan değer
 * sayfanın HTML kısıtlarına (min/max, pattern, maxlength...) uymuyorsa tarayıcının kendi
 * doğrulama mesajıyla "invalid" olarak bildirilir.
 */
export function createFillFieldsHandler(): AiActionHandler {
  return async (action: AiAction): Promise<ActionOutcome> => {
    const values = action.data.values

    if (!values || typeof values !== 'object') {
      return { status: 'failed', error: 'fill_fields: values yok' }
    }

    const written: string[] = []
    const notFound: string[] = []
    const notWritable: string[] = []
    // Bileşenin yazmayı reddetme nedeni (örn. "seçeneklerde yok", "tarih aralık dışında")
    const reasons: Record<string, string> = {}
    const invalid: { field: string; message: string }[] = []

    for (const [reference, value] of Object.entries(values as Record<string, unknown>)) {
      const element = await waitForElement(reference)

      if (!element) notFound.push(reference)
      else {
        const result = writeValue(element, value)
        if (result.ok) {
          written.push(reference)
          const problem = constraintProblem(element)
          if (problem) invalid.push({ field: reference, message: problem })
        } else {
          notWritable.push(reference)
          reasons[reference] = result.reason
        }
      }
    }

    const status = notFound.length + notWritable.length + invalid.length === 0
      ? 'applied'
      : written.length > 0
        ? 'partial'
        : 'failed'

    return { status, detail: { written, notFound, notWritable, reasons, invalid } }
  }
}

/** Alan, sayfanın HTML kısıtlarına uymuyorsa tarayıcının doğrulama mesajı. */
function constraintProblem(element: HTMLElement): string | undefined {
  if (
    (element instanceof HTMLInputElement ||
      element instanceof HTMLSelectElement ||
      element instanceof HTMLTextAreaElement) &&
    !element.validity.valid &&
    !element.validity.valueMissing
  ) {
    return element.validationMessage || 'Değer alanın kısıtlarına uymuyor.'
  }
  return undefined
}

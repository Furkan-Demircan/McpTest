import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler, ActionOutcome } from './types'
import { waitForElement } from './waitForElement'
import { writeValue } from '../dom/writeValue'

/**
 * fill_fields aksiyonu: kullanıcının ekranındaki alanlara yazar (DOM).
 * Uygulamanın state'ine dokunmaz; sayfanın kendi onChange'i çalışır, böylece
 * sayfanın validasyonu/sanitizasyonu (örn. TC'de sadece rakam) aynen uygulanır.
 * Hangi alanın yazılıp hangisinin yazılamadığı modele geri bildirilir.
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

    for (const [reference, value] of Object.entries(values as Record<string, unknown>)) {
      const element = await waitForElement(reference)

      if (!element) notFound.push(reference)
      else if (writeValue(element, value)) written.push(reference)
      else notWritable.push(reference)
    }

    const status = notFound.length + notWritable.length === 0
      ? 'applied'
      : written.length > 0
        ? 'partial'
        : 'failed'

    return { status, detail: { written, notFound, notWritable } }
  }
}

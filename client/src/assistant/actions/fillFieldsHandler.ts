import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler } from './types'
import { waitForElement } from './waitForElement'
import { writeValue } from '../dom/writeValue'

/**
 * fill_fields aksiyonu: kullanıcının ekranındaki alanlara yazar (DOM).
 * Uygulamanın state'ine dokunmaz; sayfanın kendi onChange'i çalışır, böylece
 * sayfanın validasyonu/sanitizasyonu (örn. TC'de sadece rakam) aynen uygulanır.
 */
export function createFillFieldsHandler(): AiActionHandler {
  return (action: AiAction) => {
    const values = action.data.values

    if (!values || typeof values !== 'object') {
      throw new Error('fill_fields: values yok')
    }

    for (const [reference, value] of Object.entries(values as Record<string, unknown>)) {
      waitForElement(reference).then((element) => {
        if (!element) {
          console.warn(`fill_fields: alan bulunamadı: ${reference}`)
        } else if (!writeValue(element, value)) {
          console.warn(`fill_fields: alana yazılamadı: ${reference}`)
        }
      })
    }
  }
}

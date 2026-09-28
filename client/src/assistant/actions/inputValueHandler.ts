import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler } from './types'
import { waitForElement } from './waitForElement'

/**
 * Form dışı bir giriş alanına (arama kutusu, filtre) değer yazar.
 * React kontrollü input'larında `.value` ataması state'i güncellemez; native
 * setter ile değer atanıp `input` event'i tetiklenir, böylece sayfanın kendi
 * onChange'i çalışır.
 */
export function createInputValueHandler(): AiActionHandler {
  return (action: AiAction) => {
    const { elementId, value } = action.data

    if (typeof elementId !== 'string' || !elementId) {
      throw new Error(`Invalid input elementId '${elementId}'`)
    }

    waitForElement(elementId).then((element) => {
      if (
        !(element instanceof HTMLInputElement) &&
        !(element instanceof HTMLTextAreaElement)
      ) {
        console.warn(`Input element not found or not writable: #${elementId}`)
        return
      }

      const prototype = Object.getPrototypeOf(element)
      const setValue = Object.getOwnPropertyDescriptor(prototype, 'value')?.set

      setValue?.call(element, typeof value === 'string' ? value : String(value ?? ''))
      element.dispatchEvent(new Event('input', { bubbles: true }))
      element.scrollIntoView({ behavior: 'smooth', block: 'center' })
      element.focus({ preventScroll: true })
    })
  }
}

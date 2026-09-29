import { aiFieldRegistry, type AiWriteResult } from '../../ui/ai/useAiField'

/**
 * Bir alana, sayfanın kendi değişiklik mantığı çalışacak şekilde değer yazar.
 *
 * 1. Alan ortak form bileşenlerinden biriyse (useAiField ile kayıtlı; MUI select, tarih
 *    seçici...), bileşenin kendi yazıcısı kullanılır: değer react-hook-form state'ine gider,
 *    şema/transform/hata mesajları normal kullanıcı girişindeki gibi çalışır.
 * 2. Değilse düz HTML alanıdır: native setter ile değer atanıp `input` / `change`
 *    event'leri tetiklenir (React kontrollü input'ları dahil).
 */
export type FieldWriter = (element: HTMLElement, value: unknown) => boolean

const customWriters: FieldWriter[] = []

/**
 * Kayıt defterine bağlanmamış özel bileşenler için yazıcı ekler (kaçış kapısı).
 * Yazıcı elemanı tanıyıp yazdıysa true döner; tanımıyorsa false (sıradakine geçilir).
 */
export function registerFieldWriter(writer: FieldWriter): void {
  customWriters.push(writer)
}

export function writeValue(element: HTMLElement, value: unknown): AiWriteResult {
  const registered = element.dataset.aiField ? aiFieldRegistry.get(element.dataset.aiField) : undefined
  if (registered) {
    return registered.write(value)
  }

  for (const writer of customWriters) {
    if (writer(element, value)) return { ok: true }
  }

  if (element instanceof HTMLInputElement && (element.type === 'checkbox' || element.type === 'radio')) {
    const checked = value === true || value === 'true' || value === element.value
    if (element.checked !== checked) element.click()
    return { ok: true }
  }

  if (
    element instanceof HTMLInputElement ||
    element instanceof HTMLTextAreaElement ||
    element instanceof HTMLSelectElement
  ) {
    const setValue = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(element), 'value')?.set
    setValue?.call(element, value == null ? '' : String(value))
    element.dispatchEvent(new Event('input', { bubbles: true }))
    element.dispatchEvent(new Event('change', { bubbles: true }))
    return { ok: true }
  }

  return { ok: false, reason: 'Bu eleman bir giriş alanı değil veya asistana tanıtılmamış.' }
}

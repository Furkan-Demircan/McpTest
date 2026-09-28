/**
 * Bir alana, sayfanın kendi değişiklik mantığı (onChange) çalışacak şekilde değer yazar.
 * React kontrollü input'larında `.value` ataması state'i güncellemez; native setter ile
 * değer atanıp `input` / `change` event'leri tetiklenir. Form kütüphanesinden bağımsızdır.
 */
export type FieldWriter = (element: HTMLElement, value: unknown) => boolean

const customWriters: FieldWriter[] = []

/**
 * Özel bileşenler (tarih seçici, custom select, maskeli input) için yazıcı ekler.
 * Yazıcı elemanı tanıyıp yazdıysa true döner; tanımıyorsa false (sıradakine geçilir).
 */
export function registerFieldWriter(writer: FieldWriter): void {
  customWriters.push(writer)
}

export function writeValue(element: HTMLElement, value: unknown): boolean {
  for (const writer of customWriters) {
    if (writer(element, value)) return true
  }

  if (element instanceof HTMLInputElement && (element.type === 'checkbox' || element.type === 'radio')) {
    const checked = value === true || value === 'true' || value === element.value
    if (element.checked !== checked) element.click()
    return true
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
    return true
  }

  return false
}

import { aiReference } from './dom/findAiElement'

export interface ScreenElement {
  id: string
  kind: 'input' | 'select' | 'textarea' | 'button' | 'link'
  label: string
  field?: string
  name?: string
  value?: string
  // Sayfanın HTML'de ilan ettiği aralık (örn. tarih seçicinin min/max'ı)
  min?: string
  max?: string
  required?: boolean
  disabled?: boolean
}

export interface ScreenSnapshot {
  heading?: string
  elements: ScreenElement[]
}

const INTERACTIVE_SELECTOR = [
  'input[id]', 'input[name]', 'select[id]', 'select[name]', 'textarea[id]', 'textarea[name]',
  'button[id]', 'a[id]', '[data-ai-field]',
].join(', ')
const ASSISTANT_ROOT_SELECTOR = '.assistant-container'
const MAX_ELEMENTS = 80
const MAX_LABEL_LENGTH = 80
const MAX_VALUE_LENGTH = 100

/**
 * Kullanıcının o an gördüğü etkileşimli elemanların özeti (DOM'dan). Asistan bulunulan
 * sayfayı buradan okur; uygulamanın state'ine erişmez. Alanların değerleri de gider
 * (kısaltılarak); şifre alanlarının değeri asla gönderilmez.
 */
export function captureScreenSnapshot(root: ParentNode = document): ScreenSnapshot {
  const elements: ScreenElement[] = []
  const seen = new Set<HTMLElement>()

  for (const element of root.querySelectorAll<HTMLElement>(INTERACTIVE_SELECTOR)) {
    if (elements.length >= MAX_ELEMENTS) break
    if (seen.has(element) || element.closest(ASSISTANT_ROOT_SELECTOR) || !isVisible(element)) continue
    seen.add(element)

    const snapshot: ScreenElement = {
      id: element.id,
      kind: getKind(element),
      label: getLabel(element),
    }

    if (element.dataset.aiField) snapshot.field = element.dataset.aiField
    if (element.getAttribute('name')) snapshot.name = element.getAttribute('name')!

    if (
      element instanceof HTMLInputElement ||
      element instanceof HTMLSelectElement ||
      element instanceof HTMLTextAreaElement
    ) {
      const value = readValue(element)
      if (value) snapshot.value = value
      if (isRequired(element)) snapshot.required = true
      if (element.getAttribute('min')) snapshot.min = element.getAttribute('min')!
      if (element.getAttribute('max')) snapshot.max = element.getAttribute('max')!
    }

    if ((element as HTMLButtonElement).disabled) snapshot.disabled = true

    // Referans yoksa asistan bu elemana ulaşamaz
    if (aiReference(element)) elements.push(snapshot)
  }

  const heading = document.querySelector('h1')?.textContent?.trim()

  return { heading: heading || undefined, elements }
}

function readValue(element: HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement): string | undefined {
  if (element instanceof HTMLInputElement) {
    if (element.type === 'password') return undefined
    if (element.type === 'checkbox' || element.type === 'radio') return element.checked ? 'seçili' : undefined
  }

  const value = element.value.trim()
  return value.length > MAX_VALUE_LENGTH ? `${value.slice(0, MAX_VALUE_LENGTH)}…` : value || undefined
}

function getKind(element: HTMLElement): ScreenElement['kind'] {
  switch (element.tagName) {
    case 'SELECT':
      return 'select'
    case 'TEXTAREA':
      return 'textarea'
    case 'BUTTON':
      return 'button'
    case 'A':
      return 'link'
    default:
      return 'input'
  }
}

function findLabel(element: HTMLElement): HTMLLabelElement | null {
  return (
    (element.id ? document.querySelector<HTMLLabelElement>(`label[for="${CSS.escape(element.id)}"]`) : null) ??
    element.closest('label')
  )
}

function getLabel(element: HTMLElement): string {
  const text =
    findLabel(element)?.textContent ||
    element.getAttribute('aria-label') ||
    element.getAttribute('placeholder') ||
    element.textContent ||
    ''

  // Zorunluluk yıldızını etiketten ayıkla; ayrıca `required` olarak gidiyor.
  return text.replace(/\*/g, '').replace(/\s+/g, ' ').trim().slice(0, MAX_LABEL_LENGTH)
}

function isRequired(element: HTMLElement): boolean {
  if ((element as HTMLInputElement).required) return true
  return Boolean(findLabel(element)?.querySelector('.required-star'))
}

function isVisible(element: HTMLElement): boolean {
  return element.getClientRects().length > 0
}

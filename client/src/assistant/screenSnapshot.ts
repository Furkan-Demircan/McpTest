export interface ScreenElement {
  id: string
  kind: 'input' | 'select' | 'textarea' | 'button' | 'link'
  label: string
  required?: boolean
  filled?: boolean
  disabled?: boolean
}

export interface ScreenSnapshot {
  heading?: string
  elements: ScreenElement[]
}

const INTERACTIVE_SELECTOR = 'input[id], select[id], textarea[id], button[id], a[id]'
const ASSISTANT_ROOT_SELECTOR = '.assistant-container'
const MAX_ELEMENTS = 80
const MAX_LABEL_LENGTH = 80

/**
 * Kullanıcının o an gördüğü etkileşimli elemanların özeti. Model bunu
 * "ekranda ne var" sorusunun cevabı olarak kullanır; highlight_element için
 * geçerli kimlikler buradan gelir. Değer değil, sadece dolu/boş bilgisi gönderilir.
 */
export function captureScreenSnapshot(root: ParentNode = document): ScreenSnapshot {
  const elements: ScreenElement[] = []

  for (const element of root.querySelectorAll<HTMLElement>(INTERACTIVE_SELECTOR)) {
    if (elements.length >= MAX_ELEMENTS) break
    if (element.closest(ASSISTANT_ROOT_SELECTOR) || !isVisible(element)) continue

    const kind = getKind(element)
    const snapshot: ScreenElement = {
      id: element.id,
      kind,
      label: getLabel(element),
    }

    if (
      element instanceof HTMLInputElement ||
      element instanceof HTMLSelectElement ||
      element instanceof HTMLTextAreaElement
    ) {
      snapshot.filled = element.value.trim() !== ''
      if (isRequired(element)) snapshot.required = true
    }

    if ((element as HTMLButtonElement).disabled) snapshot.disabled = true

    elements.push(snapshot)
  }

  const heading = document.querySelector('h1')?.textContent?.trim()

  return { heading: heading || undefined, elements }
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

function getLabel(element: HTMLElement): string {
  const labelFor = document.querySelector(`label[for="${CSS.escape(element.id)}"]`)
  const text =
    labelFor?.textContent ||
    element.getAttribute('aria-label') ||
    element.getAttribute('placeholder') ||
    element.textContent ||
    ''

  // Zorunluluk yıldızını etiketten ayıkla; ayrıca `required` olarak gidiyor.
  return text.replace(/\*/g, '').replace(/\s+/g, ' ').trim().slice(0, MAX_LABEL_LENGTH)
}

function isRequired(element: HTMLElement): boolean {
  if ((element as HTMLInputElement).required) return true
  const labelFor = document.querySelector(`label[for="${CSS.escape(element.id)}"]`)
  return Boolean(labelFor?.querySelector('.required-star'))
}

function isVisible(element: HTMLElement): boolean {
  return element.getClientRects().length > 0
}

import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler } from './types'
import { waitForElement } from './waitForElement'

const HIGHLIGHT_CLASS = 'assistant-highlight'
const HINT_CLASS = 'assistant-highlight-hint'
const HIGHLIGHT_DURATION_MS = 8000

let clearActiveHighlight: (() => void) | null = null

/**
 * Ekrandaki bir elemanı kullanıcıya işaretler (navigasyon sonrası elemanı bekler).
 */
export function createHighlightHandler(): AiActionHandler {
  return (action: AiAction) => {
    const elementId = action.data.elementId

    if (typeof elementId !== 'string' || !elementId) {
      throw new Error(`Invalid highlight elementId '${elementId}'`)
    }

    const message =
      typeof action.data.message === 'string' ? action.data.message : undefined

    waitForElement(elementId).then((element) => {
      if (!element) {
        console.warn(`Highlight element not found: #${elementId}`)
        return
      }

      showHighlight(element, message)
    })
  }
}

function showHighlight(element: HTMLElement, message?: string) {
  clearActiveHighlight?.()

  element.scrollIntoView({ behavior: 'smooth', block: 'center' })
  element.classList.add(HIGHLIGHT_CLASS)

  let hint: HTMLDivElement | null = null

  const positionHint = () => {
    if (!hint) return
    const rect = element.getBoundingClientRect()
    hint.style.top = `${rect.top - hint.offsetHeight - 10}px`
    hint.style.left = `${Math.max(12, rect.left)}px`
  }

  if (message) {
    hint = document.createElement('div')
    hint.className = HINT_CLASS
    hint.textContent = message
    document.body.appendChild(hint)
    positionHint()
  }

  // Kaydırma animasyonu boyunca ve sonrasında notu elemanın üstünde tut.
  window.addEventListener('scroll', positionHint, true)
  window.addEventListener('resize', positionHint)

  const clear = () => {
    window.clearTimeout(timer)
    element.classList.remove(HIGHLIGHT_CLASS)
    hint?.remove()
    window.removeEventListener('scroll', positionHint, true)
    window.removeEventListener('resize', positionHint)
    element.removeEventListener('focus', clear)
    element.removeEventListener('click', clear)
    if (clearActiveHighlight === clear) clearActiveHighlight = null
  }

  // Kullanıcı elemana dokunduğunda işaret işini yapmıştır.
  element.addEventListener('focus', clear)
  element.addEventListener('click', clear)
  const timer = window.setTimeout(clear, HIGHLIGHT_DURATION_MS)

  clearActiveHighlight = clear
}

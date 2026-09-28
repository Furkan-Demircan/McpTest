import { findAiElement } from '../dom/findAiElement'

const DEFAULT_TIMEOUT_MS = 3000

/**
 * Aynı cevapta önce navigation gelmiş olabilir; yeni sayfa render olana
 * kadar eleman DOM'da yoktur, bu yüzden kısa bir süre beklenir.
 */
export function waitForElement(
  reference: string,
  timeoutMs = DEFAULT_TIMEOUT_MS
): Promise<HTMLElement | null> {
  return new Promise((resolve) => {
    const startedAt = performance.now()

    const check = () => {
      const element = findAiElement(reference)

      if (element) {
        resolve(element)
      } else if (performance.now() - startedAt > timeoutMs) {
        resolve(null)
      } else {
        requestAnimationFrame(check)
      }
    }

    check()
  })
}

/**
 * navigate() adresi hemen değiştirir ama yeni sayfa sonraki karelerde render olur.
 * Aynı alan adı iki sayfada da olabildiği için (firstName), sonraki aksiyonlar eski
 * sayfaya yazmasın diye adres değişip React commit edene kadar beklenir.
 */
export function waitForNavigation(path: string, timeoutMs = DEFAULT_TIMEOUT_MS): Promise<void> {
  return new Promise((resolve) => {
    const startedAt = performance.now()

    const check = () => {
      if (window.location.pathname === path || performance.now() - startedAt > timeoutMs) {
        // İki kare: React'in yeni sayfayı commit etmesi için
        requestAnimationFrame(() => requestAnimationFrame(() => resolve()))
      } else {
        requestAnimationFrame(check)
      }
    }

    check()
  })
}

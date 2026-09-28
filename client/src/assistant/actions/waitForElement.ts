const DEFAULT_TIMEOUT_MS = 3000

/**
 * Aynı cevapta önce navigation gelmiş olabilir; yeni sayfa render olana
 * kadar eleman DOM'da yoktur, bu yüzden kısa bir süre beklenir.
 */
export function waitForElement(
  elementId: string,
  timeoutMs = DEFAULT_TIMEOUT_MS
): Promise<HTMLElement | null> {
  return new Promise((resolve) => {
    const startedAt = performance.now()

    const check = () => {
      const element = document.getElementById(elementId)

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

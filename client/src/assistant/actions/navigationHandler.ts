import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler, ActionOutcome } from './types'
import { waitForNavigation } from './waitForElement'

/**
 * Sayfaya gider ve yeni sayfa render olana kadar bekler; sonraki aksiyonlar ve
 * sunucuya giden ekran özeti yeni sayfaya ait olur.
 */
export function createNavigationHandler(
  navigate: (path: string) => void
): AiActionHandler {
  return async (action: AiAction): Promise<ActionOutcome> => {
    const path = action.data.path

    if (typeof path !== 'string' || !path) {
      return { status: 'failed', error: `Geçersiz path: '${String(path)}'` }
    }

    navigate(path)
    await waitForNavigation(path)

    const current = window.location.pathname
    return current === path
      ? { status: 'applied', detail: { path: current } }
      : { status: 'failed', error: `Sayfa değişmedi (şu an: ${current})`, detail: { path: current } }
  }
}

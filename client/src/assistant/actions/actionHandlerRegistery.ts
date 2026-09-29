import type { AiAction } from '../../services/assistantApi'
import type { ActionOutcome, AiActionHandler } from './types'

interface ActionHandlerRegistryOptions {
  fillFieldsHandler: AiActionHandler
  navigationHandler: AiActionHandler
  highlightHandler: AiActionHandler
}

export interface ActionHandleResult extends ActionOutcome {
  action: AiAction
}

export function createActionHandlerRegistry({
  fillFieldsHandler,
  navigationHandler,
  highlightHandler,
}: ActionHandlerRegistryOptions) {
  const handlers: Record<string, AiActionHandler> = {
    fill_fields: fillFieldsHandler,
    navigation: navigationHandler,
    highlight: highlightHandler,
  }

  return {
    // Hiçbir zaman throw etmez: bilinmeyen/hatalı aksiyon da bir sonuç olarak modele döner.
    async handle(action: AiAction): Promise<ActionHandleResult> {
      const handler = handlers[action.type]

      if (!handler) {
        console.warn(`No handler found for action type: ${action.type}`)
        return { action, status: 'failed', error: `İstemcide '${action.type}' aksiyonu desteklenmiyor.` }
      }

      try {
        return { action, ...(await handler(action)) }
      } catch (error) {
        console.error('Action handler error:', error)
        return {
          action,
          status: 'failed',
          error: error instanceof Error ? error.message : String(error),
        }
      }
    },
  }
}

import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler } from './types'

interface ActionHandlerRegistryOptions {
  fillFieldsHandler: AiActionHandler
  navigationHandler: AiActionHandler
  highlightHandler: AiActionHandler
}

export interface ActionHandleResult {
  action: AiAction
  status: 'applied' | 'unhandled' | 'failed'
  error?: string
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
    // Hiçbir zaman throw etmez: bilinmeyen/hatalı aksiyon botun cevabını düşürmemeli.
    handle(action: AiAction): ActionHandleResult {
      const handler = handlers[action.type]

      if (!handler) {
        console.warn(`No handler found for action type: ${action.type}`)
        return { action, status: 'unhandled' }
      }

      try {
        handler(action)
        return { action, status: 'applied' }
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

import type { AiAction } from '../../services/assistantApi'
import type { AiActionHandler } from './types'

interface ActionHandlerRegistryOptions {
  formPatchHandler: AiActionHandler
  navigationHandler: AiActionHandler
}

export function createActionHandlerRegistry({
  formPatchHandler,
  navigationHandler,
}: ActionHandlerRegistryOptions) {
  const handlers: Record<string, AiActionHandler> = {
    form_patch: formPatchHandler,
    navigation: navigationHandler,
  }

  return {
    handle(action: AiAction) {
      const handler = handlers[action.type]

      if (!handler) {
        throw new Error(
          `No handler found for action type: ${action.type}`
        )
      }

      handler(action)
    },
  }
}
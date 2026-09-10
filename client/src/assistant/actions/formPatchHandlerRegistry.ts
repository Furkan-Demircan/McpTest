import type { AiAction } from '../../services/assistantApi'

import type { AiActionHandler } from './types'

export function createFormPatchHandlerRegistry(
  studentFormHandler: AiActionHandler
) {
  const handlers: Record<string, AiActionHandler> = {
    studentForm: studentFormHandler,
  }

  return {
    handle(action: AiAction) {
      if (!action.target) {
        throw new Error(
          'form_patch action için target belirtilmedi.'
        )
      }

      const handler = handlers[action.target]

      if (!handler) {
        throw new Error(
          `No form handler found for target: ${action.target}`
        )
      }

      handler(action)
    },
  }
}
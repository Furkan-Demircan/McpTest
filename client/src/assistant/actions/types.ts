import type { AiAction } from '../../services/assistantApi'

/**
 * Aksiyonun kullanıcının ekranındaki gerçek sonucu. Sunucuya geri bildirilir ve
 * modele ilgili tool çağrısının sonucu olarak yazılır.
 */
export interface ActionOutcome {
  status: 'applied' | 'partial' | 'failed'
  detail?: Record<string, unknown>
  error?: string
}

export type AiActionHandler = (action: AiAction) => Promise<ActionOutcome>

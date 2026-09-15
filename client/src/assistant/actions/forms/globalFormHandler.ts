import type { AiAction } from '../../../services/assistantApi'
import type { AiActionHandler } from '../types'

export interface GlobalFormHandlerOptions {
  patchFormData: (formIdOrPath: string, patch: Record<string, unknown>) => void
  getActivePath: () => string
  getFormIdByPath: (pathname: string) => string | undefined
}

/**
 * Sayfada kaç form olursa olsun (öğrenci, öğretmen, ders, sınav vb.) gelen
 * tüm `form_patch` aksiyonlarını hedef forma veya aktif sayfadaki forma aktaran evrensel handler.
 */
export function createGlobalFormHandler({
  patchFormData,
  getActivePath,
  getFormIdByPath,
}: GlobalFormHandlerOptions): AiActionHandler {
  return (action: AiAction) => {
    console.log('[GlobalFormHandler] Form Patch Action:', action)
    const data: Record<string, unknown> = action.data

    if (!data || typeof data !== 'object') {
      console.warn('[GlobalFormHandler] Geçersiz form verisi:', data)
      return
    }

    // Hedef form belirleme:
    // 1. Aksiyonda açıkça belirtilen target varsa (örn: "studentForm", "teacherForm", "courseForm")
    // 2. Yoksa mevcut aktif rotanın form ID'si
    // 3. O da bulunamazsa doğrudan aktif rota path'i
    const currentPath = getActivePath()
    const target = action.target || getFormIdByPath(currentPath) || currentPath

    console.log(`[GlobalFormHandler] Hedef '${target}' formuna veri aktarılıyor:`, data)
    patchFormData(target, data)
  }
}

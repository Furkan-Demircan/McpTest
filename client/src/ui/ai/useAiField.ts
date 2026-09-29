import { useEffect, useRef } from 'react'

/**
 * Form bileşenleri ile asistan arasındaki köprü (CRM'de packages/ui içinde yaşar).
 *
 * MUI select / tarih seçici gibi bileşenler gerçek bir <input> gibi davranmaz; DOM'a
 * değer yazmak react-hook-form state'ine ulaşmaz. Bu yüzden her ortak form bileşeni
 * kendini burada kaydeder:
 * - write: değeri alanın beklediği tipe çevirip bileşenin kendi field.onChange'ine verir
 * - label / getValue / options / min / max: ekran özetine DOM tahmini yerine bileşenin
 *   kendisinden gelen doğru bilgi
 * Asistan alanı data-ai-field ile bulur, yazıcıyı buradan alır.
 */

export type AiWriteResult = { ok: true } | { ok: false; reason: string }

export interface AiFieldOption {
  value: string | number
  label: string
}

export interface AiFieldRegistration {
  label: string
  kind: 'input' | 'select'
  required?: boolean
  getValue: () => string | undefined
  options?: () => AiFieldOption[]
  min?: string
  max?: string
  write: (value: unknown) => AiWriteResult
}

const registrations = new Map<string, AiFieldRegistration>()

export const aiFieldRegistry = {
  get: (name: string): AiFieldRegistration | undefined => registrations.get(name),
}

/**
 * Alanı asistana tanıtır; bileşen ekrandayken kayıtlıdır, kalkınca silinir.
 * Dönen prop'lar alanın kök DOM elemanına verilir (bulma, işaretleme, ekran özeti).
 */
export function useAiField(name: string, registration: AiFieldRegistration) {
  const latest = useRef(registration)

  // Her render'da güncel field.onChange / değer / seçenekleri kullan
  useEffect(() => {
    latest.current = registration
  })

  useEffect(() => {
    registrations.set(name, {
      get label() { return latest.current.label },
      get kind() { return latest.current.kind },
      get required() { return latest.current.required },
      get min() { return latest.current.min },
      get max() { return latest.current.max },
      getValue: () => latest.current.getValue(),
      options: () => latest.current.options?.() ?? [],
      write: (value) => latest.current.write(value),
    })
    return () => {
      registrations.delete(name)
    }
  }, [name])

  return { 'data-ai-field': name }
}

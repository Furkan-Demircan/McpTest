const ASSISTANT_ROOT_SELECTOR = '.assistant-container'

/**
 * Asistanın referans verdiği elemanı bulur. Sıra: data-ai-field → name → id.
 * Input'un name'i backend (Swagger) alan adıyla aynıysa hiçbir işaret gerekmez;
 * değilse input'a data-ai-field="alanAdi" eklenir. Asistanın kendi paneli hariç tutulur.
 */
export function findAiElement(reference: string, root: ParentNode = document): HTMLElement | null {
  const escaped = CSS.escape(reference)
  const selectors = [`[data-ai-field="${escaped}"]`, `[name="${escaped}"]`, `#${escaped}`]

  for (const selector of selectors) {
    for (const element of root.querySelectorAll<HTMLElement>(selector)) {
      if (!element.closest(ASSISTANT_ROOT_SELECTOR)) return element
    }
  }

  return null
}

/** Ekran özeti ve sunucu doğrulamasıyla aynı referans: data-ai-field → name → id */
export function aiReference(element: HTMLElement): string {
  return element.dataset.aiField || element.getAttribute('name') || element.id
}

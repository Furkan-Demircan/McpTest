import React from 'react'
import type { AiTraceStep } from '../services/assistantApi'
import type { ActionHandleResult } from '../assistant/actions/actionHandlerRegistery'

interface AssistantTraceProps {
  trace: AiTraceStep[]
  actionResults: ActionHandleResult[]
}

const kindLabel: Record<AiTraceStep['kind'], string> = {
  llm: 'LLM',
  tool: 'TOOL',
  client: 'İSTEMCİ',
  limit: 'LİMİT',
}

const actionStatusLabel: Record<ActionHandleResult['status'], string> = {
  applied: 'uygulandı',
  partial: 'kısmen',
  failed: 'hata',
}

function prettyJson(value?: string): string | undefined {
  if (!value) return value
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

/**
 * Keşif amaçlı: bir bot cevabının arkasındaki LLM/tool turlarını ve
 * istemcide uygulanan UI aksiyonlarını gösterir.
 */
export const AssistantTrace: React.FC<AssistantTraceProps> = ({
  trace,
  actionResults,
}) => {
  const toolCount = trace.filter((step) => step.kind === 'tool').length
  const turnCount = trace.filter((step) => step.kind === 'llm').length
  const totalMs = trace.reduce((sum, step) => sum + step.durationMs, 0)
  const hasError =
    trace.some((step) => step.isError) ||
    actionResults.some((result) => result.status !== 'applied')

  return (
    <details className={`assistant-trace ${hasError ? 'has-error' : ''}`}>
      <summary>
        🔍 {turnCount} tur · {toolCount} tool · {actionResults.length} aksiyon · {totalMs} ms
      </summary>

      <ol className="trace-steps">
        {trace.map((step, index) => (
          <li
            key={index}
            className={`trace-step trace-${step.kind} ${step.isError ? 'is-error' : ''}`}
          >
            <div className="trace-step-head">
              <span className="trace-kind">{kindLabel[step.kind]}</span>
              <span className="trace-name">
                #{step.iteration} {step.name}
              </span>
              <span className="trace-ms">{step.durationMs} ms</span>
            </div>

            {step.arguments && (
              <pre className="trace-json">
                <span className="trace-label">args </span>
                {prettyJson(step.arguments)}
              </pre>
            )}

            {step.result && (
              <pre className="trace-json">
                <span className="trace-label">
                  {step.kind === 'llm' ? 'text ' : 'result '}
                </span>
                {prettyJson(step.result)}
              </pre>
            )}
          </li>
        ))}

        {actionResults.map((result, index) => (
          <li
            key={`action-${index}`}
            className={`trace-step trace-action ${result.status !== 'applied' ? 'is-error' : ''}`}
          >
            <div className="trace-step-head">
              <span className="trace-kind">UI</span>
              <span className="trace-name">
                {result.action.type}
                {result.action.target ? ` → ${result.action.target}` : ''}
              </span>
              <span className="trace-ms">{actionStatusLabel[result.status]}</span>
            </div>
            {result.error && <pre className="trace-json">{result.error}</pre>}
            {result.detail && result.status !== 'applied' && (
              <pre className="trace-json">{JSON.stringify(result.detail, null, 2)}</pre>
            )}
          </li>
        ))}
      </ol>
    </details>
  )
}

import React, { useEffect, useRef, useState } from 'react'
import './AssistantWidget.css'
import {
  sendAssistantMessage,
  type ChatMessage,
} from '../services/assistantApi'
import {useNavigate,useLocation} from 'react-router-dom'


import { useFormContext } from '../contexts/useFormContext'
import { createStudentFormHandler } from '../assistant/actions/forms/studentFormHandler'
import { createTeacherFormHandler } from '../assistant/actions/forms/teacherFormHandler'
import { createFormPatchHandlerRegistry } from '../assistant/actions/formPatchHandlerRegistry'
import { createActionHandlerRegistry } from '../assistant/actions/actionHandlerRegistery'
import { createNavigationHandler } from '../assistant/actions/navigationHandler'

interface Message {
  id: string
  sender: 'bot' | 'user'
  text: string
  time: string
}

interface SpeechRecognitionResultItem {
  readonly transcript: string
  readonly confidence: number
}

interface SpeechRecognitionResult {
  readonly isFinal: boolean
  readonly length: number
  [index: number]: SpeechRecognitionResultItem
}

interface SpeechRecognitionResultList {
  readonly length: number
  [index: number]: SpeechRecognitionResult
}

interface SpeechRecognitionEvent extends Event {
  readonly resultIndex: number
  readonly results: SpeechRecognitionResultList
}

interface SpeechRecognitionErrorEvent extends Event {
  readonly error: string
  readonly message?: string
}

interface SpeechRecognitionInstance extends EventTarget {
  continuous: boolean
  interimResults: boolean
  lang: string
  start: () => void
  stop: () => void
  abort: () => void
  onstart: ((event: Event) => void) | null
  onend: ((event: Event) => void) | null
  onerror: ((event: SpeechRecognitionErrorEvent) => void) | null
  onresult: ((event: SpeechRecognitionEvent) => void) | null
}

type SpeechRecognitionConstructor = new () => SpeechRecognitionInstance

const getSpeechRecognition = (): SpeechRecognitionConstructor | null => {
  if (typeof window === 'undefined') return null
  const win = window as unknown as {
    SpeechRecognition?: SpeechRecognitionConstructor
    webkitSpeechRecognition?: SpeechRecognitionConstructor
  }
  return win.SpeechRecognition || win.webkitSpeechRecognition || null
}

const combineTexts = (
  base: string,
  finalTranscript: string,
  interim: string
): string => {
  const parts = [base.trim(), finalTranscript.trim(), interim.trim()].filter(Boolean)
  return parts.join(' ')
}

let messageCounter = 1

function getNextId(): string {
  messageCounter += 1
  return `msg-${messageCounter}`
}

function getCurrentTimeString(): string {
  const now = new Date()

  return `${now
    .getHours()
    .toString()
    .padStart(2, '0')}:${now
    .getMinutes()
    .toString()
    .padStart(2, '0')}`
}

export const AssistantWidget: React.FC = () => {
  const {
    studentFormData,
    setStudentFormData,
    teacherFormData,
    setTeacherFormData,
  } = useFormContext()
  const [isOpen, setIsOpen] = useState(false)
  const [inputMessage, setInputMessage] = useState('')
  const [isTyping, setIsTyping] = useState(false)
  const [isListening, setIsListening] = useState(false)
  const isListeningRef = useRef(false)
  const location = useLocation()
  const navigate = useNavigate()

  const recognitionRef = useRef<SpeechRecognitionInstance | null>(null)
  const baseTextRef = useRef<string>('')
  const finalTranscriptRef = useRef<string>('')
  const restartTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const isSpeechSupported = typeof window !== 'undefined' && Boolean(getSpeechRecognition())

  const navigationHandler = createNavigationHandler(navigate)
  
  const studentFormPatchHandler = createStudentFormHandler(setStudentFormData)
  const teacherFormPatchHandler = createTeacherFormHandler(setTeacherFormData)

  const formPatchHandlerRegistry =
    createFormPatchHandlerRegistry(studentFormPatchHandler, teacherFormPatchHandler)

  const actionHandlerRegistry =
    createActionHandlerRegistry({
      formPatchHandler: formPatchHandlerRegistry.handle,
      navigationHandler,
    })

  const messagesEndRef = useRef<HTMLDivElement>(null)


  const [messages, setMessages] = useState<Message[]>([
    {
      id: '1',
      sender: 'bot',
      text: 'Merhaba! Size nasıl yardımcı olabilirim?',
      time: 'Şimdi',
    },
  ])

  const quickQuestions = [
    'Öğrenci nasıl eklenir?',
    'Öğretmen nasıl eklenir?',
    'Hangi bilgiler gerekli?',
    'Kayıtları nasıl görürüm?',
  ]

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({
      behavior: 'smooth',
    })
  }

  useEffect(() => {
    if (isOpen) {
      scrollToBottom()
    }
  }, [messages, isOpen, isTyping])

  const stopListening = () => {
    isListeningRef.current = false
    if (restartTimerRef.current) {
      clearTimeout(restartTimerRef.current)
      restartTimerRef.current = null
    }
    if (recognitionRef.current) {
      try {
        recognitionRef.current.stop()
      } catch {
        // ignore
      }
      recognitionRef.current = null
    }
    setIsListening(false)
  }

  const startRecognitionSession = () => {
    const SpeechRecognitionClass = getSpeechRecognition()
    if (!SpeechRecognitionClass || !isListeningRef.current) {
      return
    }

    const recognition = new SpeechRecognitionClass()
    recognition.lang = 'tr-TR'
    recognition.continuous = true
    recognition.interimResults = true

    recognition.onresult = (event: SpeechRecognitionEvent) => {
      let interim = ''

      for (let i = event.resultIndex; i < event.results.length; i++) {
        const result = event.results[i]
        const transcript = result[0].transcript
        if (result.isFinal) {
          const finalChunk = transcript.trim()
          if (finalChunk) {
            finalTranscriptRef.current = finalTranscriptRef.current
              ? `${finalTranscriptRef.current} ${finalChunk}`
              : finalChunk
          }
        } else {
          interim += transcript
        }
      }

      const combined = combineTexts(
        baseTextRef.current,
        finalTranscriptRef.current,
        interim
      )
      setInputMessage(combined)
    }

    recognition.onerror = (event: SpeechRecognitionErrorEvent) => {
      console.warn('Speech recognition error:', event.error)
      if (
        event.error === 'not-allowed' ||
        event.error === 'service-not-allowed' ||
        event.error === 'audio-capture'
      ) {
        stopListening()
      }
    }

    recognition.onend = () => {
      // Duraksama veya sessizlik zaman aşımında kullanıcı kapatmadıysa konuşma modunu açık tut
      if (isListeningRef.current) {
        restartTimerRef.current = setTimeout(() => {
          if (isListeningRef.current) {
            startRecognitionSession()
          }
        }, 200)
      } else {
        setIsListening(false)
      }
    }

    try {
      recognition.start()
      recognitionRef.current = recognition
    } catch (err) {
      console.error('Speech recognition start failed:', err)
      stopListening()
    }
  }

  const startListening = () => {
    const SpeechRecognitionClass = getSpeechRecognition()
    if (!SpeechRecognitionClass) {
      alert('Tarayıcınız sesle yazmayı (Web Speech API) desteklemiyor.')
      return
    }

    if (recognitionRef.current) {
      try {
        recognitionRef.current.abort()
      } catch {
        // ignore
      }
      recognitionRef.current = null
    }

    baseTextRef.current = inputMessage
    finalTranscriptRef.current = ''
    isListeningRef.current = true
    setIsListening(true)

    startRecognitionSession()
  }

  const toggleListening = () => {
    if (isListeningRef.current) {
      stopListening()
    } else {
      startListening()
    }
  }

  useEffect(() => {
    return () => {
      isListeningRef.current = false
      if (restartTimerRef.current) {
        clearTimeout(restartTimerRef.current)
      }
      if (recognitionRef.current) {
        try {
          recognitionRef.current.abort()
        } catch {
          // ignore
        }
      }
    }
  }, [])

  const handleSendMessage = async (messageText: string) => {
  if (isListeningRef.current) {
    stopListening()
  }
  const trimmedMessage = messageText.trim()

  if (!trimmedMessage || isTyping) {
    return
  }

  const userMessage: Message = {
    id: getNextId(),
    sender: 'user',
    text: trimmedMessage,
    time: getCurrentTimeString(),
  }

  // Yeni kullanıcı mesajını mevcut history'ye ekle
  const nextMessages = [
    ...messages,
    userMessage,
  ]

  setMessages(nextMessages)
  setInputMessage('')
  setIsTyping(true)

  try {
    // UI mesajlarını DeepSeek formatına dönüştür
    const chatMessages: ChatMessage[] =
      nextMessages.map((message) => ({
        role:
          message.sender === 'bot'
            ? 'assistant'
            : 'user',
        content: message.text,
      }))

    const isTeacherRoute =
      location.pathname.startsWith('/teacher') || location.pathname.startsWith('/ogretmen')
    const activeFormData = isTeacherRoute ? teacherFormData : studentFormData

    // Tüm conversation history'yi gönder
    const response =
      await sendAssistantMessage(chatMessages, activeFormData, location.pathname)
    
    response.actions?.forEach((action) => {
  console.log('AI Action:', action)
  actionHandlerRegistry.handle(action)
})

    const botMessage: Message = {
      id: getNextId(),
      sender: 'bot',
      text: response.message,
      time: getCurrentTimeString(),
    }

    setMessages((prev) => [
      ...prev,
      botMessage,
    ])
  } catch (error) {
    console.error('Assistant error:', error)

    const errorText =
      error instanceof Error
        ? error.message
        : 'Asistan ile iletişim kurulurken bir hata oluştu. Lütfen tekrar deneyin.'

    const errorMessage: Message = {
      id: getNextId(),
      sender: 'bot',
      text: errorText,
      time: getCurrentTimeString(),
    }

    setMessages((prev) => [
      ...prev,
      errorMessage,
    ])
  } finally {
    setIsTyping(false)
  }
}

  const handleSubmit = (
    e: React.FormEvent
  ) => {
    e.preventDefault()

    if (isListeningRef.current) {
      stopListening()
    }

    handleSendMessage(inputMessage)
  }

  return (
    <div className="assistant-container">
      {isOpen && (
        <div
          className="assistant-chatbox"
          role="dialog"
          aria-label="Asistan Penceresi"
        >
          <div className="assistant-header">
            <div className="assistant-profile">
              <div className="assistant-avatar">
                <span className="bot-icon">
                  🤖
                </span>

                <span className="status-indicator" />
              </div>

              <div className="assistant-info">
                <h3>Akıllı Asistan</h3>

                <span className="online-status">
                  Çevrimiçi • Yardımcı olmaya hazır
                </span>
              </div>
            </div>

            <button
              className="close-btn"
              onClick={() => {
                if (isListeningRef.current) {
                  stopListening()
                }
                setIsOpen(false)
              }}
              aria-label="Pencereyi kapat"
              title="Kapat"
            >
              ✕
            </button>
          </div>

          <div className="assistant-messages">
            {messages.map((msg) => (
              <div
                key={msg.id}
                className={`chat-bubble ${
                  msg.sender === 'user'
                    ? 'bubble-user'
                    : 'bubble-bot'
                }`}
              >
                <div className="bubble-text">
                  {msg.text}
                </div>

                <div className="bubble-time">
                  {msg.time}
                </div>
              </div>
            ))}

            {isTyping && (
              <div className="chat-bubble bubble-bot typing-bubble">
                <span className="typing-dot" />
                <span className="typing-dot" />
                <span className="typing-dot" />
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          <div className="quick-suggestions">
            {quickQuestions.map(
              (question, index) => (
                <button
                  key={index}
                  type="button"
                  className="quick-btn"
                  onClick={() => {
                    if (isListeningRef.current) {
                      stopListening()
                    }
                    handleSendMessage(question)
                  }}
                  disabled={isTyping}
                >
                  {question}
                </button>
              )
            )}
          </div>

          <form
            className="assistant-input-area"
            onSubmit={handleSubmit}
          >
            <input
              type="text"
              placeholder={
                isListening
                  ? 'Dinleniyor... Konuşun...'
                  : 'Bir soru yazın...'
              }
              value={inputMessage}
              onChange={(e) => {
                setInputMessage(e.target.value)
                if (isListeningRef.current) {
                  baseTextRef.current = e.target.value
                  finalTranscriptRef.current = ''
                }
              }}
              className={`assistant-input ${
                isListening ? 'listening' : ''
              }`}
              disabled={isTyping}
            />

            <button
              type="button"
              className={`assistant-mic-btn ${
                isListening ? 'listening' : ''
              }`}
              onClick={toggleListening}
              disabled={isTyping || !isSpeechSupported}
              aria-label={
                isListening
                  ? 'Dikteyi durdur'
                  : 'Dikte ile yaz'
              }
              title={
                !isSpeechSupported
                  ? 'Tarayıcınız ses tanımayı desteklemiyor'
                  : isListening
                  ? 'Dikteyi durdur'
                  : 'Dikte ile yaz (tr-TR)'
              }
            >
              {isListening ? (
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="currentColor"
                >
                  <rect x="6" y="6" width="12" height="12" rx="2" />
                </svg>
              ) : (
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3Z" />
                  <path d="M19 10v2a7 7 0 0 1-14 0v-2" />
                  <line x1="12" y1="19" x2="12" y2="22" />
                  <line x1="8" y1="22" x2="16" y2="22" />
                </svg>
              )}
            </button>

            <button
              type="submit"
              className="assistant-send-btn"
              disabled={
                !inputMessage.trim() ||
                isTyping
              }
              aria-label="Gönder"
              title="Gönder"
            >
              <svg
                width="18"
                height="18"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
              </svg>
            </button>
          </form>
        </div>
      )}

      <button
        className={`assistant-trigger-btn ${
          isOpen ? 'active' : ''
        }`}
        onClick={() => {
          if (isOpen && isListeningRef.current) {
            stopListening()
          }
          setIsOpen(!isOpen)
        }}
        aria-label={
          isOpen
            ? 'Asistanı kapat'
            : 'Asistanı aç'
        }
        title="Asistanı Aç/Kapat"
      >
        <span className="trigger-icon">
          {isOpen ? '✕' : '💬'}
        </span>

        {!isOpen && (
          <span className="notification-ping" />
        )}
      </button>
    </div>
  )
}

export default AssistantWidget

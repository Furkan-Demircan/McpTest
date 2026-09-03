import React, { useEffect, useRef, useState } from 'react'
import './AssistantWidget.css'
import { sendAssistantMessage } from '../services/assistantApi'

interface Message {
  id: string
  sender: 'bot' | 'user'
  text: string
  time: string
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
  const [isOpen, setIsOpen] = useState(false)
  const [inputMessage, setInputMessage] = useState('')
  const [isTyping, setIsTyping] = useState(false)

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
    'Hangi bilgiler gerekli?',
    'Kayıtları nasıl görürüm?',
    'TC No güvenli mi?',
    'Formu nasıl gönderebilirim?',
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

  const handleSendMessage = async (messageText: string) => {
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

    setMessages((prev) => [
      ...prev,
      userMessage,
    ])

    setInputMessage('')
    setIsTyping(true)

    try {
      const response = await sendAssistantMessage(
        trimmedMessage
      )

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
              onClick={() =>
                setIsOpen(false)
              }
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
                  onClick={() =>
                    handleSendMessage(question)
                  }
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
              placeholder="Bir soru yazın..."
              value={inputMessage}
              onChange={(e) =>
                setInputMessage(
                  e.target.value
                )
              }
              className="assistant-input"
              disabled={isTyping}
            />

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
        onClick={() =>
          setIsOpen(!isOpen)
        }
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

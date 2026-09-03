import React, { useState, useRef, useEffect } from 'react'
import './AssistantWidget.css'

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
  return `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`
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
      text: 'Merhaba! 👋 Ben form ve işlem asistanınız. Formu doldururken veya sistem hakkında aklınıza takılan bir soru olursa yardımcı olmaktan mutluluk duyarım.',
      time: 'Şimdi',
    },
  ])

  const quickQuestions = [
    'Hangi bilgiler gerekli?',
    'TC No güvenli mi?',
    'Formu nasıl gönderebilirim?',
  ]

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    if (isOpen) {
      scrollToBottom()
    }
  }, [messages, isOpen, isTyping])

  const generateBotReply = (userText: string): string => {
    const text = userText.toLowerCase()
    if (text.includes('hangi bilgi') || text.includes('gerekli') || text.includes('alan')) {
      return 'Formda Ad, Soyad, 11 haneli TC Kimlik Numarası, E-posta adresi, Anne Adı, Baba Adı ve Doğum Tarihi alanları doldurulmalıdır.'
    }
    if (text.includes('tc') || text.includes('güven') || text.includes('guven')) {
      return 'TC Kimlik Numaranız yalnızca doğrulama ve kayıt amaçlı kullanılır; üçüncü taraflarla paylaşılmaz.'
    }
    if (text.includes('gönder') || text.includes('gonder') || text.includes('kaydet')) {
      return 'Tüm zorunlu alanları eksiksiz doldurduktan sonra formun altındaki "Formu Kaydet" butonuna tıklayarak formu gönderebilirsiniz.'
    }
    if (text.includes('merhaba') || text.includes('selam')) {
      return 'Merhaba! Size nasıl yardımcı olabilirim?'
    }
    return `"${userText}" sorunuz için teşekkürler! Formdaki alanları doğru girdiğinizden emin olup "Formu Kaydet" butonunu kullanabilirsiniz. Başka bir konuda yardımcı olabilir miyim?`
  }

  const handleSendMessage = (messageText: string) => {
    if (!messageText.trim()) return

    const timeStr = getCurrentTimeString()
    const newMsg: Message = {
      id: getNextId(),
      sender: 'user',
      text: messageText,
      time: timeStr,
    }

    setMessages((prev) => [...prev, newMsg])
    setInputMessage('')
    setIsTyping(true)

    setTimeout(() => {
      const botResponse: Message = {
        id: getNextId(),
        sender: 'bot',
        text: generateBotReply(messageText),
        time: getCurrentTimeString(),
      }
      setMessages((prev) => [...prev, botResponse])
      setIsTyping(false)
    }, 600)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    handleSendMessage(inputMessage)
  }

  return (
    <div className="assistant-container">
      {/* Açılır Sohbet Paneli */}
      {isOpen && (
        <div className="assistant-chatbox" role="dialog" aria-label="Asistan Penceresi">
          {/* Header */}
          <div className="assistant-header">
            <div className="assistant-profile">
              <div className="assistant-avatar">
                <span className="bot-icon">🤖</span>
                <span className="status-indicator"></span>
              </div>
              <div className="assistant-info">
                <h3>Akıllı Asistan</h3>
                <span className="online-status">Çevrimiçi • Yardımcı olmaya hazır</span>
              </div>
            </div>
            <button
              className="close-btn"
              onClick={() => setIsOpen(false)}
              aria-label="Pencereyi kapat"
              title="Kapat"
            >
              ✕
            </button>
          </div>

          {/* Mesaj Alanı */}
          <div className="assistant-messages">
            {messages.map((msg) => (
              <div
                key={msg.id}
                className={`chat-bubble ${msg.sender === 'user' ? 'bubble-user' : 'bubble-bot'}`}
              >
                <div className="bubble-text">{msg.text}</div>
                <div className="bubble-time">{msg.time}</div>
              </div>
            ))}

            {isTyping && (
              <div className="chat-bubble bubble-bot typing-bubble">
                <span className="typing-dot"></span>
                <span className="typing-dot"></span>
                <span className="typing-dot"></span>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Hızlı Öneri Butonları */}
          <div className="quick-suggestions">
            {quickQuestions.map((q, idx) => (
              <button
                key={idx}
                type="button"
                className="quick-btn"
                onClick={() => handleSendMessage(q)}
              >
                {q}
              </button>
            ))}
          </div>

          {/* Input & Form */}
          <form className="assistant-input-area" onSubmit={handleSubmit}>
            <input
              type="text"
              placeholder="Bir soru yazın..."
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              className="assistant-input"
            />
            <button
              type="submit"
              className="assistant-send-btn"
              disabled={!inputMessage.trim()}
              aria-label="Gönder"
              title="Gönder"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
              </svg>
            </button>
          </form>
        </div>
      )}

      {/* Sağ Alttaki Yuvarlak Asistan Butonu */}
      <button
        className={`assistant-trigger-btn ${isOpen ? 'active' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        aria-label={isOpen ? 'Asistanı kapat' : 'Asistanı aç'}
        title="Asistanı Aç/Kapat"
      >
        <span className="trigger-icon">{isOpen ? '✕' : '💬'}</span>
        {!isOpen && <span className="notification-ping"></span>}
      </button>
    </div>
  )
}

export default AssistantWidget

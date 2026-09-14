import React from 'react'
import { Link } from 'react-router-dom'
import heroImg from '../assets/hero.png'
import reactLogo from '../assets/react.svg'
import viteLogo from '../assets/vite.svg'
import './HomePage.css'

export const HomePage: React.FC = () => {
  return (
    <section id="center" className="home-container">
      <div className="hero">
        <img src={heroImg} className="base" width="170" height="179" alt="" />
        <img src={reactLogo} className="framework" alt="React logo" />
        <img src={viteLogo} className="vite" alt="Vite logo" />
      </div>

      <div className="home-content">
        <h1>Kişisel Bilgi & Okul Yönetim Sistemi</h1>
        <p className="home-subtitle">
          Öğrenci ve öğretmen kayıtlarını güvenli, doğrulamalı formlar üzerinden sisteme ekleyebilir veya veritabanındaki tüm kayıtları inceleyebilirsiniz.
        </p>

        <div className="home-cta">
          <Link to="/form" className="btn-go-to-form">
            <span>👨‍🎓 Öğrenci Ekle</span>
            <span className="btn-arrow">→</span>
          </Link>
          <Link to="/teacher" className="btn-go-to-teacher">
            <span>👩‍🏫 Öğretmen Ekle</span>
            <span className="btn-arrow">→</span>
          </Link>
          <Link to="/users" className="btn-go-to-list">
            <span>📋 Kayıtlı Kullanıcıları Gör</span>
          </Link>
        </div>

        <div className="features-preview">
          <div className="feature-badge">
            <span className="badge-dot"></span>
            👨‍🎓 Öğrenci Kayıt & Ebeveyn Bilgileri
          </div>
          <div className="feature-badge">
            <span className="badge-dot teacher-dot"></span>
            👩‍🏫 Öğretmen & Branş Yönetimi
          </div>
          <div className="feature-badge">
            <span className="badge-dot"></span>
            🔒 11 Haneli TC Kimlik & E-posta Doğrulaması
          </div>
        </div>
      </div>
    </section>
  )
}

export default HomePage

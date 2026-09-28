import React from 'react'
import { Link } from 'react-router-dom'
import { navLinks } from '../app/appManifest'
import { useManifest } from '../app/useManifest'
import heroImg from '../assets/hero.png'
import reactLogo from '../assets/react.svg'
import viteLogo from '../assets/vite.svg'
import './HomePage.css'

// Form linkleri sırayla bu renkleri alır; formsuz sayfalar liste stilinde görünür
const FORM_LINK_CLASSES = ['btn-go-to-form', 'btn-go-to-teacher']

export const HomePage: React.FC = () => {
  // Menü manifest'ten gelir: [AppForm(NavLabel = ...)] ile eklenen her form burada görünür
  const links = navLinks(useManifest())
  const formLinks = links.filter((link) => link.page.formId)
  const otherLinks = links.filter((link) => !link.page.formId)

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
          {formLinks.map((link, index) => (
            <Link
              key={link.id}
              id={link.id}
              to={link.page.path}
              className={FORM_LINK_CLASSES[index % FORM_LINK_CLASSES.length]}
            >
              <span>{link.label}</span>
              <span className="btn-arrow">→</span>
            </Link>
          ))}
          {otherLinks.map((link) => (
            <Link key={link.id} id={link.id} to={link.page.path} className="btn-go-to-list">
              <span>📋 {link.label}</span>
            </Link>
          ))}
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

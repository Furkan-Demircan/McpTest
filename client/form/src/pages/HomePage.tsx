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
        <h1>Kişisel Bilgi Sistemi</h1>
        <p className="home-subtitle">
          Kullanıcı bilgilerini güvenli ve hızlı bir şekilde girmek için aşağıdaki butonu kullanarak form sayfasına geçiş yapabilirsiniz.
        </p>

        <div className="home-cta">
          <Link to="/form" className="btn-go-to-form">
            <span>Form Sayfasına Git</span>
            <span className="btn-arrow">→</span>
          </Link>
        </div>

        <div className="features-preview">
          <div className="feature-badge">
            <span className="badge-dot"></span>
            Ad, Soyad ve TC Kimlik Doğrulaması
          </div>
          <div className="feature-badge">
            <span className="badge-dot"></span>
            İletişim ve Ebeveyn Bilgileri
          </div>
          <div className="feature-badge">
            <span className="badge-dot"></span>
            Doğum Tarihi Kaydı
          </div>
        </div>
      </div>
    </section>
  )
}
export default HomePage

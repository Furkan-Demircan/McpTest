import React, { useEffect, useState, useMemo } from 'react'
import { Link } from 'react-router-dom'
import { api, type UserResponse } from '../services/api'
import './UsersListPage.css'

export const UsersListPage: React.FC = () => {
  const [users, setUsers] = useState<UserResponse[]>([])
  const [isLoading, setIsLoading] = useState<boolean>(true)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState<string>('')

  const loadUsers = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await api.getUsers()
      setUsers(data)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message)
      } else {
        setError('Kayıtlar yüklenirken bir sorun oluştu.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    let ignore = false

    const fetchInitialData = async () => {
      try {
        const data = await api.getUsers()
        if (!ignore) {
          setUsers(data)
          setIsLoading(false)
        }
      } catch (err: unknown) {
        if (!ignore) {
          setError(err instanceof Error ? err.message : 'Kayıtlar yüklenirken bir sorun oluştu.')
          setIsLoading(false)
        }
      }
    }

    fetchInitialData()

    return () => {
      ignore = true
    }
  }, [])

  // Arama filtresi
  const filteredUsers = useMemo(() => {
    if (!searchQuery.trim()) return users

    const query = searchQuery.toLowerCase().trim()
    return users.filter(
      (u) =>
        u.firstName.toLowerCase().includes(query) ||
        u.lastName.toLowerCase().includes(query) ||
        u.tcNo.includes(query) ||
        u.email.toLowerCase().includes(query) ||
        u.motherName.toLowerCase().includes(query) ||
        u.fatherName.toLowerCase().includes(query)
    )
  }, [users, searchQuery])

  return (
    <div className="users-list-container">
      {/* Üst Başlık & Navigasyon */}
      <div className="list-page-header">
        <div className="header-nav">
          <Link to="/" className="nav-btn-back">
            ← Ana Sayfa
          </Link>
          <Link to="/form" className="nav-btn-add">
            + Yeni Kullanıcı Ekle
          </Link>
        </div>

        <div className="header-titles">
          <h1>Kayıtlı Kullanıcılar</h1>
          <p className="header-desc">
            PostgreSQL veritabanında kayıtlı tüm kullanıcıların listesi
          </p>
        </div>
      </div>

      {/* Kontrol Barı: Arama ve İstatistik */}
      <div className="list-controls-card">
        <div className="search-box">
          <span className="search-icon">🔍</span>
          <input
            type="text"
            placeholder="Ad, soyad, TC No veya e-posta ile ara..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
          {searchQuery && (
            <button
              className="clear-search-btn"
              onClick={() => setSearchQuery('')}
              title="Aramayı temizle"
            >
              ✕
            </button>
          )}
        </div>

        <div className="list-stats">
          <button
            type="button"
            className="btn-refresh"
            onClick={loadUsers}
            disabled={isLoading}
            title="Listeyi Yenile"
          >
            {isLoading ? '⏳' : '🔄'} Yenile
          </button>
          <div className="total-badge">
            Toplam <strong>{users.length}</strong> kayıt
          </div>
        </div>
      </div>

      {/* Hata Durumu */}
      {error && (
        <div className="list-error-banner" role="alert">
          <div className="error-content">
            <span className="error-icon">⚠️</span>
            <div>
              <strong>Bağlantı Hatası:</strong>
              <p>{error}</p>
            </div>
          </div>
          <button type="button" className="btn-retry" onClick={loadUsers}>
            Tekrar Dene
          </button>
        </div>
      )}

      {/* Yükleniyor Durumu */}
      {isLoading ? (
        <div className="list-loading-state">
          <div className="spinner"></div>
          <p>Kullanıcı kayıtları yükleniyor...</p>
        </div>
      ) : filteredUsers.length === 0 ? (
        /* Kayıt Bulunamadı / Boş Durum */
        <div className="empty-state-card">
          <div className="empty-icon">{searchQuery ? '🔎' : '👥'}</div>
          <h3>
            {searchQuery
              ? `"${searchQuery}" ile eşleşen kayıt bulunamadı`
              : 'Henüz hiç kullanıcı kaydı bulunmuyor'}
          </h3>
          <p>
            {searchQuery
              ? 'Farklı bir arama terimi deneyebilir veya arama filtresini temizleyebilirsiniz.'
              : 'Veritabanına ilk kaydı eklemek için formu doldurabilirsiniz.'}
          </p>
          {searchQuery ? (
            <button
              type="button"
              className="btn-secondary"
              onClick={() => setSearchQuery('')}
            >
              Aramayı Temizle
            </button>
          ) : (
            <Link to="/form" className="btn-primary">
              İlk Kullanıcıyı Ekle
            </Link>
          )}
        </div>
      ) : (
        /* Kullanıcılar Tablosu */
        <div className="table-responsive-wrapper">
          <table className="users-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Ad Soyad</th>
                <th>TC Kimlik No</th>
                <th>E-posta</th>
                <th>Ebeveyn (Anne / Baba)</th>
                <th>Doğum Tarihi</th>
                <th>Kayıt Tarihi</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map((user, index) => (
                <tr key={user.id}>
                  <td className="col-index">{index + 1}</td>
                  <td className="col-name">
                    <strong>
                      {user.firstName} {user.lastName}
                    </strong>
                  </td>
                  <td className="col-tc">
                    <span className="tc-tag">{user.tcNo}</span>
                  </td>
                  <td className="col-email">{user.email}</td>
                  <td className="col-parents">
                    <span className="parent-info">
                      👩 {user.motherName} / 👨 {user.fatherName}
                    </span>
                  </td>
                  <td className="col-birth">{user.birthDate}</td>
                  <td className="col-created">
                    {new Date(user.createdAt).toLocaleDateString('tr-TR', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

export default UsersListPage

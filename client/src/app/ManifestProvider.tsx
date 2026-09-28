import React, { useEffect, useState, type ReactNode } from 'react'
import { ManifestContext } from './ManifestContext'
import type { AppManifest } from './appManifest'
import { api } from '../services/api'

/**
 * Uygulama manifest'ini sunucudan çeker; yüklenene kadar uygulamayı render etmez.
 * Route'lar, menü, formlar ve form durumu bu manifest'ten kurulur.
 */
export const ManifestProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [manifest, setManifest] = useState<AppManifest | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let ignore = false

    api
      .getAppManifest()
      .then((data) => {
        if (!ignore) setManifest(data)
      })
      .catch((err: unknown) => {
        if (!ignore) setError(err instanceof Error ? err.message : String(err))
      })

    return () => {
      ignore = true
    }
  }, [])

  if (error) {
    return (
      <div className="list-loading-state" role="alert">
        <p>Uygulama tanımı yüklenemedi: {error}</p>
        <button type="button" className="btn-primary" onClick={() => window.location.reload()}>
          Tekrar Dene
        </button>
      </div>
    )
  }

  if (!manifest) {
    return (
      <div className="list-loading-state">
        <div className="spinner"></div>
        <p>Uygulama yükleniyor...</p>
      </div>
    )
  }

  return <ManifestContext.Provider value={manifest}>{children}</ManifestContext.Provider>
}

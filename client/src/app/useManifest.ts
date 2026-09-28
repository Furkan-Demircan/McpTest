import { useContext } from 'react'
import { ManifestContext } from './ManifestContext'
import type { AppManifest } from './appManifest'

export function useManifest(): AppManifest {
  const manifest = useContext(ManifestContext)

  if (!manifest) {
    throw new Error('useManifest must be used within a ManifestProvider')
  }

  return manifest
}

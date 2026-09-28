import { createContext } from 'react'
import type { AppManifest } from './appManifest'

export const ManifestContext = createContext<AppManifest | undefined>(undefined)

import type { ReactElement } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import HomePage from './pages/HomePage'
import GenericFormPage from './pages/GenericFormPage'
import UsersListPage from './pages/UsersListPage'
import AssistantWidget from './components/AssistantWidget'
import { FormProvider } from './contexts/FormProvider'
import { ManifestProvider } from './app/ManifestProvider'
import { useManifest } from './app/useManifest'
import type { PageDefinition } from './app/appManifest'
import './App.css'

// Elle yazılmış özel sayfalar (manifest page.id → bileşen). Formu olan sayfalar
// buraya yazılmaz, GenericFormPage ile otomatik render edilir; genel renderer'ın
// yetmediği bir form için buraya özel bileşen eklemek kaçış kapısıdır.
const PAGE_COMPONENTS: Record<string, ReactElement> = {
  home: <HomePage />,
  users: <UsersListPage />,
}

function pageElement(page: PageDefinition): ReactElement {
  const custom = PAGE_COMPONENTS[page.id]
  if (custom) return custom
  if (page.formId) return <GenericFormPage key={page.formId} formId={page.formId} />

  console.warn(`Sayfa için bileşen yok: ${page.id}`)
  return <Navigate to="/" replace />
}

function AppRoutes() {
  const manifest = useManifest()

  return (
    <Routes>
      {manifest.pages.map((page) => (
        <Route key={page.id} path={page.path} element={pageElement(page)} />
      ))}

      {/* Alias'lar kanonik path'e yönlenir; böylece form/sayfa eşlemesi tek path üzerinden yapılır */}
      {manifest.pages.flatMap((page) =>
        page.aliases.map((alias) => (
          <Route key={alias} path={alias} element={<Navigate to={page.path} replace />} />
        ))
      )}

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

function App() {
  return (
    <ManifestProvider>
      <FormProvider>
        <AppRoutes />
        <AssistantWidget />
      </FormProvider>
    </ManifestProvider>
  )
}

export default App

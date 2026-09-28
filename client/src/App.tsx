import type { ReactElement } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import HomePage from './pages/HomePage'
import FormPage from './pages/FormPage'
import TeacherFormPage from './pages/TeacherFormPage'
import UsersListPage from './pages/UsersListPage'
import AssistantWidget from './components/AssistantWidget'
import { FormProvider } from './contexts/FormProvider'
import { appManifest } from './app/appManifest'
import './App.css'

// Manifest'teki sayfa kimliği → bileşen
const PAGE_COMPONENTS: Record<string, ReactElement> = {
  home: <HomePage />,
  'student-create': <FormPage />,
  'teacher-create': <TeacherFormPage />,
  users: <UsersListPage />,
}

function App() {
  return (
    <FormProvider>
      <>
        <Routes>
          {appManifest.pages.map((page) => (
            <Route key={page.id} path={page.path} element={PAGE_COMPONENTS[page.id]} />
          ))}

          {/* Alias'lar kanonik path'e yönlenir; böylece form/sayfa eşlemesi tek path üzerinden yapılır */}
          {appManifest.pages.flatMap((page) =>
            page.aliases.map((alias) => (
              <Route key={alias} path={alias} element={<Navigate to={page.path} replace />} />
            ))
          )}

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>

        <AssistantWidget />
      </>
    </FormProvider>
  )
}

export default App

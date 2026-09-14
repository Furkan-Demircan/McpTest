import { Routes, Route, Navigate } from 'react-router-dom'
import HomePage from './pages/HomePage'
import FormPage from './pages/FormPage'
import TeacherFormPage from './pages/TeacherFormPage'
import UsersListPage from './pages/UsersListPage'
import AssistantWidget from './components/AssistantWidget'
import { FormProvider } from './contexts/FormProvider'
import './App.css'

function App() {
  return (
    <FormProvider>
      <>
        <Routes>
          <Route path="/" element={<HomePage />} />
          
          {/* Öğrenci Ekleme Formu */}
          <Route path="/form" element={<FormPage />} />
          <Route path="/ogrenci" element={<FormPage />} />
          <Route path="/ogrenci-ekle" element={<FormPage />} />
          <Route path="/student" element={<FormPage />} />
          <Route path="/student-form" element={<FormPage />} />
          <Route
            path="/forma"
            element={<Navigate to="/form" replace />}
          />

          {/* Öğretmen Ekleme Formu */}
          <Route path="/teacher" element={<TeacherFormPage />} />
          <Route path="/teacher-form" element={<TeacherFormPage />} />
          <Route path="/ogretmen" element={<TeacherFormPage />} />
          <Route path="/ogretmen-ekle" element={<TeacherFormPage />} />

          {/* Kayıtlar Listesi */}
          <Route path="/users" element={<UsersListPage />} />
          <Route
            path="/kayitlar"
            element={<Navigate to="/users" replace />}
          />
          <Route
            path="/liste"
            element={<Navigate to="/users" replace />}
          />

          <Route
            path="*"
            element={<Navigate to="/" replace />}
          />
        </Routes>

        <AssistantWidget />
      </>
    </FormProvider>
  )
}

export default App

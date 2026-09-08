import { Routes, Route, Navigate } from 'react-router-dom'
import HomePage from './pages/HomePage'
import FormPage from './pages/FormPage'
import UsersListPage from './pages/UsersListPage'
import AssistantWidget from './components/AssistantWidget'
import './App.css'

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/form" element={<FormPage />} />
        <Route path="/forma" element={<Navigate to="/form" replace />} />
        <Route path="/users" element={<UsersListPage />} />
        <Route path="/kayitlar" element={<Navigate to="/users" replace />} />
        <Route path="/liste" element={<Navigate to="/users" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <AssistantWidget />
    </>
  )
}

export default App

import { Routes, Route, Navigate } from 'react-router-dom'
import HomePage from './pages/HomePage'
import FormPage from './pages/FormPage'
import AssistantWidget from './components/AssistantWidget'
import './App.css'

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/form" element={<FormPage />} />
        {/* Kullanıcı /forma yazarsa da /form'a yönlendirilsin */}
        <Route path="/forma" element={<Navigate to="/form" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <AssistantWidget />
    </>
  )
}

export default App

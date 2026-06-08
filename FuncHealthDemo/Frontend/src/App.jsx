import { Navigate, Route, Routes } from 'react-router-dom'
import './App.css'
import { AuthProvider } from './context/AuthContext'
import DashboardPage from './pages/DashboardPage'
import LoginPage from './pages/LoginPage'
import OnboardingPage from './pages/OnboardingPage'
import SignupPage from './pages/SignupPage'
import CreateTaskPage from './pages/CreateTaskPage'
import { ROUTES } from './lib/auth'

function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path={ROUTES.login} element={<LoginPage />} />
        <Route path={ROUTES.signup} element={<SignupPage />} />
        <Route path={ROUTES.onboarding} element={<OnboardingPage />} />
        <Route path={ROUTES.createTask} element={<CreateTaskPage />} />
        <Route path={ROUTES.dashboard} element={<DashboardPage />} />
        <Route path="*" element={<Navigate to={ROUTES.login} replace />} />
      </Routes>
    </AuthProvider>
  )
}

export default App

import { Navigate, useNavigate } from 'react-router-dom'
import AuthLayout from '../components/AuthLayout'
import OnboardingForm from '../components/OnboardingForm'
import { useAuth } from '../context/AuthContext'
import { hasCompletedProfile, ROUTES } from '../lib/auth'

export default function OnboardingPage() {
  const navigate = useNavigate()
  const { activeUser, setActiveUser } = useAuth()

  if (!activeUser) {
    return <Navigate to={ROUTES.login} replace />
  }

  if (hasCompletedProfile(activeUser)) {
    return <Navigate to={ROUTES.dashboard} replace />
  }

  const handleCompleted = (profileData) => {
    setActiveUser((currentUser) => ({
      ...currentUser,
      ...profileData,
      onboardingComplete: true,
    }))
    navigate(ROUTES.dashboard)
  }

  return (
    <AuthLayout>
      <OnboardingForm onCompleted={handleCompleted} />
    </AuthLayout>
  )
}

import { useState } from 'react'
import { signInWithEmailAndPassword } from 'firebase/auth'
import { Link, useNavigate } from 'react-router-dom'
import { apiClient, AUTH_USER_ID_MAP_KEY } from '../api/apiClient'
import AuthLayout from '../components/AuthLayout'
import { useAuth } from '../context/AuthContext'
import { auth, firebaseSetupError } from '../firebase/firebaseConfig'
import { hasCompletedProfile, ROUTES } from '../lib/auth'
import { normalizeBackendUserPayload } from '../lib/userPayload'

function getFirebaseLoginErrorMessage(errorCode) {
  switch (errorCode) {
    case 'auth/invalid-credential':
    case 'auth/user-not-found':
    case 'auth/wrong-password':
      return 'Incorrect email or password. Please try again.'
    case 'auth/invalid-email':
      return 'Please enter a valid email address.'
    case 'auth/user-disabled':
      return 'This account is disabled. Please contact support.'
    case 'auth/network-request-failed':
      return 'Network issue detected. Please check your connection and try again.'
    default:
      return 'Unable to log in right now. Please try again.'
  }
}

function getStoredUserIdForUid(uid) {
  try {
    if (!uid) {
      return null
    }

    const rawMap = window.localStorage.getItem(AUTH_USER_ID_MAP_KEY)
    if (!rawMap) {
      return null
    }

    const parsedMap = JSON.parse(rawMap)
    const value = parsedMap?.[uid]
    if (value === null || value === undefined || value === '') {
      return null
    }

    return value
  } catch {
    return null
  }
}

export default function LoginPage() {
  const navigate = useNavigate()
  const { setActiveUser } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [helperMessage, setHelperMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleContinue = async (event) => {
    event.preventDefault()

    const normalizedEmail = email.trim().toLowerCase()
    if (!normalizedEmail) {
      setHelperMessage('Please enter an email address to continue.')
      return
    }

    if (!password.trim()) {
      setHelperMessage('Please enter your password to continue.')
      return
    }

    setHelperMessage('')
    setIsSubmitting(true)

    try {
      if (firebaseSetupError || !auth) {
        setHelperMessage(
          'Firebase is not configured in this frontend yet. Please set the Firebase environment variables and reload the app.'
        )
        return
      }

      const firebaseResult = await signInWithEmailAndPassword(
        auth,
        normalizedEmail,
        password
      )

      let profileData = {}
      try {
        const storedUserId = getStoredUserIdForUid(firebaseResult.user.uid)
        if (storedUserId !== null) {
          const profileResponse = await apiClient.get(
            `/users/${encodeURIComponent(String(storedUserId))}`,
            {
              headers: {
                'X-User-Id': firebaseResult.user.uid,
              },
            }
          )
          profileData = normalizeBackendUserPayload(profileResponse?.data)
        }
      } catch {
        // Let onboarding capture missing profile fields when profile endpoint is unavailable.
      }

      const nextActiveUser = {
        ...profileData,
        uid: firebaseResult.user.uid,
        email: firebaseResult.user.email || normalizedEmail,
      }

      setActiveUser(nextActiveUser)

      if (hasCompletedProfile(nextActiveUser)) {
        navigate(ROUTES.dashboard)
      } else {
        navigate(ROUTES.onboarding)
      }
    } catch (error) {
      if (error?.code?.startsWith('auth/')) {
        setHelperMessage(getFirebaseLoginErrorMessage(error.code))
      } else {
        setHelperMessage(error.message || 'Unable to log in right now. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout>
      <section aria-labelledby="login-heading">
        <form className="login-form" onSubmit={handleContinue}>
          <h1 id="login-heading">Log in</h1>
          <p className="subtext">Enter your email and password to continue.</p>

          <label className="sr-only" htmlFor="email">
            Email
          </label>
          <input
            id="email"
            type="email"
            placeholder="Email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            autoComplete="email"
            className="text-input"
          />

          <label className="sr-only" htmlFor="password">
            Password
          </label>
          <input
            id="password"
            type="password"
            placeholder="Password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            className="text-input"
          />

          <button type="submit" className="continue-button" disabled={isSubmitting}>
            Continue
          </button>

          <p className="signup-row">
            Not a member yet? <Link to={ROUTES.signup}>Sign up.</Link>
          </p>

          {helperMessage ? (
            <p className="helper-message" role="status" aria-live="polite">
              {helperMessage}
            </p>
          ) : null}
        </form>
      </section>
    </AuthLayout>
  )
}

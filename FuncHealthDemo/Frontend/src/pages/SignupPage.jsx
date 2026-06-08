import { useState } from 'react'
import { createUserWithEmailAndPassword } from 'firebase/auth'
import { Link, useNavigate } from 'react-router-dom'
import { apiClient } from '../api/apiClient'
import AuthLayout from '../components/AuthLayout'
import { useAuth } from '../context/AuthContext'
import { auth, firebaseSetupError } from '../firebase/firebaseConfig'
import { EMAIL_PATTERN, ROUTES } from '../lib/auth'
import { normalizeBackendUserPayload } from '../lib/userPayload'

function getFirebaseErrorMessage(errorCode) {
  switch (errorCode) {
    case 'auth/configuration-not-found':
      return 'Firebase Authentication is not configured for this project yet. Please contact support or try again later.'
    case 'auth/email-already-in-use':
      return 'An account with this email already exists. Please log in instead.'
    case 'auth/invalid-email':
      return 'Please enter a valid email address.'
    case 'auth/weak-password':
      return 'Your password is too weak. Please use at least 6 characters.'
    case 'auth/network-request-failed':
      return 'Network issue detected. Please check your connection and try again.'
    default:
      return 'We could not create your account right now. Please try again.'
  }
}

export default function SignupPage() {
  const navigate = useNavigate()
  const { setActiveUser } = useAuth()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [helperMessage, setHelperMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleCreateAccount = async (event) => {
    event.preventDefault()

    if (
      !firstName.trim() ||
      !lastName.trim() ||
      !email.trim() ||
      !password.trim() ||
      !confirmPassword.trim()
    ) {
      setHelperMessage('Please complete every field before creating your account.')
      return
    }

    if (!EMAIL_PATTERN.test(email.trim().toLowerCase())) {
      setHelperMessage('Please enter a valid email address.')
      return
    }

    if (password.length < 6) {
      setHelperMessage('Password must be at least 6 characters long.')
      return
    }

    if (password !== confirmPassword) {
      setHelperMessage('Password and Confirm Password must match.')
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

      const normalizedEmail = email.trim().toLowerCase()
      const firebaseResult = await createUserWithEmailAndPassword(
        auth,
        normalizedEmail,
        password
      )

      const response = await apiClient.post('/users', {
        uid: firebaseResult.user.uid,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: firebaseResult.user.email || normalizedEmail,
      })

      const profileData = normalizeBackendUserPayload(response?.data)

      setActiveUser({
        ...profileData,
        uid: firebaseResult.user.uid,
        userId: profileData.userId,
        email: firebaseResult.user.email || normalizedEmail,
        firstName: profileData.firstName ?? firstName.trim(),
        lastName: profileData.lastName ?? lastName.trim(),
        onboardingComplete: false,
      })
      navigate(ROUTES.onboarding)
    } catch (error) {
      if (error?.code?.startsWith('auth/')) {
        setHelperMessage(getFirebaseErrorMessage(error.code))
      } else {
        setHelperMessage(error.message || 'Unable to create your account right now.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout>
      <section aria-labelledby="signup-heading">
        <form className="login-form signup-form" onSubmit={handleCreateAccount}>
          <h1 id="signup-heading">Create your account</h1>
          <p className="subtext">Enter your details to get started.</p>

          <label className="sr-only" htmlFor="firstName">
            First Name
          </label>
          <input
            id="firstName"
            type="text"
            placeholder="First Name"
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
            autoComplete="given-name"
            className="text-input"
          />

          <label className="sr-only" htmlFor="lastName">
            Last Name
          </label>
          <input
            id="lastName"
            type="text"
            placeholder="Last Name"
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
            autoComplete="family-name"
            className="text-input"
          />

          <label className="sr-only" htmlFor="signupEmail">
            Email
          </label>
          <input
            id="signupEmail"
            type="email"
            placeholder="Email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            autoComplete="email"
            className="text-input"
          />

          <label className="sr-only" htmlFor="signupPassword">
            Password
          </label>
          <input
            id="signupPassword"
            type="password"
            placeholder="Password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="new-password"
            className="text-input"
          />

          <label className="sr-only" htmlFor="confirmPassword">
            Confirm Password
          </label>
          <input
            id="confirmPassword"
            type="password"
            placeholder="Confirm Password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            autoComplete="new-password"
            className="text-input"
          />

          <button type="submit" className="continue-button" disabled={isSubmitting}>
            Create Account
          </button>

          <p className="signup-row">
            Already a member? <Link to={ROUTES.login}>Log in.</Link>
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

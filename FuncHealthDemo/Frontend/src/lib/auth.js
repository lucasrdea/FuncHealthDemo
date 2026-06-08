export const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export const MOCK_USERS_BY_EMAIL = {
  'alice@example.com': { userId: 1, email: 'alice@example.com' },
  'bob@example.com': { userId: 2, email: 'bob@example.com' },
}

export const ROUTES = {
  login: '/',
  signup: '/signup',
  onboarding: '/onboarding',
  createTask: '/create-task',
  dashboard: '/dashboard',
}

export function hasCompletedProfile(user) {
  if (!user) {
    return false
  }

  const hasPhoneNumber = typeof user.phoneNumber === 'string' && user.phoneNumber.trim().length > 0

  return hasPhoneNumber
}

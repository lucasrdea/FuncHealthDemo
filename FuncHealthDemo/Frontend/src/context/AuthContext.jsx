import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { AUTH_STORAGE_KEY, AUTH_USER_ID_MAP_KEY } from '../api/apiClient'

const AuthContext = createContext(null)

function getBackendUserId(user) {
  const candidateUserId = user?.userId ?? user?.id ?? user?.UserId ?? user?.ID
  return candidateUserId !== null && candidateUserId !== undefined ? Number(candidateUserId) : null
}

function getInitialActiveUser() {
  try {
    const storedUser = window.localStorage.getItem(AUTH_STORAGE_KEY)
    return storedUser ? JSON.parse(storedUser) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [activeUser, setActiveUser] = useState(getInitialActiveUser)

  const signOut = () => {
    try {
      window.localStorage.removeItem(AUTH_STORAGE_KEY)
    } catch {
      // Ignore localStorage failures and keep in-memory auth state.
    } finally {
      setActiveUser(null)
    }
  }

  useEffect(() => {
    try {
      const backendUserId = getBackendUserId(activeUser)

      if (activeUser?.uid && backendUserId !== null && !Number.isNaN(backendUserId)) {
        const existingMapRaw = window.localStorage.getItem(AUTH_USER_ID_MAP_KEY)
        const existingMap = existingMapRaw ? JSON.parse(existingMapRaw) : {}
        const nextMap = {
          ...existingMap,
          [activeUser.uid]: backendUserId,
        }
        window.localStorage.setItem(AUTH_USER_ID_MAP_KEY, JSON.stringify(nextMap))
      }

      if (activeUser) {
        window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(activeUser))
      } else {
        window.localStorage.removeItem(AUTH_STORAGE_KEY)
      }
    } catch {
      // Ignore localStorage failures and keep in-memory auth state.
    }
  }, [activeUser])

  const value = useMemo(() => ({ activeUser, setActiveUser, signOut }), [activeUser])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }

  return context
}

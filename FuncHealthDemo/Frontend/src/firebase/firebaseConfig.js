import { initializeApp } from 'firebase/app'
import { getAuth } from 'firebase/auth'

const firebaseEnvConfig = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID,
  storageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET,
  messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID,
  appId: import.meta.env.VITE_FIREBASE_APP_ID,
}

const requiredFirebaseKeys = [
  'apiKey',
  'authDomain',
  'projectId',
  'storageBucket',
  'messagingSenderId',
  'appId',
]

const missingFirebaseKeys = requiredFirebaseKeys.filter(
  (key) => !firebaseEnvConfig[key]
)

const firebaseSetupError = missingFirebaseKeys.length
  ? `Firebase is not configured. Missing: ${missingFirebaseKeys.join(', ')}`
  : null

let auth = null

if (!firebaseSetupError) {
  const firebaseApp = initializeApp(firebaseEnvConfig)
  auth = getAuth(firebaseApp)
} else {
  console.warn(firebaseSetupError)
}

export { auth, firebaseSetupError }

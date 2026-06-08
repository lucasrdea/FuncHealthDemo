import { auth } from '../firebase/firebaseConfig'

const API_BASE_URL = 'http://localhost:5000/api'
const AUTH_STORAGE_KEY = 'task-manager-active-user'
const AUTH_USER_ID_MAP_KEY = 'task-manager-user-id-map'

async function getBearerToken(forceRefresh = false) {
  try {
    if (auth) {
      await auth.authStateReady()
      const currentUser = auth.currentUser
      if (currentUser) {
        return await currentUser.getIdToken(forceRefresh)
      }
    }
  } catch {
    // Fall through if token retrieval fails
  }
  return null
}

function extractErrorMessage(responseBody) {
  if (!responseBody) {
    return null
  }

  if (typeof responseBody === 'string') {
    const trimmed = responseBody.trim()
    return trimmed.length > 0 ? trimmed : null
  }

  if (typeof responseBody !== 'object') {
    return null
  }

  if (typeof responseBody.error === 'string' && responseBody.error.trim()) {
    return responseBody.error.trim()
  }

  if (typeof responseBody.message === 'string' && responseBody.message.trim()) {
    return responseBody.message.trim()
  }

  if (Array.isArray(responseBody.request) && responseBody.request.length > 0) {
    const firstRequestError = responseBody.request.find(
      (item) => typeof item === 'string' && item.trim().length > 0
    )
    if (firstRequestError) {
      return firstRequestError.trim()
    }
  }

  if (typeof responseBody.request === 'string' && responseBody.request.trim()) {
    return responseBody.request.trim()
  }

  if (responseBody.errors && typeof responseBody.errors === 'object') {
    for (const value of Object.values(responseBody.errors)) {
      if (Array.isArray(value)) {
        const firstItem = value.find((item) => typeof item === 'string' && item.trim().length > 0)
        if (firstItem) {
          return firstItem.trim()
        }
      }

      if (typeof value === 'string' && value.trim()) {
        return value.trim()
      }
    }
  }

  if (typeof responseBody.title === 'string' && responseBody.title.trim()) {
    return responseBody.title.trim()
  }

  return null
}

async function request(path, options = {}) {
  const executeRequest = async (forceRefreshToken = false) => {
    const headers = new Headers(options.headers ?? {})

    if (!headers.has('Content-Type') && options.body) {
      headers.set('Content-Type', 'application/json')
    }

    const token = await getBearerToken(forceRefreshToken)
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }

    return fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers,
    })
  }

  let response = await executeRequest(false)

  if (response.status === 401) {
    response = await executeRequest(true)
  }

  const contentType = response.headers.get('content-type')
  const isJsonResponse = contentType?.includes('application/json')
  const responseBody = isJsonResponse ? await response.json() : await response.text()

  if (!response.ok) {
    const extractedMessage = extractErrorMessage(responseBody)
    const error = new Error(extractedMessage || 'Request failed. Please try again.')

    error.status = response.status
    error.body = responseBody
    throw error
  }

  return {
    status: response.status,
    data: responseBody,
  }
}

export const apiClient = {
  get(path, options) {
    return request(path, { ...options, method: 'GET' })
  },
  post(path, body, options) {
    return request(path, {
      ...options,
      method: 'POST',
      body: JSON.stringify(body),
    })
  },
  put(path, body, options) {
    return request(path, {
      ...options,
      method: 'PUT',
      body: JSON.stringify(body),
    })
  },
  delete(path, options) {
    return request(path, { ...options, method: 'DELETE' })
  },
}

export { API_BASE_URL, AUTH_STORAGE_KEY, AUTH_USER_ID_MAP_KEY, getBearerToken }

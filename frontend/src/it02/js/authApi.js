import { getAuthToken } from './authStorage'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5020').replace(/\/$/, '')

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    },
    ...options
  })

  if (!response.ok) {
    let message = 'ไม่สามารถเชื่อมต่อกับระบบได้'

    try {
      const payload = await response.json()
      message = payload.message || message
    } catch {
      // no-op
    }

    throw new Error(message)
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export function registerUser(payload) {
  return request('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function loginUser(payload) {
  return request('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function fetchCurrentUser() {
  const token = getAuthToken()
  return request('/api/auth/me', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  })
}

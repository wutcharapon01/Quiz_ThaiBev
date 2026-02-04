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
    let message = 'ไม่สามารถทำรายการได้'
    try {
      const payload = await response.json()
      message = payload.message || message
    } catch {
      // no-op
    }
    throw new Error(message)
  }

  return response.json()
}

export function fetchIt09Thread() {
  return request('/api/it09/thread')
}

export function createIt09Comment(message) {
  return request('/api/it09/comments', {
    method: 'POST',
    body: JSON.stringify({ message })
  })
}
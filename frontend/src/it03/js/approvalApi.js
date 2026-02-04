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
    let message = 'ทำรายการไม่สำเร็จ'
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

export function fetchIt03Documents() {
  return request('/api/it03/documents')
}

export function submitIt03Decision(payload) {
  return request('/api/it03/documents/decision', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function resetIt03Documents() {
  return request('/api/it03/documents/reset', {
    method: 'POST'
  })
}

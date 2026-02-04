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
    let message = 'ไม่สามารถบันทึกข้อมูลได้'

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

export function getOccupations() {
  return request('/api/it04/occupations')
}

export function createIt04Profile(payload) {
  return request('/api/it04/profiles', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
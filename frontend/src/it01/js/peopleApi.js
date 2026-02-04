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
    let message = 'Request failed'
    try {
      const body = await response.json()
      message = body.message || message
    } catch {
      // ignore json parse error
    }
    throw new Error(message)
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export function fetchPeople() {
  return request('/api/people')
}

export function fetchPersonById(id) {
  return request(`/api/people/${id}`)
}

export function createPerson(payload) {
  return request('/api/people', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
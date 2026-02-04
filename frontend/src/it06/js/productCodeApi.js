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

export function fetchProductCodes() {
  return request('/api/it06/codes')
}

export function createProductCode(productCode) {
  return request('/api/it06/codes', {
    method: 'POST',
    body: JSON.stringify({ productCode })
  })
}

export function deleteProductCode(id) {
  return request(`/api/it06/codes/${id}`, {
    method: 'DELETE'
  })
}
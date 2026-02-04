/**
 * Security: Using sessionStorage instead of localStorage
 * - sessionStorage clears when tab/browser closes (limits token exposure)
 * - If XSS occurs, attacker has shorter window to steal token
 * - For production: Consider HttpOnly cookies instead
 */

const TOKEN_KEY = 'quiz.auth.jwt_token'
const USERNAME_KEY = 'quiz.auth.username'

// Helper to encode values (basic XSS mitigation)
function encodeValue(value) {
  if (!value) return ''
  return btoa(encodeURIComponent(value))
}

function decodeValue(encoded) {
  if (!encoded) return ''
  try {
    return decodeURIComponent(atob(encoded))
  } catch {
    return ''
  }
}

export function saveAuthSession(token, username) {
  // Use sessionStorage for security - clears when tab closes
  sessionStorage.setItem(TOKEN_KEY, encodeValue(token))
  sessionStorage.setItem(USERNAME_KEY, encodeValue(username))
}

export function getAuthToken() {
  return decodeValue(sessionStorage.getItem(TOKEN_KEY))
}

export function getSavedUsername() {
  return decodeValue(sessionStorage.getItem(USERNAME_KEY))
}

export function clearAuthSession() {
  sessionStorage.removeItem(TOKEN_KEY)
  sessionStorage.removeItem(USERNAME_KEY)
}

// Migration: Clear old localStorage tokens if they exist
if (typeof window !== 'undefined') {
  if (localStorage.getItem(TOKEN_KEY)) {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USERNAME_KEY)
  }
}
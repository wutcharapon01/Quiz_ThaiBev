const TOKEN_KEY = 'quiz.auth.jwt_token'
const USERNAME_KEY = 'quiz.auth.username'

export function saveAuthSession(token, username) {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USERNAME_KEY, username)
}

export function getAuthToken() {
  return localStorage.getItem(TOKEN_KEY) || ''
}

export function getSavedUsername() {
  return localStorage.getItem(USERNAME_KEY) || ''
}

export function clearAuthSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USERNAME_KEY)
}
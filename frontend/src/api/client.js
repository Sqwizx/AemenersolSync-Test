// frontend/src/api/client.js
const BASE_URL = 'https://localhost:7208' // backend HTTPS port from backend/AemenersolSync.Api/Properties/launchSettings.json ("https" profile)

export class UnauthorizedError extends Error {}

async function request(path, options = {}) {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (response.status === 401) {
    throw new UnauthorizedError('Session expired or not authenticated')
  }

  if (!response.ok) {
    throw new Error(`Request to ${path} failed with status ${response.status}`)
  }

  const text = await response.text()
  return text ? JSON.parse(text) : null
}

export function login() {
  return request('/api/auth/login', { method: 'POST', body: JSON.stringify({}) })
}

export function logout() {
  return request('/api/auth/logout', { method: 'POST' })
}

export function getStatus() {
  return request('/api/auth/status')
}

export function getSyncData() {
  return request('/api/sync/data')
}

export function triggerSync() {
  return request('/api/sync', { method: 'POST' })
}

import { apiBaseUrl } from './api'

export interface AuthenticatedUser {
  id: string
  email: string
  role: string
}

export interface WebSession {
  accessToken: string
  accessTokenExpiresAt: string
  user: AuthenticatedUser
}

let currentSession: WebSession | null = null
let refreshRequest: Promise<WebSession | null> | null = null

async function webAuthRequest(
  path: string,
  body?: object,
): Promise<Response> {
  return fetch(`${apiBaseUrl}/auth/web${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      'X-ReciclaFacil-Web': '1',
    },
    body: body ? JSON.stringify(body) : undefined,
  })
}

export async function login(email: string, password: string): Promise<WebSession> {
  const response = await webAuthRequest('/login', { email, password })
  if (response.status === 401) throw new Error('E-mail ou senha inválidos.')
  if (!response.ok) throw new Error('Não foi possível entrar. Tente novamente.')
  currentSession = await response.json() as WebSession
  return currentSession
}

export function restoreSession(): Promise<WebSession | null> {
  if (refreshRequest) return refreshRequest

  refreshRequest = webAuthRequest('/refresh')
    .then(async (response) => {
      if (response.status === 401) {
        currentSession = null
        return null
      }
      if (!response.ok) throw new Error('Não foi possível restaurar a sessão.')
      currentSession = await response.json() as WebSession
      return currentSession
    })
    .finally(() => {
      refreshRequest = null
    })

  return refreshRequest
}

export async function logout(): Promise<void> {
  try {
    await webAuthRequest('/logout')
  } finally {
    currentSession = null
  }
}

export async function authorizedFetch(
  path: string,
  init: RequestInit = {},
): Promise<Response> {
  let session = currentSession
  if (!session) session = await restoreSession()
  if (!session) throw new Error('Sua sessão expirou. Entre novamente.')

  const request = () => fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      ...init.headers,
      Authorization: `Bearer ${session?.accessToken}`,
    },
  })

  let response = await request()
  if (response.status === 401) {
    session = await restoreSession()
    if (!session) throw new Error('Sua sessão expirou. Entre novamente.')
    response = await request()
  }
  return response
}

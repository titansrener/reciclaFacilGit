export const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '/api/v1').replace(/\/$/, '')

export async function apiGet<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    if (response.status === 404) throw new Error('Registro não encontrado.')
    throw new Error('A API está temporariamente indisponível. Tente novamente em instantes.')
  }

  return response.json() as Promise<T>
}

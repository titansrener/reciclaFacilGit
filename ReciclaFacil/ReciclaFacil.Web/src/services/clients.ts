import { authorizedFetch } from './auth'

export interface ClientCollectionSummary {
  id: number
  scheduledAt: string | null
  collectedAt: string | null
  status: string
  materialCount: number
}

export interface ClientNotificationSummary {
  id: number
  collectionId: number
  createdAt: string | null
  description: string
  type: string
  active: boolean
  requiresValueDecision: boolean
  offeredValue: number | null
}

export interface ClientOverview {
  name: string
  cooperativeName: string
  walletBalance: number
  activeNotificationCount: number
  collections: ClientCollectionSummary[]
  recentNotifications: ClientNotificationSummary[]
}

export interface ClientCollectionOptions {
  slots: Array<{ id: number, scheduledAt: string }>
  materials: Array<{ id: number, description: string }>
}

export interface ClientCollectionDetails {
  id: number
  scheduledAt: string | null
  collectedAt: string | null
  status: string
  materials: Array<{
    id: number
    description: string
    quantity: number | null
    value: number | null
  }>
}

async function problemMessage(response: Response, fallback: string) {
  try {
    const problem = await response.json() as { title?: string }
    return problem.title || fallback
  } catch {
    return fallback
  }
}

export async function getClientOverview(): Promise<ClientOverview> {
  const response = await authorizedFetch('/clients/me/overview')
  if (response.status === 403) throw new Error('Este painel é exclusivo para clientes.')
  if (!response.ok) throw new Error('Não foi possível carregar o painel.')
  return response.json() as Promise<ClientOverview>
}

export async function getCollectionOptions(): Promise<ClientCollectionOptions> {
  const response = await authorizedFetch('/clients/me/collection-options')
  if (!response.ok) throw new Error('Não foi possível carregar os horários disponíveis.')
  return response.json() as Promise<ClientCollectionOptions>
}

export async function scheduleCollection(collectionId: number, materialIds: number[]) {
  const response = await authorizedFetch('/clients/me/collections', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ collectionId, materialIds }),
  })
  if (!response.ok) {
    throw new Error(await problemMessage(response, 'Não foi possível agendar a coleta.'))
  }
}

export async function getCollectionDetails(id: number): Promise<ClientCollectionDetails> {
  const response = await authorizedFetch(`/clients/me/collections/${id}`)
  if (response.status === 404) throw new Error('Coleta não encontrada.')
  if (!response.ok) throw new Error('Não foi possível carregar a coleta.')
  return response.json() as Promise<ClientCollectionDetails>
}

export async function cancelCollection(id: number) {
  const response = await authorizedFetch(`/clients/me/collections/${id}`, {
    method: 'DELETE',
  })
  if (!response.ok) {
    throw new Error(await problemMessage(response, 'Não foi possível cancelar a coleta.'))
  }
}

export async function decideCollectionOffer(id: number, decision: 'accept' | 'reject') {
  const response = await authorizedFetch(`/clients/me/collections/${id}/offer/${decision}`, {
    method: 'PUT',
  })
  if (!response.ok) {
    throw new Error(await problemMessage(response, 'Não foi possível responder à oferta.'))
  }
  return response.json() as Promise<{ value: number }>
}

export async function readNotification(id: number) {
  const response = await authorizedFetch(`/clients/me/notifications/${id}/read`, {
    method: 'PUT',
  })
  if (!response.ok) {
    throw new Error(await problemMessage(response, 'Não foi possível atualizar a notificação.'))
  }
}

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
  createdAt: string | null
  description: string
  type: string
  active: boolean
}

export interface ClientOverview {
  name: string
  cooperativeName: string
  walletBalance: number
  activeNotificationCount: number
  collections: ClientCollectionSummary[]
  recentNotifications: ClientNotificationSummary[]
}

export async function getClientOverview(): Promise<ClientOverview> {
  const response = await authorizedFetch('/clients/me/overview')
  if (response.status === 403) throw new Error('Este painel é exclusivo para clientes.')
  if (!response.ok) throw new Error('Não foi possível carregar o painel.')
  return response.json() as Promise<ClientOverview>
}

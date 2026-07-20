import { authorizedFetch } from './auth'

export interface EmployeeCollectionSummary {
  id: number
  scheduledAt: string | null
  status: string
  pendingClients: number
  completedClients: number
}
export interface EmployeeOverview {
  id: string
  name: string
  cooperativeName: string
  collections: EmployeeCollectionSummary[]
}
export interface EmployeeClientSummary {
  id: string
  name: string
  type: string
  address: string
  mobile: string
  status: string | null
  latitude: number | null
  longitude: number | null
}
export interface EmployeeCollectionDetails {
  id: number
  scheduledAt: string | null
  status: string
  trucks: Array<{ id: number; plate: string; description: string }>
  clients: EmployeeClientSummary[]
}
export interface EmployeeCollectionRoute {
  collectionId: number
  origin: EmployeeRoutePoint
  stops: EmployeeRouteStop[]
  unmappedStops: number
}
export interface EmployeeRoutePoint {
  name: string
  address: string
  latitude: number | null
  longitude: number | null
}
export interface EmployeeRouteStop extends EmployeeRoutePoint {
  sequence: number
  clientId: string
  mobile: string
}
export interface EmployeeClientDetails {
  id: string
  name: string
  type: string
  address: string
  phone: string | null
  mobile: string
  collectionId: number
  collectionStatus: string
  status: string | null
  collectedAt: string | null
  materials: Array<{
    id: number
    description: string
    quantity: number | null
    purchaseValue: number | null
    status: string | null
  }>
}

async function read<T>(path: string): Promise<T> {
  const response = await authorizedFetch(path)
  if (!response.ok) throw new Error('Não foi possível carregar os dados do funcionário.')
  return response.json() as Promise<T>
}
export const getEmployeeOverview = () => read<EmployeeOverview>('/employees/me/overview')
export const getEmployeeCollection = (id: number) =>
  read<EmployeeCollectionDetails>(`/employees/me/collections/${id}`)
export const getEmployeeRoute = (id: number) =>
  read<EmployeeCollectionRoute>(`/employees/me/collections/${id}/route`)
export const getEmployeeClient = (collectionId: number, clientId: string) =>
  read<EmployeeClientDetails>(`/employees/me/collections/${collectionId}/clients/${clientId}`)

export async function recordMaterials(
  collectionId: number, clientId: string, materials: Array<{ materialId: number; quantity: number }>,
): Promise<{ totalValue: number }> {
  const response = await authorizedFetch(
    `/employees/me/collections/${collectionId}/clients/${clientId}/materials`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ materials }),
    },
  )
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { title?: string } | null
    throw new Error(problem?.title ?? 'Não foi possível registrar os materiais.')
  }
  return response.json() as Promise<{ totalValue: number }>
}

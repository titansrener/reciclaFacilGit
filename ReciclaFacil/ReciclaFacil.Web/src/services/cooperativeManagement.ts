import { authorizedFetch } from './auth'

export interface CooperativeCollection {
  id: number
  scheduledAt: string | null
  status: string
  clientCount: number
  quantity: number | null
}

export interface ManagedMaterial {
  id: number
  description: string
  resalePrice: number | null
}

export interface MaterialOption {
  id: number
  description: string
}

export interface CooperativeOverview {
  id: string
  name: string
  scheduledCollections: number
  inProgressCollections: number
  finishedCollections: number
  registeredClients: number
  collections: CooperativeCollection[]
  materials: ManagedMaterial[]
  materialOptions: MaterialOption[]
}

export interface CooperativeCollectionDetails {
  id: number
  scheduledAt: string | null
  status: string
  quantity: number | null
  clients: Array<{
    id: string
    name: string
    status: string | null
    collectedAt: string | null
    materials: string[]
  }>
  trucks: Truck[]
  employees: Employee[]
}

export interface Truck { id: number; description: string; plate: string }
export interface Employee { id: string; name: string; birthDate: string; email: string }
export interface CooperativeResources { trucks: Truck[]; employees: Employee[] }

async function parseError(response: Response) {
  if (response.ok) return
  const problem = await response.json().catch(() => null) as { title?: string } | null
  throw new Error(problem?.title ?? 'Não foi possível concluir a operação.')
}

export async function getCooperativeOverview(): Promise<CooperativeOverview> {
  const response = await authorizedFetch('/cooperatives/me/overview')
  await parseError(response)
  return response.json() as Promise<CooperativeOverview>
}

export async function getManagedCollection(id: number): Promise<CooperativeCollectionDetails> {
  const response = await authorizedFetch(`/cooperatives/me/collections/${id}`)
  await parseError(response)
  return response.json() as Promise<CooperativeCollectionDetails>
}

export async function createCollection(scheduledAt: string) {
  const response = await authorizedFetch('/cooperatives/me/collections', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ scheduledAt }),
  })
  await parseError(response)
}

export async function collectionAction(id: number, action: 'start' | 'finish' | 'delete') {
  const path = action === 'delete'
    ? `/cooperatives/me/collections/${id}`
    : `/cooperatives/me/collections/${id}/${action}`
  const response = await authorizedFetch(path, { method: action === 'delete' ? 'DELETE' : 'POST' })
  await parseError(response)
}

export async function addManagedMaterial(materialId: number, resalePrice: number | null) {
  const response = await authorizedFetch('/cooperatives/me/materials', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ materialId, resalePrice }),
  })
  await parseError(response)
}

export async function updateManagedMaterial(materialId: number, resalePrice: number | null) {
  const response = await authorizedFetch(`/cooperatives/me/materials/${materialId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ resalePrice }),
  })
  await parseError(response)
}

export async function removeManagedMaterial(materialId: number) {
  const response = await authorizedFetch(`/cooperatives/me/materials/${materialId}`, { method: 'DELETE' })
  await parseError(response)
}

export async function getCooperativeResources(): Promise<CooperativeResources> {
  const response = await authorizedFetch('/cooperatives/me/resources')
  await parseError(response)
  return response.json() as Promise<CooperativeResources>
}

export async function createTruck(description: string, plate: string) {
  const response = await authorizedFetch('/cooperatives/me/trucks', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ description, plate }),
  })
  await parseError(response)
}

export async function deleteTruck(id: number) {
  const response = await authorizedFetch(`/cooperatives/me/trucks/${id}`, { method: 'DELETE' })
  await parseError(response)
}

export async function createEmployee(
  name: string, birthDate: string, email: string, password: string,
) {
  const response = await authorizedFetch('/cooperatives/me/employees', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, birthDate, email, password }),
  })
  await parseError(response)
}

export async function deleteEmployee(id: string) {
  const response = await authorizedFetch(`/cooperatives/me/employees/${id}`, { method: 'DELETE' })
  await parseError(response)
}

export async function setCollectionResource(
  collectionId: number, resource: 'trucks' | 'employees', id: number | string, assign: boolean,
) {
  const response = await authorizedFetch(
    `/cooperatives/me/collections/${collectionId}/${resource}/${id}`,
    { method: assign ? 'PUT' : 'DELETE' },
  )
  await parseError(response)
}

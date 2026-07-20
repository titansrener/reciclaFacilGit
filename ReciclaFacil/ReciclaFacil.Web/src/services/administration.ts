import { authorizedFetch } from './auth'

export interface AdminMaterial {
  id: number
  description: string
  averageDecompositionTime: number
}

async function problem(response: Response) {
  if (response.ok) return
  const details = await response.json().catch(() => null) as { title?: string } | null
  throw new Error(details?.title ?? 'Não foi possível concluir a operação.')
}

export async function listAdminMaterials(): Promise<AdminMaterial[]> {
  const response = await authorizedFetch('/admin/materials')
  await problem(response)
  return response.json() as Promise<AdminMaterial[]>
}

export async function createAdminMaterial(
  description: string, averageDecompositionTime: number,
) {
  const response = await authorizedFetch('/admin/materials', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ description, averageDecompositionTime }),
  })
  await problem(response)
}

export async function updateAdminMaterial(
  id: number, description: string, averageDecompositionTime: number,
) {
  const response = await authorizedFetch(`/admin/materials/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ description, averageDecompositionTime }),
  })
  await problem(response)
}

export async function deleteAdminMaterial(id: number) {
  const response = await authorizedFetch(`/admin/materials/${id}`, { method: 'DELETE' })
  await problem(response)
}

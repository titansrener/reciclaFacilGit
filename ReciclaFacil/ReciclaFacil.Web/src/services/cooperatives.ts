import { apiGet } from './api'

export interface CooperativeListItem {
  id: string
  name: string
  address: string
  city: string
  state: string
  latitude: number | null
  longitude: number | null
}

export interface CommercializedMaterial {
  id: number
  description: string
  resalePrice: number | null
}

export interface CooperativeDetails {
  id: string
  name: string
  taxId: string
  address: string
  city: string
  state: string
  email: string | null
  materials: CommercializedMaterial[]
}

export interface CooperativeSearch {
  name: string
  city: string
  state: string
  page: number
  pageSize: number
}

interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
}

export function searchCooperatives(search: CooperativeSearch, signal?: AbortSignal) {
  const params = new URLSearchParams({
    page: String(search.page),
    pageSize: String(search.pageSize),
  })

  if (search.name.trim()) params.set('name', search.name.trim())
  if (search.city.trim()) params.set('city', search.city.trim())
  if (search.state.trim()) params.set('state', search.state.trim())

  return apiGet<PagedResult<CooperativeListItem>>(`/cooperatives?${params}`, signal)
}

export function getCooperative(id: string) {
  return apiGet<CooperativeDetails>(`/cooperatives/${encodeURIComponent(id)}`)
}

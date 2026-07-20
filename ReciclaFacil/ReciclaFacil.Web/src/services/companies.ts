import { authorizedFetch } from './auth'

export interface CompanyOverview {
  id: string
  cnpj: string
  corporateName: string
  address: string
  phone: string
  fax: string | null
  email: string
}

export async function getCompanyOverview(): Promise<CompanyOverview> {
  const response = await authorizedFetch('/companies/me/overview', {
    headers: { Accept: 'application/json' },
  })
  if (response.status === 404) throw new Error('Perfil da empresa não encontrado.')
  if (!response.ok) throw new Error('Não foi possível carregar os dados da empresa.')
  return response.json() as Promise<CompanyOverview>
}

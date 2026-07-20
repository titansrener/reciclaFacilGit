import { apiBaseUrl } from './api'

export interface RegistrationCooperative {
  id: string
  name: string
  city: string
  state: string
}

export interface ClientRegistration {
  email: string
  password: string
  confirmPassword: string
  cpf: string
  type: string
  name: string
  address: string
  gender: string
  birthDate: string
  phone: string
  mobile: string
  cooperativeId: string
}

export interface CooperativeRegistration {
  email: string
  password: string
  confirmPassword: string
  cnpj: string
  corporateName: string
  address: string
  city: string
  state: string
}

export interface RegisteredAccount {
  id: string
  email: string
  role: string
}

export class RegistrationRequestError extends Error {
  readonly fieldErrors: Record<string, string[]>

  constructor(
    message: string,
    fieldErrors: Record<string, string[]> = {},
  ) {
    super(message)
    this.fieldErrors = fieldErrors
  }
}

export async function listRegistrationCooperatives(): Promise<RegistrationCooperative[]> {
  const response = await fetch(`${apiBaseUrl}/auth/register/cooperatives`, {
    headers: { Accept: 'application/json' },
  })
  if (!response.ok) throw new Error('Não foi possível carregar as cooperativas.')
  return response.json() as Promise<RegistrationCooperative[]>
}

export function registerClient(data: ClientRegistration): Promise<RegisteredAccount> {
  return register('/auth/register/clients', data)
}

export function registerCooperative(data: CooperativeRegistration): Promise<RegisteredAccount> {
  return register('/auth/register/cooperatives', data)
}

async function register(path: string, data: object): Promise<RegisteredAccount> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  })

  if (response.ok) return response.json() as Promise<RegisteredAccount>

  const problem = await response.json().catch(() => ({})) as {
    title?: string
    detail?: string
    errors?: Record<string, string[]>
  }
  throw new RegistrationRequestError(
    problem.detail ?? problem.title ?? 'Não foi possível criar a conta.',
    problem.errors,
  )
}

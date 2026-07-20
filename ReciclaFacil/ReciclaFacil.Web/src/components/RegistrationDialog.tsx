import { useEffect, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import {
  listRegistrationCooperatives,
  registerClient,
  registerCooperative,
  RegistrationRequestError,
} from '../services/registrations'
import type {
  ClientRegistration,
  CooperativeRegistration,
  RegistrationCooperative,
} from '../services/registrations'

interface RegistrationDialogProps {
  onClose: () => void
  onRegistered: (email: string) => void
}

const emptyClient: ClientRegistration = {
  email: '', password: '', confirmPassword: '', cpf: '', type: 'F', name: '',
  address: '', gender: '', birthDate: '', phone: '', mobile: '', cooperativeId: '',
}

const emptyCooperative: CooperativeRegistration = {
  email: '', password: '', confirmPassword: '', cnpj: '', corporateName: '',
  address: '', city: '', state: '',
}

export function RegistrationDialog({ onClose, onRegistered }: RegistrationDialogProps) {
  const [profile, setProfile] = useState<'client' | 'cooperative'>('client')
  const [client, setClient] = useState(emptyClient)
  const [cooperative, setCooperative] = useState(emptyCooperative)
  const [cooperatives, setCooperatives] = useState<RegistrationCooperative[]>([])
  const [loadingOptions, setLoadingOptions] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({})

  useEffect(() => {
    listRegistrationCooperatives()
      .then(setCooperatives)
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar as cooperativas.',
      ))
      .finally(() => setLoadingOptions(false))
  }, [])

  useEffect(() => {
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', closeOnEscape)
    return () => document.removeEventListener('keydown', closeOnEscape)
  }, [onClose])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    setFieldErrors({})
    try {
      const account = profile === 'client'
        ? await registerClient(client)
        : await registerCooperative(cooperative)
      onRegistered(account.email)
    } catch (reason) {
      if (reason instanceof RegistrationRequestError) {
        setError(reason.message)
        setFieldErrors(reason.fieldErrors)
      } else {
        setError(reason instanceof Error ? reason.message : 'Não foi possível criar a conta.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  const fieldError = (name: string) => fieldErrors[name]?.[0]

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section
        className="registration-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="registration-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Faça parte da rede</span>
        <h2 id="registration-title">Crie sua conta.</h2>
        <div className="registration-tabs" role="tablist" aria-label="Tipo de conta">
          <button
            type="button"
            role="tab"
            aria-selected={profile === 'client'}
            onClick={() => { setProfile('client'); setError(''); setFieldErrors({}) }}
          >
            Sou cliente
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={profile === 'cooperative'}
            onClick={() => { setProfile('cooperative'); setError(''); setFieldErrors({}) }}
          >
            Sou cooperativa
          </button>
        </div>

        <form className="registration-form" onSubmit={submit}>
          {profile === 'client' ? (
            <>
              <Field label="Nome" error={fieldError('name')}>
                <input required maxLength={75} value={client.name}
                  onChange={(e) => setClient({ ...client, name: e.target.value })} />
              </Field>
              <Field label="CPF (opcional)" error={fieldError('cpf')}>
                <input inputMode="numeric" maxLength={11} value={client.cpf}
                  onChange={(e) => setClient({ ...client, cpf: digits(e.target.value) })} />
              </Field>
              <Field label="Tipo" error={fieldError('type')}>
                <select value={client.type}
                  onChange={(e) => setClient({ ...client, type: e.target.value })}>
                  <option value="F">Pessoa física</option>
                  <option value="V">Vendedor</option>
                </select>
              </Field>
              <Field label="Gênero" error={fieldError('gender')}>
                <select required value={client.gender}
                  onChange={(e) => setClient({ ...client, gender: e.target.value })}>
                  <option value="">Selecione</option>
                  <option value="F">Feminino</option>
                  <option value="M">Masculino</option>
                  <option value="O">Outro / prefiro não informar</option>
                </select>
              </Field>
              <Field label="Data de nascimento" error={fieldError('birthDate')}>
                <input required type="date" value={client.birthDate}
                  onChange={(e) => setClient({ ...client, birthDate: e.target.value })} />
              </Field>
              <Field label="Endereço" error={fieldError('address')} wide>
                <input required maxLength={100} value={client.address}
                  onChange={(e) => setClient({ ...client, address: e.target.value })} />
              </Field>
              <Field label="Telefone (opcional)" error={fieldError('phone')}>
                <input inputMode="tel" maxLength={11} value={client.phone}
                  onChange={(e) => setClient({ ...client, phone: digits(e.target.value) })} />
              </Field>
              <Field label="Celular" error={fieldError('mobile')}>
                <input required inputMode="tel" maxLength={11} value={client.mobile}
                  onChange={(e) => setClient({ ...client, mobile: digits(e.target.value) })} />
              </Field>
              <Field label="Cooperativa" error={fieldError('cooperativeId')} wide>
                <select required disabled={loadingOptions} value={client.cooperativeId}
                  onChange={(e) => setClient({ ...client, cooperativeId: e.target.value })}>
                  <option value="">{loadingOptions ? 'Carregando…' : 'Selecione uma cooperativa'}</option>
                  {cooperatives.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.name} — {item.city}/{item.state}
                    </option>
                  ))}
                </select>
              </Field>
            </>
          ) : (
            <>
              <Field label="Razão social" error={fieldError('corporateName')} wide>
                <input required maxLength={100} value={cooperative.corporateName}
                  onChange={(e) => setCooperative({ ...cooperative, corporateName: e.target.value })} />
              </Field>
              <Field label="CNPJ" error={fieldError('cnpj')}>
                <input required inputMode="numeric" maxLength={14} value={cooperative.cnpj}
                  onChange={(e) => setCooperative({ ...cooperative, cnpj: digits(e.target.value) })} />
              </Field>
              <Field label="UF" error={fieldError('state')}>
                <input required maxLength={2} value={cooperative.state}
                  onChange={(e) => setCooperative({ ...cooperative, state: letters(e.target.value) })} />
              </Field>
              <Field label="Cidade" error={fieldError('city')}>
                <input required maxLength={80} value={cooperative.city}
                  onChange={(e) => setCooperative({ ...cooperative, city: e.target.value })} />
              </Field>
              <Field label="Endereço" error={fieldError('address')}>
                <input required maxLength={150} value={cooperative.address}
                  onChange={(e) => setCooperative({ ...cooperative, address: e.target.value })} />
              </Field>
            </>
          )}

          <Field label="E-mail" error={fieldError('email')} wide>
            <input required type="email" autoComplete="email"
              value={profile === 'client' ? client.email : cooperative.email}
              onChange={(e) => profile === 'client'
                ? setClient({ ...client, email: e.target.value })
                : setCooperative({ ...cooperative, email: e.target.value })} />
          </Field>
          <Field label="Senha" error={fieldError('password')}>
            <input required minLength={6} maxLength={100} type="password" autoComplete="new-password"
              value={profile === 'client' ? client.password : cooperative.password}
              onChange={(e) => profile === 'client'
                ? setClient({ ...client, password: e.target.value })
                : setCooperative({ ...cooperative, password: e.target.value })} />
          </Field>
          <Field label="Confirmar senha" error={fieldError('confirmPassword')}>
            <input required minLength={6} maxLength={100} type="password" autoComplete="new-password"
              value={profile === 'client' ? client.confirmPassword : cooperative.confirmPassword}
              onChange={(e) => profile === 'client'
                ? setClient({ ...client, confirmPassword: e.target.value })
                : setCooperative({ ...cooperative, confirmPassword: e.target.value })} />
          </Field>

          {error && <div className="login-error registration-error" role="alert">{error}</div>}
          <button className="registration-submit" type="submit" disabled={submitting || loadingOptions}>
            {submitting ? 'Criando conta…' : 'Criar minha conta'}
          </button>
        </form>
      </section>
    </div>
  )
}

function Field({
  label,
  error,
  wide = false,
  children,
}: {
  label: string
  error?: string
  wide?: boolean
  children: ReactNode
}) {
  return (
    <label className={wide ? 'field-wide' : undefined}>
      <span>{label}</span>
      {children}
      {error && <small className="field-error">{error}</small>}
    </label>
  )
}

function digits(value: string) {
  return value.replace(/\D/g, '')
}

function letters(value: string) {
  return value.replace(/[^a-z]/gi, '').toUpperCase()
}

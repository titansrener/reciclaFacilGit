import { useState } from 'react'
import type { FormEvent } from 'react'
import { requestPasswordReset } from '../services/auth'

interface Props {
  onClose: () => void
  onDevelopmentToken: (token: string) => void
}

export function ForgotPasswordDialog({ onClose, onDevelopmentToken }: Props) {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [developmentToken, setDevelopmentToken] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      const token = await requestPasswordReset(email)
      setDevelopmentToken(token)
      setMessage('Se o e-mail estiver cadastrado, você receberá as instruções de redefinição.')
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível solicitar a redefinição.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="login-modal" role="dialog" aria-modal="true"
        aria-labelledby="forgot-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Recuperar acesso</span>
        <h2 id="forgot-title">Esqueceu sua senha?</h2>
        <p>Informe o e-mail da conta. A resposta é sempre a mesma para proteger seus dados.</p>
        {!message ? (
          <form className="login-form" onSubmit={submit}>
            <label>
              <span>E-mail</span>
              <input required autoFocus type="email" autoComplete="email"
                value={email} onChange={(event) => setEmail(event.target.value)} />
            </label>
            {error && <div className="login-error" role="alert">{error}</div>}
            <button type="submit" disabled={submitting}>
              {submitting ? 'Enviando…' : 'Enviar instruções'}
            </button>
          </form>
        ) : (
          <div className="credential-success">
            <p>{message}</p>
            {developmentToken && (
              <button type="button" onClick={() => onDevelopmentToken(developmentToken)}>
                Continuar redefinição local
              </button>
            )}
          </div>
        )}
      </section>
    </div>
  )
}

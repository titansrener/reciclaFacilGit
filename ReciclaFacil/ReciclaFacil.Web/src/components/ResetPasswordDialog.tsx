import { useState } from 'react'
import type { FormEvent } from 'react'
import { resetPassword } from '../services/auth'

interface Props {
  token: string
  onClose: () => void
  onReset: () => void
}

export function ResetPasswordDialog({ token, onClose, onReset }: Props) {
  const [password, setPassword] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      await resetPassword(token, password, confirmation)
      onReset()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível redefinir a senha.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="login-modal" role="dialog" aria-modal="true"
        aria-labelledby="reset-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Nova credencial</span>
        <h2 id="reset-title">Redefinir senha</h2>
        <p>Crie uma senha com pelo menos oito caracteres.</p>
        <form className="login-form" onSubmit={submit}>
          <label>
            <span>Nova senha</span>
            <input required autoFocus minLength={8} maxLength={100}
              type="password" autoComplete="new-password"
              value={password} onChange={(event) => setPassword(event.target.value)} />
          </label>
          <label>
            <span>Confirmar senha</span>
            <input required minLength={8} maxLength={100}
              type="password" autoComplete="new-password"
              value={confirmation} onChange={(event) => setConfirmation(event.target.value)} />
          </label>
          {error && <div className="login-error" role="alert">{error}</div>}
          <button type="submit" disabled={submitting}>
            {submitting ? 'Redefinindo…' : 'Redefinir senha'}
          </button>
        </form>
      </section>
    </div>
  )
}

import { useState } from 'react'
import type { FormEvent } from 'react'
import { changePassword } from '../services/auth'

interface Props {
  onClose: () => void
  onChanged: () => void
}

export function ChangePasswordDialog({ onClose, onChanged }: Props) {
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      await changePassword(currentPassword, newPassword, confirmation)
      onChanged()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível alterar a senha.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="login-modal" role="dialog" aria-modal="true"
        aria-labelledby="change-password-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Segurança da conta</span>
        <h2 id="change-password-title">Alterar senha</h2>
        <p>As outras sessões serão encerradas depois da alteração.</p>
        <form className="login-form" onSubmit={submit}>
          <label>
            <span>Senha atual</span>
            <input required autoFocus type="password" autoComplete="current-password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)} />
          </label>
          <label>
            <span>Nova senha</span>
            <input required minLength={8} maxLength={100}
              type="password" autoComplete="new-password"
              value={newPassword} onChange={(event) => setNewPassword(event.target.value)} />
          </label>
          <label>
            <span>Confirmar nova senha</span>
            <input required minLength={8} maxLength={100}
              type="password" autoComplete="new-password"
              value={confirmation} onChange={(event) => setConfirmation(event.target.value)} />
          </label>
          {error && <div className="login-error" role="alert">{error}</div>}
          <button type="submit" disabled={submitting}>
            {submitting ? 'Alterando…' : 'Alterar senha'}
          </button>
        </form>
      </section>
    </div>
  )
}

import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import type { WebSession } from '../services/auth'

interface LoginDialogProps {
  onClose: () => void
  onLogin: (email: string, password: string) => Promise<WebSession>
}

export function LoginDialog({ onClose, onLogin }: LoginDialogProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

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
    try {
      await onLogin(email, password)
      onClose()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível entrar.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section
        className="login-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="login-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Área do usuário</span>
        <h2 id="login-title">Que bom ter você de volta.</h2>
        <p>Acesse sua conta para acompanhar coletas e atividades.</p>
        <form className="login-form" onSubmit={submit}>
          <label>
            <span>E-mail</span>
            <input
              autoComplete="username"
              autoFocus
              type="email"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="voce@exemplo.com"
            />
          </label>
          <label>
            <span>Senha</span>
            <input
              autoComplete="current-password"
              type="password"
              required
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Sua senha"
            />
          </label>
          {error && <div className="login-error" role="alert">{error}</div>}
          <button type="submit" disabled={submitting}>
            {submitting ? 'Entrando…' : 'Entrar'}
          </button>
        </form>
        <small>O refresh token é protegido por cookie HttpOnly e não fica disponível ao JavaScript.</small>
      </section>
    </div>
  )
}

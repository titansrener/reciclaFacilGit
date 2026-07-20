import { useEffect, useState } from 'react'
import {
  decideCollectionOffer,
  getClientNotifications,
  readNotification,
} from '../services/clients'
import type { ClientNotificationPage } from '../services/clients'
import { HistoryPagination } from './WalletDialog'

interface Props {
  onClose: () => void
  onChanged: () => void
}

const dateTime = (value: string | null) => value
  ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
  : 'Data não informada'
const money = (value: number) =>
  value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

export function NotificationsDialog({ onClose, onChanged }: Props) {
  const [page, setPage] = useState(1)
  const [data, setData] = useState<ClientNotificationPage | null>(null)
  const [version, setVersion] = useState(0)
  const [busy, setBusy] = useState<number | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    setData(null)
    getClientNotifications(page)
      .then(setData)
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar as notificações.',
      ))
  }, [page, version])

  async function update(id: number, operation: () => Promise<unknown>) {
    setBusy(id)
    setError('')
    try {
      await operation()
      setVersion((current) => current + 1)
      onChanged()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível atualizar a notificação.')
    } finally {
      setBusy(null)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section className="operation-modal history-modal" role="dialog" aria-modal="true"
        aria-labelledby="notifications-dialog-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Histórico</span>
        <h2 id="notifications-dialog-title">Todas as notificações</h2>
        {error && <div className="alert" role="alert">{error}</div>}
        {!data && !error && <p>Carregando notificações…</p>}
        {data && (
          <>
            {data.items.length === 0 ? <p className="panel-empty">Nenhuma notificação encontrada.</p> : (
              <ul className="history-list notification-history-list">
                {data.items.map((item) => <li key={item.id}>
                  <span className={item.active ? 'notification-active' : ''} aria-hidden="true" />
                  <div>
                    <strong>{item.description}</strong>
                    <small>{dateTime(item.createdAt)} · {item.active ? 'Não lida' : 'Lida'}</small>
                    {item.requiresValueDecision && <div className="notification-offer">
                      <span>Oferta: {money(item.offeredValue ?? 0)}</span>
                      <div>
                        <button disabled={busy === item.id}
                          onClick={() => update(item.id,
                            () => decideCollectionOffer(item.collectionId, 'accept'))}>Aceitar</button>
                        <button disabled={busy === item.id}
                          onClick={() => update(item.id,
                            () => decideCollectionOffer(item.collectionId, 'reject'))}>Recusar</button>
                      </div>
                    </div>}
                    {item.active && !item.requiresValueDecision &&
                      <button className="notification-read" disabled={busy === item.id}
                        onClick={() => update(item.id, () => readNotification(item.id))}>
                        Marcar como lida
                      </button>}
                  </div>
                </li>)}
              </ul>
            )}
            <HistoryPagination page={data.page} pageSize={data.pageSize}
              total={data.total} onPage={setPage} />
          </>
        )}
      </section>
    </div>
  )
}

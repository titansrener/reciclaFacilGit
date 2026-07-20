import { useEffect, useState } from 'react'
import { decideCollectionOffer, getClientOverview, readNotification } from '../services/clients'
import type { ClientOverview } from '../services/clients'
import { CollectionDetailsDialog } from './CollectionDetailsDialog'
import { ScheduleCollectionDialog } from './ScheduleCollectionDialog'
import { EditCollectionDialog } from './EditCollectionDialog'

const statusLabels: Record<string, string> = {
  A: 'Agendada',
  C: 'Concluída',
  F: 'Finalizada',
  I: 'Em andamento',
  N: 'Não coletada',
  P: 'Aguardando sua decisão',
  S: 'Concluída',
  X: 'Cancelada',
}

function formatDate(value: string | null) {
  if (!value) return 'Data não informada'
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function ClientDashboard() {
  const [overview, setOverview] = useState<ClientOverview | null>(null)
  const [error, setError] = useState('')
  const [version, setVersion] = useState(0)
  const [scheduling, setScheduling] = useState(false)
  const [selectedCollection, setSelectedCollection] = useState<number | null>(null)
  const [editingCollection, setEditingCollection] = useState<number | null>(null)
  const [busyNotification, setBusyNotification] = useState<number | null>(null)

  useEffect(() => {
    let active = true
    getClientOverview()
      .then((result) => {
        if (active) setOverview(result)
      })
      .catch((reason) => {
        if (active) setError(reason instanceof Error ? reason.message : 'Não foi possível carregar o painel.')
      })
    return () => { active = false }
  }, [version])

  function refresh() {
    setOverview(null)
    setVersion((current) => current + 1)
  }

  async function decide(notificationId: number, collectionId: number, decision: 'accept' | 'reject') {
    setBusyNotification(notificationId)
    setError('')
    try {
      await decideCollectionOffer(collectionId, decision)
      refresh()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível responder à oferta.')
    } finally {
      setBusyNotification(null)
    }
  }

  async function markRead(notificationId: number) {
    setBusyNotification(notificationId)
    try {
      await readNotification(notificationId)
      refresh()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível atualizar a notificação.')
    } finally {
      setBusyNotification(null)
    }
  }

  if (error) {
    return <main className="client-dashboard"><div className="alert" role="alert">{error}</div></main>
  }

  if (!overview) {
    return (
      <main className="client-dashboard" aria-busy="true">
        <div className="dashboard-loading">Carregando seu painel…</div>
      </main>
    )
  }

  return (
    <main className="client-dashboard">
      <section className="dashboard-welcome">
        <div>
          <span className="eyebrow">Painel do cliente</span>
          <h1>Olá, {overview.name.split(' ')[0]}.</h1>
          <p>Sua cooperativa de referência é <strong>{overview.cooperativeName}</strong>.</p>
        </div>
        <div className="impact-badge" aria-label="Conta conectada">
          <span aria-hidden="true">♻</span>
          <small>Conta conectada</small>
        </div>
      </section>

      <section className="dashboard-metrics" aria-label="Resumo da conta">
        <article>
          <span>Saldo da carteira</span>
          <strong>{overview.walletBalance.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</strong>
          <small>Movimentações acumuladas</small>
        </article>
        <article>
          <span>Coletas</span>
          <strong>{overview.collections.length}</strong>
          <small>Agendamentos no histórico</small>
        </article>
        <article>
          <span>Notificações</span>
          <strong>{overview.activeNotificationCount}</strong>
          <small>Itens que precisam de atenção</small>
        </article>
      </section>

      <div className="dashboard-columns">
        <section className="dashboard-panel" aria-labelledby="collections-title">
          <div className="panel-heading">
            <div>
              <span className="eyebrow">Histórico</span>
              <h2 id="collections-title">Minhas coletas</h2>
            </div>
            <button className="panel-action" type="button" onClick={() => setScheduling(true)}>
              Agendar coleta
            </button>
          </div>
          {overview.collections.length === 0 ? (
            <div className="panel-empty">Você ainda não possui coletas agendadas.</div>
          ) : (
            <ul className="collection-list">
              {overview.collections.map((collection) => (
                <li key={collection.id}>
                  <span className={`status-dot status-${collection.status.toLowerCase()}`} />
                  <div>
                    <strong>Coleta #{collection.id}</strong>
                    <small>{formatDate(collection.scheduledAt)}</small>
                  </div>
                  <div className="collection-meta">
                    <span>{statusLabels[collection.status] ?? collection.status}</span>
                    <small>{collection.materialCount} materiais</small>
                  </div>
                  <button className="collection-action" type="button"
                    onClick={() => setSelectedCollection(collection.id)}>
                    Ver detalhes
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="dashboard-panel notifications-panel" aria-labelledby="notifications-title">
          <div className="panel-heading">
            <div>
              <span className="eyebrow">Atualizações</span>
              <h2 id="notifications-title">Notificações</h2>
            </div>
          </div>
          {overview.recentNotifications.length === 0 ? (
            <div className="panel-empty">Nenhuma notificação por enquanto.</div>
          ) : (
            <ul className="notification-list">
              {overview.recentNotifications.map((notification) => (
                <li key={notification.id}>
                  <span className={notification.active ? 'notification-active' : ''} aria-hidden="true" />
                  <div>
                    <strong>{notification.description}</strong>
                    <small>{formatDate(notification.createdAt)}</small>
                    {notification.requiresValueDecision && (
                      <div className="notification-offer">
                        <span>
                          Oferta: {(notification.offeredValue ?? 0).toLocaleString(
                            'pt-BR', { style: 'currency', currency: 'BRL' })}
                        </span>
                        <div>
                          <button disabled={busyNotification === notification.id}
                            onClick={() => decide(notification.id, notification.collectionId, 'accept')}>
                            Aceitar
                          </button>
                          <button disabled={busyNotification === notification.id}
                            onClick={() => decide(notification.id, notification.collectionId, 'reject')}>
                            Recusar
                          </button>
                        </div>
                      </div>
                    )}
                    {notification.active && !notification.requiresValueDecision && (
                      <button className="notification-read" disabled={busyNotification === notification.id}
                        onClick={() => markRead(notification.id)}>Marcar como lida</button>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
      {scheduling && (
        <ScheduleCollectionDialog onClose={() => setScheduling(false)} onScheduled={refresh} />
      )}
      {selectedCollection !== null && (
        <CollectionDetailsDialog collectionId={selectedCollection}
          onClose={() => setSelectedCollection(null)}
          onChanged={refresh}
          onEdit={() => {
            setEditingCollection(selectedCollection)
            setSelectedCollection(null)
          }} />
      )}
      {editingCollection !== null && (
        <EditCollectionDialog
          collectionId={editingCollection}
          onClose={() => setEditingCollection(null)}
          onUpdated={refresh}
        />
      )}
    </main>
  )
}

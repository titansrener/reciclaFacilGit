import { useEffect, useState } from 'react'
import {
  getEmployeeClient,
  getEmployeeCollection,
  getEmployeeOverview,
  recordMaterials,
} from '../services/employees'
import type {
  EmployeeClientDetails,
  EmployeeCollectionDetails,
  EmployeeOverview,
} from '../services/employees'
import { CollectionRouteDialog } from './CollectionRouteDialog'

const statusLabel: Record<string, string> = {
  A: 'Agendada', I: 'Em andamento', F: 'Finalizada', S: 'Coletado', N: 'Não coletado',
}
const formatDate = (value: string | null) => value
  ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
  : 'A definir'

export function EmployeeDashboard() {
  const [overview, setOverview] = useState<EmployeeOverview | null>(null)
  const [collection, setCollection] = useState<EmployeeCollectionDetails | null>(null)
  const [client, setClient] = useState<EmployeeClientDetails | null>(null)
  const [routeCollectionId, setRouteCollectionId] = useState<number | null>(null)
  const [quantities, setQuantities] = useState<Record<number, string>>({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function refresh() { setOverview(await getEmployeeOverview()) }
  useEffect(() => { refresh().catch(showError) }, [])
  function showError(reason: unknown) {
    setError(reason instanceof Error ? reason.message : 'Não foi possível carregar os dados.')
  }

  async function openCollection(id: number) {
    setError('')
    try { setCollection(await getEmployeeCollection(id)) } catch (reason) { showError(reason) }
  }

  async function openClient(collectionId: number, clientId: string) {
    setError('')
    try {
      const details = await getEmployeeClient(collectionId, clientId)
      setClient(details)
      setQuantities(Object.fromEntries(
        details.materials.map((material) => [material.id, material.quantity?.toString() ?? '0']),
      ))
    } catch (reason) { showError(reason) }
  }

  async function submitMaterials() {
    if (!client) return
    setBusy(true)
    setError('')
    try {
      await recordMaterials(client.collectionId, client.id,
        client.materials.map((material) => ({
          materialId: material.id,
          quantity: Number(quantities[material.id] ?? 0),
        })))
      const refreshedCollection = await getEmployeeCollection(client.collectionId)
      setCollection(refreshedCollection)
      setClient(null)
      await refresh()
    } catch (reason) { showError(reason) } finally { setBusy(false) }
  }

  if (!overview) return <main className="client-dashboard dashboard-loading">Carregando suas coletas…</main>
  const pending = overview.collections.reduce((sum, item) => sum + item.pendingClients, 0)
  const completed = overview.collections.reduce((sum, item) => sum + item.completedClients, 0)

  return (
    <main className="client-dashboard employee-dashboard">
      <section className="dashboard-welcome">
        <div><span className="eyebrow">Operação de coleta</span><h1>Olá, {overview.name}</h1>
          <p>Equipe da <strong>{overview.cooperativeName}</strong></p></div>
        <div className="impact-badge"><strong>{overview.collections.length}</strong><small>coletas</small></div>
      </section>
      {error && <div className="alert" role="alert">{error}</div>}
      <section className="dashboard-metrics">
        <article><span>Atribuídas</span><strong>{overview.collections.length}</strong><small>Total na agenda</small></article>
        <article><span>Pendentes</span><strong>{pending}</strong><small>Clientes aguardando</small></article>
        <article><span>Realizadas</span><strong>{completed}</strong><small>Clientes coletados</small></article>
      </section>
      <section className="dashboard-panel">
        <div className="panel-heading"><div><span className="eyebrow">Minha agenda</span><h2>Coletas atribuídas</h2></div></div>
        {overview.collections.length === 0 ? <p className="panel-empty">Nenhuma coleta atribuída.</p> : (
          <ul className="collection-list">
            {overview.collections.map((item) => <li key={item.id}>
              <span className={`status-dot status-${item.status.toLowerCase()}`} />
              <div><strong>{formatDate(item.scheduledAt)}</strong>
                <small>{item.pendingClients} pendente(s) · {item.completedClients} realizada(s)</small></div>
              <div className="collection-meta"><span>{statusLabel[item.status]}</span></div>
              <button className="collection-action" onClick={() => openCollection(item.id)}>Abrir</button>
            </li>)}
          </ul>
        )}
      </section>

      {collection && (
        <div className="modal-backdrop" onMouseDown={() => setCollection(null)}>
          <section className="operation-modal employee-collection-modal" role="dialog" aria-modal="true"
            onMouseDown={(event) => event.stopPropagation()}>
            <button className="modal-close" onClick={() => setCollection(null)} aria-label="Fechar">×</button>
            <span className="eyebrow">{statusLabel[collection.status]}</span>
            <h2>Coleta de {formatDate(collection.scheduledAt)}</h2>
            <button className="route-open-button"
              disabled={!collection.clients.some((item) => item.status === 'A')}
              onClick={() => setRouteCollectionId(collection.id)}>
              Ver roteiro
            </button>
            <h3>Caminhões</h3>
            <p>{collection.trucks.map((truck) => `${truck.plate} · ${truck.description}`).join(', ') || 'Nenhum caminhão associado.'}</p>
            <h3>Clientes</h3>
            <ul className="employee-client-list">
              {collection.clients.map((item) => <li key={item.id}>
                <div><strong>{item.name}</strong><small>{item.address} · {item.mobile}</small></div>
                <span>{statusLabel[item.status ?? 'A']}</span>
                <button disabled={collection.status !== 'I' || item.status !== 'A'}
                  onClick={() => openClient(collection.id, item.id)}>
                  {item.status === 'A' ? 'Registrar' : 'Visualizar'}
                </button>
              </li>)}
            </ul>
          </section>
        </div>
      )}

      {client && (
        <div className="modal-backdrop nested-modal" onMouseDown={() => setClient(null)}>
          <section className="operation-modal" role="dialog" aria-modal="true"
            onMouseDown={(event) => event.stopPropagation()}>
            <button className="modal-close" onClick={() => setClient(null)} aria-label="Fechar">×</button>
            <span className="eyebrow">{client.type === 'V' ? 'Vendedor' : 'Doador'}</span>
            <h2>{client.name}</h2>
            <p>{client.address} · {client.mobile}</p>
            <h3>Materiais recebidos</h3>
            <div className="material-quantity-list">
              {client.materials.map((material) => <label key={material.id}>
                <span>{material.description}</span>
                <div><input type="number" min="0" step="0.01"
                  value={quantities[material.id] ?? '0'}
                  onChange={(event) => setQuantities({
                    ...quantities, [material.id]: event.target.value,
                  })} /><small>kg</small></div>
              </label>)}
            </div>
            <button className="operation-primary" disabled={busy} onClick={submitMaterials}>
              {busy ? 'Salvando…' : 'Concluir atendimento'}
            </button>
          </section>
        </div>
      )}
      {routeCollectionId !== null && (
        <CollectionRouteDialog collectionId={routeCollectionId}
          onClose={() => setRouteCollectionId(null)} />
      )}
    </main>
  )
}

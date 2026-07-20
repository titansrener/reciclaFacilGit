import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  addManagedMaterial,
  collectionAction,
  createEmployee,
  createCollection,
  createTruck,
  deleteEmployee,
  deleteTruck,
  getCooperativeOverview,
  getCooperativeResources,
  getManagedCollection,
  removeManagedMaterial,
  setCollectionResource,
  updateCollection,
  updateEmployee,
  updateManagedMaterial,
  updateTruck,
} from '../services/cooperativeManagement'
import type {
  CooperativeCollectionDetails,
  CooperativeCollection,
  CooperativeOverview,
  CooperativeResources,
  Employee,
  Truck,
} from '../services/cooperativeManagement'

const statusLabel: Record<string, string> = { A: 'Agendada', I: 'Em andamento', F: 'Finalizada' }
const money = (value: number | null) => value == null
  ? 'Não informado'
  : value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const dateTime = (value: string | null) => value
  ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
  : 'A definir'
const dateTimeInput = (value: string | null) => {
  if (!value) return ''
  const date = new Date(value)
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}

export function CooperativeDashboard() {
  const [overview, setOverview] = useState<CooperativeOverview | null>(null)
  const [details, setDetails] = useState<CooperativeCollectionDetails | null>(null)
  const [resources, setResources] = useState<CooperativeResources | null>(null)
  const [scheduledAt, setScheduledAt] = useState('')
  const [editingCollection, setEditingCollection] = useState<CooperativeCollection | null>(null)
  const [materialId, setMaterialId] = useState('')
  const [resalePrice, setResalePrice] = useState('')
  const [truckDescription, setTruckDescription] = useState('')
  const [truckPlate, setTruckPlate] = useState('')
  const [editingTruck, setEditingTruck] = useState<Truck | null>(null)
  const [employeeName, setEmployeeName] = useState('')
  const [employeeBirthDate, setEmployeeBirthDate] = useState('')
  const [employeeEmail, setEmployeeEmail] = useState('')
  const [employeePassword, setEmployeePassword] = useState('')
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function refresh() {
    const [nextOverview, nextResources] = await Promise.all([
      getCooperativeOverview(), getCooperativeResources(),
    ])
    setOverview(nextOverview)
    setResources(nextResources)
  }

  useEffect(() => { refresh().catch(showError) }, [])

  function showError(reason: unknown) {
    setError(reason instanceof Error ? reason.message : 'Não foi possível carregar os dados.')
  }

  async function run(operation: () => Promise<void>) {
    setBusy(true)
    setError('')
    try {
      await operation()
      await refresh()
    } catch (reason) {
      showError(reason)
    } finally {
      setBusy(false)
    }
  }

  function submitCollection(event: FormEvent) {
    event.preventDefault()
    const value = new FormData(event.currentTarget as HTMLFormElement)
      .get('scheduledAt')?.toString() ?? ''
    run(async () => {
      if (editingCollection) {
        await updateCollection(editingCollection.id, value)
      } else {
        await createCollection(value)
      }
      setScheduledAt('')
      setEditingCollection(null)
    })
  }

  function editCollection(collection: CooperativeCollection) {
    setEditingCollection(collection)
    setScheduledAt(dateTimeInput(collection.scheduledAt))
  }

  function cancelCollectionEdit() {
    setEditingCollection(null)
    setScheduledAt('')
  }

  function submitMaterial(event: FormEvent) {
    event.preventDefault()
    run(async () => {
      await addManagedMaterial(Number(materialId), resalePrice === '' ? null : Number(resalePrice))
      setMaterialId('')
      setResalePrice('')
    })
  }

  function submitTruck(event: FormEvent) {
    event.preventDefault()
    run(async () => {
      if (editingTruck) await updateTruck(editingTruck.id, truckDescription, truckPlate)
      else await createTruck(truckDescription, truckPlate)
      setTruckDescription('')
      setTruckPlate('')
      setEditingTruck(null)
    })
  }

  function submitEmployee(event: FormEvent) {
    event.preventDefault()
    run(async () => {
      if (editingEmployee) {
        await updateEmployee(editingEmployee.id, employeeName, employeeBirthDate)
      } else {
        await createEmployee(employeeName, employeeBirthDate, employeeEmail, employeePassword)
      }
      setEmployeeName('')
      setEmployeeBirthDate('')
      setEmployeeEmail('')
      setEmployeePassword('')
      setEditingEmployee(null)
    })
  }

  function editTruck(truck: Truck) {
    setEditingTruck(truck)
    setTruckDescription(truck.description)
    setTruckPlate(truck.plate)
  }

  function cancelTruckEdit() {
    setEditingTruck(null)
    setTruckDescription('')
    setTruckPlate('')
  }

  function editEmployee(employee: Employee) {
    setEditingEmployee(employee)
    setEmployeeName(employee.name)
    setEmployeeBirthDate(employee.birthDate.slice(0, 10))
    setEmployeeEmail('')
    setEmployeePassword('')
  }

  function cancelEmployeeEdit() {
    setEditingEmployee(null)
    setEmployeeName('')
    setEmployeeBirthDate('')
    setEmployeeEmail('')
    setEmployeePassword('')
  }

  async function openDetails(id: number) {
    setError('')
    try { setDetails(await getManagedCollection(id)) } catch (reason) { showError(reason) }
  }

  if (!overview) return <main className="client-dashboard dashboard-loading">Carregando painel da cooperativa…</main>

  return (
    <main className="client-dashboard cooperative-dashboard">
      <section className="dashboard-welcome">
        <div>
          <span className="eyebrow">Gestão da cooperativa</span>
          <h1>{overview.name}</h1>
          <p>Organize coletas e materiais em uma API pronta para web e aplicativos.</p>
        </div>
        <div className="impact-badge"><strong>{overview.registeredClients}</strong><small>clientes</small></div>
      </section>

      {error && <div className="alert" role="alert">{error}</div>}
      <section className="dashboard-metrics">
        <article><span>Agendadas</span><strong>{overview.scheduledCollections}</strong><small>Aguardando início</small></article>
        <article><span>Em andamento</span><strong>{overview.inProgressCollections}</strong><small>Coletas na rua</small></article>
        <article><span>Finalizadas</span><strong>{overview.finishedCollections}</strong><small>Histórico concluído</small></article>
      </section>

      <div className="dashboard-columns">
        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">Operação</span><h2>Agenda de coletas</h2></div></div>
          <form className="management-form" onSubmit={submitCollection}>
            <label><span>{editingCollection ? 'Nova data e hora' : 'Data e hora'}</span>
              <input name="scheduledAt" type="datetime-local" required value={scheduledAt}
              onChange={(event) => setScheduledAt(event.target.value)} /></label>
            <button disabled={busy}>{editingCollection ? 'Salvar horário' : 'Criar horário'}</button>
            {editingCollection && <button className="secondary-form-action" type="button"
              disabled={busy} onClick={cancelCollectionEdit}>Cancelar</button>}
          </form>
          {overview.collections.length === 0 ? <p className="panel-empty">Nenhuma coleta cadastrada.</p> : (
            <ul className="collection-list">
              {overview.collections.map((collection) => (
                <li key={collection.id}>
                  <span className={`status-dot status-${collection.status.toLowerCase()}`} />
                  <div><strong>{dateTime(collection.scheduledAt)}</strong><small>{collection.clientCount} cliente(s)</small></div>
                  <div className="collection-meta"><span>{statusLabel[collection.status]}</span></div>
                  <div className="row-actions">
                    <button onClick={() => openDetails(collection.id)}>Detalhes</button>
                    {collection.status === 'A' && <button disabled={busy}
                      onClick={() => editCollection(collection)}>Editar</button>}
                    {collection.status === 'A' && <button disabled={busy}
                      onClick={() => run(() => collectionAction(collection.id, 'start'))}>Iniciar</button>}
                    {collection.status === 'A' && collection.clientCount === 0 && <button disabled={busy}
                      onClick={() => run(() => collectionAction(collection.id, 'delete'))}>Excluir</button>}
                    {collection.status === 'I' && <button disabled={busy}
                      onClick={() => run(() => collectionAction(collection.id, 'finish'))}>Finalizar</button>}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">Catálogo</span><h2>Materiais</h2></div></div>
          <form className="management-form material-form" onSubmit={submitMaterial}>
            <label><span>Material</span><select required value={materialId}
              onChange={(event) => setMaterialId(event.target.value)}>
              <option value="">Selecione</option>
              {overview.materialOptions.map((material) =>
                <option key={material.id} value={material.id}>{material.description}</option>)}
            </select></label>
            <label><span>Preço de revenda</span><input type="number" min="0" step="0.01" value={resalePrice}
              onChange={(event) => setResalePrice(event.target.value)} placeholder="Opcional" /></label>
            <button disabled={busy || !materialId}>Associar</button>
          </form>
          <ul className="managed-materials">
            {overview.materials.map((material) => (
              <li key={material.id}>
                <div><strong>{material.description}</strong><small>{money(material.resalePrice)}</small></div>
                <div className="row-actions">
                  <button disabled={busy} onClick={() => {
                    const value = window.prompt('Novo preço de revenda (deixe vazio para remover):',
                      material.resalePrice?.toString() ?? '')
                    if (value !== null) run(() => updateManagedMaterial(material.id, value === '' ? null : Number(value)))
                  }}>Preço</button>
                  <button disabled={busy} onClick={() => run(() => removeManagedMaterial(material.id))}>Remover</button>
                </div>
              </li>
            ))}
          </ul>
        </section>
      </div>

      <div className="resource-columns">
        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">Frota</span><h2>Caminhões</h2></div></div>
          <form className="management-form" onSubmit={submitTruck}>
            <label><span>Descrição</span><input required maxLength={45} value={truckDescription}
              onChange={(event) => setTruckDescription(event.target.value)} placeholder="Ex.: Caminhão leve" /></label>
            <label><span>Placa</span><input required maxLength={8} value={truckPlate}
              onChange={(event) => setTruckPlate(event.target.value.toUpperCase())} placeholder="ABC1D23" /></label>
            <button disabled={busy}>{editingTruck ? 'Salvar' : 'Cadastrar'}</button>
            {editingTruck && <button className="secondary-form-action" type="button"
              disabled={busy} onClick={cancelTruckEdit}>Cancelar</button>}
          </form>
          <ul className="managed-materials">
            {resources?.trucks.map((truck) => <li key={truck.id}>
              <div><strong>{truck.plate}</strong><small>{truck.description}</small></div>
              <div className="row-actions"><button disabled={busy}
                onClick={() => editTruck(truck)}>Editar</button><button disabled={busy}
                onClick={() => run(() => deleteTruck(truck.id))}>Excluir</button></div>
            </li>)}
          </ul>
        </section>

        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">Equipe</span><h2>Funcionários</h2></div></div>
          <form className="management-form employee-form" onSubmit={submitEmployee}>
            <label><span>Nome</span><input required maxLength={45} value={employeeName}
              onChange={(event) => setEmployeeName(event.target.value)} /></label>
            <label><span>Nascimento</span><input required type="date" value={employeeBirthDate}
              onChange={(event) => setEmployeeBirthDate(event.target.value)} /></label>
            {editingEmployee ? (
              <p className="editing-resource-email">Conta: <strong>{editingEmployee.email}</strong></p>
            ) : (
              <>
                <label><span>E-mail</span><input required type="email" value={employeeEmail}
                  onChange={(event) => setEmployeeEmail(event.target.value)} /></label>
                <label><span>Senha inicial</span><input required type="password" minLength={8} value={employeePassword}
                  onChange={(event) => setEmployeePassword(event.target.value)} /></label>
              </>
            )}
            <button disabled={busy}>{editingEmployee ? 'Salvar' : 'Cadastrar'}</button>
            {editingEmployee && <button className="secondary-form-action" type="button"
              disabled={busy} onClick={cancelEmployeeEdit}>Cancelar</button>}
          </form>
          <ul className="managed-materials">
            {resources?.employees.map((employee) => <li key={employee.id}>
              <div><strong>{employee.name}</strong><small>{employee.email}</small></div>
              <div className="row-actions"><button disabled={busy}
                onClick={() => editEmployee(employee)}>Editar</button><button disabled={busy}
                onClick={() => run(() => deleteEmployee(employee.id))}>Excluir</button></div>
            </li>)}
          </ul>
        </section>
      </div>

      {details && (
        <div className="modal-backdrop" onMouseDown={() => setDetails(null)}>
          <section className="operation-modal" role="dialog" aria-modal="true"
            onMouseDown={(event) => event.stopPropagation()}>
            <button className="modal-close" onClick={() => setDetails(null)} aria-label="Fechar">×</button>
            <span className="eyebrow">{statusLabel[details.status]}</span>
            <h2>Coleta de {dateTime(details.scheduledAt)}</h2>
            <h3>Clientes agendados</h3>
            {details.clients.length === 0 ? <p className="operation-empty">Nenhum cliente agendado.</p> : (
              <ul className="managed-materials">
                {details.clients.map((client) => <li className="collection-client-details" key={client.id}>
                  <div>
                    <strong>{client.name}</strong>
                    <small>{client.type === 'V' ? 'Vendedor' : 'Doador'} · {client.address}</small>
                    <small>{client.email} · {client.mobile}
                      {client.phone ? ` · ${client.phone}` : ''}</small>
                    <small>Materiais: {client.materials.join(', ') || 'nenhum'}</small>
                  </div>
                </li>)}
              </ul>
            )}
            <h3>Frota</h3>
            <div className="assignment-grid">
              {resources?.trucks.map((truck) => {
                const assigned = details.trucks.some((item) => item.id === truck.id)
                return <label className="choice-row" key={truck.id}>
                  <input type="checkbox" checked={assigned} disabled={busy}
                    onChange={() => run(async () => {
                      await setCollectionResource(details.id, 'trucks', truck.id, !assigned)
                      setDetails(await getManagedCollection(details.id))
                    })} />
                  <span>{truck.plate} · {truck.description}</span>
                </label>
              })}
            </div>
            <h3>Equipe</h3>
            <div className="assignment-grid">
              {resources?.employees.map((employee) => {
                const assigned = details.employees.some((item) => item.id === employee.id)
                return <label className="choice-row" key={employee.id}>
                  <input type="checkbox" checked={assigned} disabled={busy}
                    onChange={() => run(async () => {
                      await setCollectionResource(details.id, 'employees', employee.id, !assigned)
                      setDetails(await getManagedCollection(details.id))
                    })} />
                  <span>{employee.name}</span>
                </label>
              })}
            </div>
          </section>
        </div>
      )}
    </main>
  )
}

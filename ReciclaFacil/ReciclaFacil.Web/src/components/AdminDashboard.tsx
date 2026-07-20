import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  createAdminMaterial,
  deleteAdminMaterial,
  listAdminMaterials,
  updateAdminMaterial,
} from '../services/administration'
import type { AdminMaterial } from '../services/administration'

export function AdminDashboard() {
  const [materials, setMaterials] = useState<AdminMaterial[]>([])
  const [description, setDescription] = useState('')
  const [time, setTime] = useState('')
  const [editing, setEditing] = useState<AdminMaterial | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function refresh() { setMaterials(await listAdminMaterials()) }
  useEffect(() => { refresh().catch(showError) }, [])
  function showError(reason: unknown) {
    setError(reason instanceof Error ? reason.message : 'Não foi possível carregar o catálogo.')
  }
  function resetForm() {
    setDescription('')
    setTime('')
    setEditing(null)
  }
  async function run(operation: () => Promise<void>) {
    setBusy(true)
    setError('')
    try {
      await operation()
      resetForm()
      await refresh()
    } catch (reason) { showError(reason) } finally { setBusy(false) }
  }
  function submit(event: FormEvent) {
    event.preventDefault()
    const averageTime = Number(time)
    run(() => editing
      ? updateAdminMaterial(editing.id, description, averageTime)
      : createAdminMaterial(description, averageTime))
  }
  function beginEdit(material: AdminMaterial) {
    setEditing(material)
    setDescription(material.description)
    setTime(material.averageDecompositionTime.toString())
  }

  return (
    <main className="client-dashboard admin-dashboard">
      <section className="dashboard-welcome">
        <div><span className="eyebrow">Administração</span><h1>Catálogo de materiais</h1>
          <p>Mantenha a referência usada por cooperativas, clientes e funcionários.</p></div>
        <div className="impact-badge"><strong>{materials.length}</strong><small>materiais</small></div>
      </section>
      {error && <div className="alert" role="alert">{error}</div>}
      <div className="admin-layout">
        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">
            {editing ? 'Edição' : 'Novo registro'}</span>
            <h2>{editing ? `Material #${editing.id}` : 'Cadastrar material'}</h2></div></div>
          <form className="admin-material-form" onSubmit={submit}>
            <label><span>Descrição</span><input required minLength={2} maxLength={50}
              value={description} onChange={(event) => setDescription(event.target.value)}
              placeholder="Ex.: Papelão" /></label>
            <label><span>Tempo médio de decomposição</span>
              <div><input required type="number" min="0" max="1000000" value={time}
                onChange={(event) => setTime(event.target.value)} /><small>meses</small></div>
            </label>
            <div className="admin-form-actions">
              {editing && <button type="button" onClick={resetForm}>Cancelar</button>}
              <button className="operation-primary" disabled={busy}>
                {busy ? 'Salvando…' : editing ? 'Salvar alterações' : 'Cadastrar'}
              </button>
            </div>
          </form>
        </section>
        <section className="dashboard-panel">
          <div className="panel-heading"><div><span className="eyebrow">Referência</span>
            <h2>Materiais cadastrados</h2></div></div>
          {materials.length === 0 ? <p className="panel-empty">Nenhum material cadastrado.</p> : (
            <ul className="managed-materials">
              {materials.map((material) => <li key={material.id}>
                <div><strong>{material.description}</strong>
                  <small>{material.averageDecompositionTime} meses para decomposição</small></div>
                <div className="row-actions">
                  <button disabled={busy} onClick={() => beginEdit(material)}>Editar</button>
                  <button disabled={busy} onClick={() => run(() => deleteAdminMaterial(material.id))}>
                    Excluir
                  </button>
                </div>
              </li>)}
            </ul>
          )}
        </section>
      </div>
    </main>
  )
}

import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import {
  getCollectionDetails,
  getCollectionOptions,
  updateCollection,
} from '../services/clients'
import type {
  ClientCollectionDetails,
  ClientCollectionOptions,
} from '../services/clients'

interface Props {
  collectionId: number
  onClose: () => void
  onUpdated: () => void
}

function formatSlot(value: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'full',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function EditCollectionDialog({ collectionId, onClose, onUpdated }: Props) {
  const [options, setOptions] = useState<ClientCollectionOptions | null>(null)
  const [details, setDetails] = useState<ClientCollectionDetails | null>(null)
  const [slotId, setSlotId] = useState(collectionId)
  const [materialIds, setMaterialIds] = useState<number[]>([])
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    Promise.all([
      getCollectionOptions(collectionId),
      getCollectionDetails(collectionId),
    ])
      .then(([available, current]) => {
        setOptions(available)
        setDetails(current)
        setSlotId(current.id)
        setMaterialIds(current.materials.map((material) => material.id))
      })
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar a coleta.',
      ))
  }, [collectionId])

  function toggleMaterial(id: number) {
    setMaterialIds((current) =>
      current.includes(id) ? current.filter((item) => item !== id) : [...current, id])
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!materialIds.length) {
      setError('Selecione pelo menos um material.')
      return
    }
    setSubmitting(true)
    setError('')
    try {
      await updateCollection(collectionId, slotId, materialIds)
      onUpdated()
      onClose()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível alterar a coleta.')
    } finally {
      setSubmitting(false)
    }
  }

  const currentAvailable = options?.slots.some((slot) => slot.id === collectionId) ?? false

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="operation-modal" role="dialog" aria-modal="true"
        aria-labelledby="edit-collection-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Coleta #{collectionId}</span>
        <h2 id="edit-collection-title">Editar coleta</h2>
        {!options && !details && !error ? <p>Carregando coleta…</p> : (
          <form className="schedule-form" onSubmit={submit}>
            <fieldset>
              <legend>Escolha o horário</legend>
              {options?.slots.length ? options.slots.map((slot) => (
                <label className="choice-row" key={slot.id}>
                  <input type="radio" name="edit-slot" value={slot.id}
                    checked={slotId === slot.id}
                    onChange={() => setSlotId(slot.id)} />
                  <span>{formatSlot(slot.scheduledAt)}</span>
                </label>
              )) : <div className="operation-empty">Nenhum horário disponível.</div>}
            </fieldset>
            <fieldset>
              <legend>Materiais para coleta</legend>
              {options?.materials.length ? options.materials.map((material) => (
                <label className="choice-row" key={material.id}>
                  <input type="checkbox" checked={materialIds.includes(material.id)}
                    onChange={() => toggleMaterial(material.id)} />
                  <span>{material.description}</span>
                </label>
              )) : <div className="operation-empty">A cooperativa não informou materiais.</div>}
            </fieldset>
            {!currentAvailable && options && (
              <div className="login-error" role="alert">
                Esta coleta não está mais disponível para edição.
              </div>
            )}
            {error && <div className="login-error" role="alert">{error}</div>}
            <button className="operation-primary" type="submit"
              disabled={submitting || !currentAvailable || !options?.materials.length}>
              {submitting ? 'Salvando…' : 'Salvar alterações'}
            </button>
          </form>
        )}
      </section>
    </div>
  )
}

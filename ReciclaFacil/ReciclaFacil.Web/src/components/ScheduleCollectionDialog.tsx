import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { getCollectionOptions, scheduleCollection } from '../services/clients'
import type { ClientCollectionOptions } from '../services/clients'

interface Props {
  onClose: () => void
  onScheduled: () => void
}

function formatSlot(value: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'full',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function ScheduleCollectionDialog({ onClose, onScheduled }: Props) {
  const [options, setOptions] = useState<ClientCollectionOptions | null>(null)
  const [slotId, setSlotId] = useState(0)
  const [materialIds, setMaterialIds] = useState<number[]>([])
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    getCollectionOptions()
      .then(setOptions)
      .catch((reason) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar as opções.'))
  }, [])

  function toggleMaterial(id: number) {
    setMaterialIds((current) =>
      current.includes(id) ? current.filter((item) => item !== id) : [...current, id])
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!slotId) {
      setError('Selecione um horário.')
      return
    }
    if (!materialIds.length) {
      setError('Selecione pelo menos um material.')
      return
    }
    setSubmitting(true)
    setError('')
    try {
      await scheduleCollection(slotId, materialIds)
      onScheduled()
      onClose()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível agendar.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="operation-modal" role="dialog" aria-modal="true"
        aria-labelledby="schedule-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Nova coleta</span>
        <h2 id="schedule-title">Agendar coleta</h2>
        {!options && !error ? <p>Carregando opções…</p> : (
          <form className="schedule-form" onSubmit={submit}>
            <fieldset>
              <legend>Escolha o horário</legend>
              {options?.slots.length ? options.slots.map((slot) => (
                <label className="choice-row" key={slot.id}>
                  <input type="radio" name="slot" value={slot.id}
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
            {error && <div className="login-error" role="alert">{error}</div>}
            <button className="operation-primary" type="submit"
              disabled={submitting || !options?.slots.length || !options?.materials.length}>
              {submitting ? 'Agendando…' : 'Confirmar agendamento'}
            </button>
          </form>
        )}
      </section>
    </div>
  )
}

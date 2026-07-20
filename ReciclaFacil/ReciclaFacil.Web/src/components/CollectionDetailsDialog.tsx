import { useEffect, useState } from 'react'
import {
  cancelCollection,
  getCollectionDetails,
} from '../services/clients'
import type { ClientCollectionDetails } from '../services/clients'

interface Props {
  collectionId: number
  onClose: () => void
  onChanged: () => void
}

function formatDate(value: string | null) {
  if (!value) return 'Não informada'
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'full',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function CollectionDetailsDialog({ collectionId, onClose, onChanged }: Props) {
  const [details, setDetails] = useState<ClientCollectionDetails | null>(null)
  const [error, setError] = useState('')
  const [confirming, setConfirming] = useState(false)
  const [cancelling, setCancelling] = useState(false)

  useEffect(() => {
    getCollectionDetails(collectionId)
      .then(setDetails)
      .catch((reason) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar a coleta.'))
  }, [collectionId])

  async function confirmCancellation() {
    setCancelling(true)
    setError('')
    try {
      await cancelCollection(collectionId)
      onChanged()
      onClose()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível cancelar.')
      setConfirming(false)
    } finally {
      setCancelling(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="operation-modal" role="dialog" aria-modal="true"
        aria-labelledby="collection-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" type="button" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Detalhes da coleta</span>
        <h2 id="collection-title">Coleta #{collectionId}</h2>
        {error && <div className="login-error" role="alert">{error}</div>}
        {!details && !error ? <p>Carregando detalhes…</p> : details && (
          <>
            <dl className="collection-details">
              <div><dt>Agendada para</dt><dd>{formatDate(details.scheduledAt)}</dd></div>
              <div><dt>Status</dt><dd>{
                details.status === 'A' ? 'Agendada' :
                details.status === 'P' ? 'Aguardando sua decisão' :
                details.status === 'S' ? 'Concluída' : details.status
              }</dd></div>
            </dl>
            <h3>Materiais</h3>
            <ul className="material-list">
              {details.materials.map((material) => (
                <li key={material.id}>
                  <span>{material.description}</span>
                  <strong>{material.quantity == null
                    ? 'Quantidade pendente'
                    : `${material.quantity} kg${material.value == null
                      ? '' : ` · ${material.value.toLocaleString(
                        'pt-BR', { style: 'currency', currency: 'BRL' })}`}`}
                  </strong>
                </li>
              ))}
            </ul>
            {details.status === 'A' && (
              confirming ? (
                <div className="cancel-confirmation">
                  <p>Tem certeza? O horário ficará disponível novamente.</p>
                  <div>
                    <button type="button" onClick={() => setConfirming(false)}>Voltar</button>
                    <button className="danger-button" type="button"
                      onClick={confirmCancellation} disabled={cancelling}>
                      {cancelling ? 'Cancelando…' : 'Confirmar cancelamento'}
                    </button>
                  </div>
                </div>
              ) : (
                <button className="cancel-button" type="button" onClick={() => setConfirming(true)}>
                  Cancelar coleta
                </button>
              )
            )}
          </>
        )}
      </section>
    </div>
  )
}

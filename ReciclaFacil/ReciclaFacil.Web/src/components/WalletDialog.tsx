import { useEffect, useState } from 'react'
import { getClientWallet } from '../services/clients'
import type { ClientWalletPage } from '../services/clients'

interface Props {
  onClose: () => void
}

const money = (value: number) =>
  value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
const dateTime = (value: string | null) => value
  ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
  : 'Data não informada'

export function WalletDialog({ onClose }: Props) {
  const [page, setPage] = useState(1)
  const [wallet, setWallet] = useState<ClientWalletPage | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    setWallet(null)
    getClientWallet(page)
      .then(setWallet)
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar o extrato.',
      ))
  }, [page])

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section className="operation-modal history-modal" role="dialog" aria-modal="true"
        aria-labelledby="wallet-dialog-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Carteira</span>
        <h2 id="wallet-dialog-title">Meu extrato</h2>
        {error && <div className="alert" role="alert">{error}</div>}
        {!wallet && !error && <p>Carregando movimentações…</p>}
        {wallet && (
          <>
            <div className="wallet-total"><span>Saldo atual</span><strong>{money(wallet.balance)}</strong></div>
            {wallet.items.length === 0 ? <p className="panel-empty">Nenhuma movimentação registrada.</p> : (
              <ul className="history-list">
                {wallet.items.map((entry) => <li key={entry.id}>
                  <div><strong>Crédito de coleta</strong><small>{dateTime(entry.createdAt)}</small></div>
                  <span>{money(entry.value)}</span>
                </li>)}
              </ul>
            )}
            <HistoryPagination page={wallet.page} pageSize={wallet.pageSize}
              total={wallet.total} onPage={setPage} />
          </>
        )}
      </section>
    </div>
  )
}

interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPage: (page: number) => void
}

export function HistoryPagination({ page, pageSize, total, onPage }: PaginationProps) {
  const pages = Math.max(1, Math.ceil(total / pageSize))
  if (pages === 1) return null
  return (
    <nav className="history-pagination" aria-label="Paginação">
      <button disabled={page === 1} onClick={() => onPage(page - 1)}>Anterior</button>
      <span>Página {page} de {pages}</span>
      <button disabled={page === pages} onClick={() => onPage(page + 1)}>Próxima</button>
    </nav>
  )
}

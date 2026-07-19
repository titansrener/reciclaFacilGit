import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'
import { getCooperative, searchCooperatives } from './services/cooperatives'
import type {
  CooperativeDetails,
  CooperativeListItem,
  CooperativeSearch,
} from './services/cooperatives'

const initialSearch: CooperativeSearch = {
  name: '',
  city: '',
  state: '',
  page: 1,
  pageSize: 6,
}

function App() {
  const [filters, setFilters] = useState(initialSearch)
  const [query, setQuery] = useState(initialSearch)
  const [cooperatives, setCooperatives] = useState<CooperativeListItem[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<CooperativeDetails | null>(null)
  const [detailsLoading, setDetailsLoading] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    setError('')

    searchCooperatives(query, controller.signal)
      .then((result) => {
        setCooperatives(result.items)
        setTotal(result.total)
      })
      .catch((reason: unknown) => {
        if (!controller.signal.aborted) {
          setError(reason instanceof Error ? reason.message : 'Não foi possível realizar a busca.')
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })

    return () => controller.abort()
  }, [query])

  function submitSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setQuery({ ...filters, page: 1 })
  }

  async function showDetails(id: string) {
    setDetailsLoading(true)
    setError('')
    try {
      setSelected(await getCooperative(id))
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível abrir a cooperativa.')
    } finally {
      setDetailsLoading(false)
    }
  }

  const totalPages = Math.max(1, Math.ceil(total / query.pageSize))

  return (
    <div className="app-shell">
      <header className="site-header">
        <a className="brand" href="/" aria-label="Recicla Fácil — início">
          <span className="brand-mark" aria-hidden="true">↻</span>
          <span>Recicla<strong>Fácil</strong></span>
        </a>
        <nav aria-label="Navegação principal">
          <a href="#como-funciona">Como funciona</a>
          <a href="#cooperativas">Cooperativas</a>
          <span className="nav-login">Área do usuário em breve</span>
        </nav>
      </header>

      <main>
        <section className="hero">
          <div className="hero-copy">
            <span className="eyebrow">Reciclagem perto de você</span>
            <h1>Transforme resíduos em um futuro mais limpo.</h1>
            <p>
              Encontre cooperativas, descubra quais materiais são aceitos e dê o destino
              certo aos recicláveis da sua casa ou empresa.
            </p>
            <a className="primary-action" href="#cooperativas">Encontrar cooperativa</a>
          </div>
          <div className="hero-art" aria-hidden="true">
            <div className="orbit orbit-one" />
            <div className="orbit orbit-two" />
            <div className="recycle-symbol">♻</div>
            <span className="material-chip chip-paper">Papel</span>
            <span className="material-chip chip-glass">Vidro</span>
            <span className="material-chip chip-metal">Metal</span>
          </div>
        </section>

        <section className="steps" id="como-funciona" aria-labelledby="steps-title">
          <div>
            <span className="eyebrow">Simples e consciente</span>
            <h2 id="steps-title">Reciclar ficou mais fácil</h2>
          </div>
          <ol>
            <li><span>01</span><strong>Encontre</strong><small>Busque por cidade, estado ou nome.</small></li>
            <li><span>02</span><strong>Separe</strong><small>Confira os materiais que a cooperativa recebe.</small></li>
            <li><span>03</span><strong>Recicle</strong><small>Combine a entrega ou agende uma coleta.</small></li>
          </ol>
        </section>

        <section className="directory" id="cooperativas" aria-labelledby="directory-title">
          <div className="section-heading">
            <div>
              <span className="eyebrow">Rede Recicla Fácil</span>
              <h2 id="directory-title">Encontre uma cooperativa</h2>
            </div>
            <p>Consulte informações atualizadas diretamente na nossa API.</p>
          </div>

          <form className="search-form" onSubmit={submitSearch}>
            <label>
              <span>Nome</span>
              <input
                value={filters.name}
                onChange={(event) => setFilters({ ...filters, name: event.target.value })}
                placeholder="Nome da cooperativa"
              />
            </label>
            <label>
              <span>Cidade</span>
              <input
                value={filters.city}
                onChange={(event) => setFilters({ ...filters, city: event.target.value })}
                placeholder="Ex.: Fortaleza"
              />
            </label>
            <label className="state-field">
              <span>UF</span>
              <input
                value={filters.state}
                maxLength={2}
                onChange={(event) => setFilters({ ...filters, state: event.target.value.toUpperCase() })}
                placeholder="CE"
              />
            </label>
            <button type="submit">Buscar</button>
          </form>

          <div className="results-status" aria-live="polite">
            {!loading && !error && (
              <span>{total === 1 ? '1 cooperativa encontrada' : `${total} cooperativas encontradas`}</span>
            )}
          </div>

          {error && <div className="alert" role="alert">{error}</div>}
          {loading ? (
            <div className="card-grid" aria-label="Carregando cooperativas">
              {[1, 2, 3].map((item) => <div className="cooperative-card skeleton" key={item} />)}
            </div>
          ) : cooperatives.length === 0 ? (
            <div className="empty-state">
              <span aria-hidden="true">⌕</span>
              <h3>Nenhuma cooperativa encontrada</h3>
              <p>Tente remover um filtro ou pesquisar uma cidade próxima.</p>
            </div>
          ) : (
            <div className="card-grid">
              {cooperatives.map((cooperative) => (
                <article className="cooperative-card" key={cooperative.id}>
                  <div className="card-icon" aria-hidden="true">♻</div>
                  <div>
                    <span className="location">{cooperative.city} · {cooperative.state}</span>
                    <h3>{cooperative.name}</h3>
                    <p>{cooperative.address}</p>
                  </div>
                  <button type="button" onClick={() => showDetails(cooperative.id)}>
                    Ver materiais <span aria-hidden="true">→</span>
                  </button>
                </article>
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <div className="pagination" aria-label="Paginação">
              <button
                type="button"
                disabled={query.page === 1}
                onClick={() => setQuery({ ...query, page: query.page - 1 })}
              >
                Anterior
              </button>
              <span>Página {query.page} de {totalPages}</span>
              <button
                type="button"
                disabled={query.page === totalPages}
                onClick={() => setQuery({ ...query, page: query.page + 1 })}
              >
                Próxima
              </button>
            </div>
          )}
        </section>
      </main>

      <footer>
        <a className="brand brand-footer" href="/">
          <span className="brand-mark" aria-hidden="true">↻</span>
          <span>Recicla<strong>Fácil</strong></span>
        </a>
        <p>Conectando pessoas e cooperativas por um planeta mais circular.</p>
      </footer>

      {(selected || detailsLoading) && (
        <div className="modal-backdrop" role="presentation" onMouseDown={() => setSelected(null)}>
          <section
            className="details-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="details-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <button className="modal-close" type="button" onClick={() => setSelected(null)} aria-label="Fechar">×</button>
            {detailsLoading || !selected ? (
              <p>Carregando detalhes…</p>
            ) : (
              <>
                <span className="eyebrow">{selected.city} · {selected.state}</span>
                <h2 id="details-title">{selected.name}</h2>
                <p className="modal-address">{selected.address}</p>
                {selected.email && <a href={`mailto:${selected.email}`}>{selected.email}</a>}
                <h3>Materiais comercializados</h3>
                {selected.materials.length ? (
                  <ul className="material-list">
                    {selected.materials.map((material) => (
                      <li key={material.id}>
                        <span>{material.description}</span>
                        <strong>
                          {material.resalePrice == null
                            ? 'Consulte'
                            : material.resalePrice.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                        </strong>
                      </li>
                    ))}
                  </ul>
                ) : <p>Esta cooperativa ainda não informou materiais.</p>}
              </>
            )}
          </section>
        </div>
      )}
    </div>
  )
}

export default App

import { useEffect, useState } from 'react'
import { getEmployeeRoute } from '../services/employees'
import type {
  EmployeeCollectionRoute,
  EmployeeRoutePoint,
} from '../services/employees'

interface Props {
  collectionId: number
  onClose: () => void
}

function locationValue(point: EmployeeRoutePoint) {
  return point.latitude !== null && point.longitude !== null
    ? `${point.latitude},${point.longitude}`
    : point.address
}

function directionsUrl(route: EmployeeCollectionRoute) {
  if (route.stops.length === 0) return ''
  const destination = route.stops.at(-1)!
  const waypoints = route.stops.slice(0, -1).map(locationValue).join('|')
  const parameters = new URLSearchParams({
    api: '1',
    origin: locationValue(route.origin),
    destination: locationValue(destination),
    travelmode: 'driving',
  })
  if (waypoints) parameters.set('waypoints', waypoints)
  return `https://www.google.com/maps/dir/?${parameters}`
}

function stopUrl(stop: EmployeeRoutePoint) {
  const parameters = new URLSearchParams({
    api: '1',
    destination: locationValue(stop),
  })
  return `https://www.google.com/maps/dir/?${parameters}`
}

export function CollectionRouteDialog({ collectionId, onClose }: Props) {
  const [route, setRoute] = useState<EmployeeCollectionRoute | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    getEmployeeRoute(collectionId)
      .then(setRoute)
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar o roteiro.',
      ))
  }, [collectionId])

  return (
    <div className="modal-backdrop nested-modal" onMouseDown={onClose}>
      <section className="operation-modal route-modal" role="dialog" aria-modal="true"
        aria-labelledby="route-dialog-title" onMouseDown={(event) => event.stopPropagation()}>
        <button className="modal-close" onClick={onClose} aria-label="Fechar">×</button>
        <span className="eyebrow">Rota da coleta</span>
        <h2 id="route-dialog-title">Paradas pendentes</h2>
        {error && <div className="alert" role="alert">{error}</div>}
        {!route && !error && <p>Carregando roteiro…</p>}
        {route && (
          <>
            <div className="route-origin">
              <span>Partida</span>
              <strong>{route.origin.name}</strong>
              <small>{route.origin.address}</small>
            </div>
            {route.stops.length === 0 ? (
              <p className="panel-empty">Todos os clientes já foram atendidos.</p>
            ) : (
              <>
                <ol className="route-stop-list">
                  {route.stops.map((stop) => (
                    <li key={stop.clientId}>
                      <span>{stop.sequence}</span>
                      <div>
                        <strong>{stop.name}</strong>
                        <small>{stop.address}</small>
                        {stop.latitude === null && (
                          <em>Endereço sem coordenada; confirme antes de sair.</em>
                        )}
                      </div>
                      <a href={stopUrl(stop)} target="_blank" rel="noreferrer">Navegar</a>
                    </li>
                  ))}
                </ol>
                {route.unmappedStops > 0 && (
                  <p className="route-warning">
                    {route.unmappedStops} parada(s) serão localizadas pelo endereço informado.
                  </p>
                )}
                <a className="operation-primary route-navigation" href={directionsUrl(route)}
                  target="_blank" rel="noreferrer">
                  Abrir roteiro no Google Maps
                </a>
              </>
            )}
          </>
        )}
      </section>
    </div>
  )
}

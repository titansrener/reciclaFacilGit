import { useEffect, useState } from 'react'
import { getCompanyOverview } from '../services/companies'
import type { CompanyOverview } from '../services/companies'

export function CompanyDashboard() {
  const [company, setCompany] = useState<CompanyOverview | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    getCompanyOverview()
      .then(setCompany)
      .catch((reason: unknown) => setError(
        reason instanceof Error ? reason.message : 'Não foi possível carregar a empresa.',
      ))
  }, [])

  if (error) {
    return <main className="client-dashboard"><div className="alert" role="alert">{error}</div></main>
  }
  if (!company) {
    return <main className="client-dashboard dashboard-loading">Carregando empresa…</main>
  }

  return (
    <main className="client-dashboard company-dashboard">
      <section className="dashboard-welcome">
        <div>
          <span className="eyebrow">Área empresarial</span>
          <h1>{company.corporateName}</h1>
          <p>Seu perfil empresarial está conectado à rede Recicla Fácil.</p>
        </div>
        <div className="impact-badge" aria-label="Perfil ativo">
          <strong>Ativo</strong>
          <span>perfil empresarial</span>
        </div>
      </section>

      <section className="company-profile" aria-labelledby="company-profile-title">
        <div className="panel-heading">
          <div>
            <span className="eyebrow">Dados cadastrais</span>
            <h2 id="company-profile-title">Informações da empresa</h2>
          </div>
        </div>
        <dl>
          <div><dt>CNPJ</dt><dd>{formatCnpj(company.cnpj)}</dd></div>
          <div><dt>E-mail</dt><dd>{company.email}</dd></div>
          <div><dt>Telefone</dt><dd>{company.phone}</dd></div>
          <div><dt>Fax</dt><dd>{company.fax || 'Não informado'}</dd></div>
          <div className="company-address"><dt>Endereço</dt><dd>{company.address}</dd></div>
        </dl>
      </section>
    </main>
  )
}

function formatCnpj(value: string) {
  return value.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}

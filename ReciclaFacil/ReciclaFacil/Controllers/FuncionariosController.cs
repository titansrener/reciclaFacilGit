using Microsoft.AspNet.Identity;
using ReciclaFacil.Models;
using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReciclaFacil.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class FuncionariosController : Controller
    {
        private Funcionarios funcionario;
        private ReciclaFacil_Contexto db;

        public FuncionariosController()
        {
            funcionario = null;
            db = new ReciclaFacil_Contexto();
        }

        #region Coletas

        // GET: Lista de Coletas 
        public ActionResult Index()
        {
            if (funcionario == null)
            {
                string id = User.Identity.GetUserId();
                funcionario = db.Funcionarios.Find(id);
            }
            
            return View(funcionario.Coletas.OrderByDescending(x => x.horaAgendada).ToList());
        }

        public ActionResult DetalheColeta(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Coletas c = db.Coletas.Find(coletaId);

            if (c == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }


            return View(c);
        }

        #endregion

        #region Clientes
        
        public ActionResult DetalheCliente(string clienteId, int? coletaId, string urlRetorno)
        {
            if(clienteId == null)
            {
                string mensagem = "Id do cliente não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ClientesColetas c = db.ClientesColetas.Find(coletaId, clienteId);

            if (c == null)
            {
                string mensagem = "Cliente não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("Index", "Funcionarios");
            if (urlRetorno != null)
            {
                ViewBag.retorno = @Url.Action("DetalheColeta", "Funcionarios", new { coletaId = coletaId, aba = 1 });
            }

            DetalheClienteViewModel model = new DetalheClienteViewModel()
            {
                cliente = c.Clientes,
                coleta = c.Coletas,
                materiais = c.MateriaisColetados.ToList(),
                clienteColeta = c
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetalheCliente(DetalheClienteViewModel model)
        {
            string clienteId = model.cliente.clienteId;
            ClientesColetas cc = db.ClientesColetas.Find(model.coleta.coletaId, clienteId);
            decimal saldoTotal = 0;
            bool achou = false;

            foreach (var mc in model.materiais)
            {
                MateriaisColetados m = cc.MateriaisColetados.Where(x => x.materialId == mc.materialId &&
                                                 x.coletaId == mc.coletaId &&
                                                 x.clienteId == mc.clienteId).SingleOrDefault();
                if (mc.quantidade > 0)
                {
                    achou = true;
                    m.quantidade = mc.quantidade;
                    m.coletado = "S";
                    if (model.cliente.tipo == "V")
                    {
                        MateriaisComercializados materialCoop = db.MateriaisComercializados.Find(m.materialId, cc.Clientes.cooperativaId);
                        m.valorCompra = materialCoop.valorRevenda.Value * (decimal)mc.quantidade.Value;
                        saldoTotal = saldoTotal + m.valorCompra.Value;                        
                    }
                    else
                    {
                        m.valorCompra = 0;
                    }
                }
                else
                {
                    m.quantidade = 0;
                    m.valorCompra = 0;
                    m.coletado = "N";
                }
            }

            if (achou)
            {
                if (model.cliente.tipo == "V")
                {
                    if (funcionario == null)
                    {
                        string id = User.Identity.GetUserId();
                        funcionario = db.Funcionarios.Find(id);
                    }
                    Coletas coleta = db.Coletas.Find(model.coleta.coletaId);
                    Notificacoes n = new Notificacoes()
                    {
                        clienteId = model.cliente.clienteId,
                        coletaId = model.coleta.coletaId,
                        cooperativaId = funcionario.cooperativaId,
                        dataHorario = DateTime.Now,
                        descricao = "Coleta do dia " + coleta.horaAgendada.Value.ToShortDateString() +
                                " - " + coleta.horaAgendada.Value.ToShortTimeString() + " foi relizada. O valor total a receber é de R$ " + saldoTotal,
                        ativa = true,
                        tipo = "C"
                    };

                    db.Notificacoes.Add(n);

                    
                }
                else {
                    cc.coletado = "S";
                }
            }
            else
                cc.coletado = "N";

            cc.horaDaColeta = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("DetalheColeta", "Funcionarios", new { coletaId = model.coleta.coletaId});
        }

        #endregion

        #region Rotas

        public ActionResult RotaColeta(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Coletas coleta = db.Coletas.Find(coletaId);
            
            ViewBag.retorno = @Url.Action("Index", "Funcionarios");

            if (funcionario == null)
            {
                string id = User.Identity.GetUserId();
                funcionario = db.Funcionarios.Find(id);
            }

            Cooperativas coop = funcionario.Cooperativas;

            List<ClientesColetas> clientes = coleta.ClientesColetas.Where(x => x.coletado == "A").ToList();
            Coordenada[] nClientesCoord = new Coordenada[clientes.Count()];
            int i = 0;
            foreach (var c in clientes)
            {
                DbGeometry coord = c.Clientes.enderecoCoordenada;
                nClientesCoord [i] = new Coordenada()
                {
                    latitude = coord.YCoordinate.ToString().Replace(",", "."),
                    longitude = coord.XCoordinate.ToString().Replace(",", ".")
                };
                i++;
            }

            RotasViewModel model = new RotasViewModel()
            {
                cooperativaCoord = new Coordenada() {
                    latitude = coop.enderecoCoordenada.YCoordinate.ToString().Replace(",","."),
                    longitude = coop.enderecoCoordenada.XCoordinate.ToString().Replace(",", ".")
                },
                clientesCoord = nClientesCoord
            };

            if (model.clientesCoord.Count() > 0)
                model.faltaCliente = true;
            else
                model.faltaCliente = false;

            return View(model);
        }

        #endregion
    }
}
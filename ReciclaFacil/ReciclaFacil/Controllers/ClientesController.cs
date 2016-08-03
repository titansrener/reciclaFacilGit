using Microsoft.AspNet.Identity;
using ReciclaFacil.Models;
using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ReciclaFacil.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientesController : Controller
    {
        private Clientes cliente;
        private ReciclaFacil_Contexto db;

        public ClientesController()
        {
            cliente = null;
            db = new ReciclaFacil_Contexto();
        }

        #region Coletas

        // GET: Clientes com lista de coletas
        public ActionResult Index()
        {
            if(cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            return View(cliente.ClientesColetas.OrderByDescending(x => x.Coletas.horaAgendada).ToList());
        }



        public ActionResult AgendarColeta()
        {
            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            var coletas = cliente.Cooperativas.Coletas
                                 .Where(x => x.coletado == "A" && x.horaAgendada > DateTime.Now)
                                 .OrderBy(x => x.horaAgendada).ToList();

            foreach (var cc in cliente.ClientesColetas)
            {
                Coletas c = coletas.Find(x => x.coletaId == cc.coletaId);
                coletas.Remove(c);
            }

            ViewBag.horario = coletas.Select(h => new SelectListItem
            {
                Value = h.coletaId.ToString(),
                Text = h.horaAgendada.Value.ToShortDateString() + " - " + h.horaAgendada.Value.ToShortTimeString()
            });

            var materiaisCoop = cliente.Cooperativas.MateriaisComercializados.ToList();

            AgendamentoViewModel avm = new AgendamentoViewModel();
            avm.materiais = materiaisCoop.Select(m => m.Materiais).ToList();

            return View(avm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgendarColeta(AgendamentoViewModel model)
        {
            string clienteId = User.Identity.GetUserId();
            List<MateriaisColetados> lista = new List<MateriaisColetados>();
            bool achou = false;
            foreach (Materiais m in model.materiais)
            {
                if (m.selecionado)
                {
                    achou = true;
                    lista.Add(new MateriaisColetados(){ materialId = m.materialId, coletado = "A" });
                }
            }

            if (!achou)
            {
                ViewBag.erro = "Marque pelo menos um item!";
            } else if(ModelState.IsValid)
            {
                string idCliente = User.Identity.GetUserId();
                ClientesColetas cc = new ClientesColetas()
                {
                    clienteId = idCliente,
                    coletaId = model.coletaId,                    
                    coletado = "A",
                    horaDaColeta = null
                };

                db.ClientesColetas.Add(cc);

                foreach (MateriaisColetados m in lista)
                {
                    m.clienteId = idCliente;
                    m.coletaId = model.coletaId;
                    db.MateriaisColetados.Add(m);
                }

                db.SaveChanges();

                return RedirectToAction("Index", "Clientes");
            }


            #region Validação Falha
            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }
            var coletas = cliente.Cooperativas.Coletas
                                 .Where(x => x.coletado == "A" && x.horaAgendada > DateTime.Now)
                                 .OrderBy(x => x.horaAgendada).ToList();

            foreach (var cc in cliente.ClientesColetas)
            {
                Coletas c = coletas.Find(x => x.coletaId == cc.coletaId);
                coletas.Remove(c);
            }

            ViewBag.horario = coletas.Select(h => new SelectListItem
            {
                Value = h.coletaId.ToString(),
                Text = h.horaAgendada.Value.ToShortDateString() + " - " + h.horaAgendada.Value.ToShortTimeString()
            });

            #endregion
            return View(model);
        }



        public ActionResult EditarColeta(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }
            
            var coletas = cliente.Cooperativas.Coletas
                                 .Where(x => x.coletado == "A" && x.horaAgendada > DateTime.Now)
                                 .OrderBy(x => x.horaAgendada).ToList();

            foreach (var cc in cliente.ClientesColetas)
            {
                Coletas c = coletas.Find(x => x.coletaId == cc.coletaId && x.coletaId != coletaId);
                coletas.Remove(c);
            }

            ViewBag.horario = coletas.Select(h => new SelectListItem
            {
                Value = h.coletaId.ToString(),
                Text = h.horaAgendada.Value.ToShortDateString() + " - " + h.horaAgendada.Value.ToShortTimeString()
            });

            ClientesColetas coleta = cliente.ClientesColetas.SingleOrDefault(c => c.coletaId == coletaId);

            if (coleta == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            var matCliente = coleta.MateriaisColetados.Select(m => m.Materiais).ToList();  
                
            var materiaisCoop = cliente.Cooperativas.MateriaisComercializados.Select(m => m.Materiais).ToList();

            foreach (var item in materiaisCoop)
            {
                if (matCliente.Contains(item))
                {
                    item.selecionado = true;
                }
            }

            AgendamentoViewModel avm = new AgendamentoViewModel();
            avm.coletaId = coletaId.Value;
            avm.novoColetaId = coletaId.Value;
            avm.materiais = materiaisCoop.ToList();

            return View(avm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarColeta(AgendamentoViewModel model)
        {
            List<MateriaisColetados> lista = new List<MateriaisColetados>();
            bool achou = false;
            string clienteId = User.Identity.GetUserId();
            foreach (Materiais m in model.materiais)
            {
                if (m.selecionado)
                {
                    achou = true;
                    lista.Add(new MateriaisColetados() { materialId = m.materialId,  coletado = "A" });
                    //lista.Add(new MateriaisColetados() { materialId = m.materialId, clienteId = clienteId, coletado = "A" });  Substituído pela lina acima
                }
            }

            if (!achou)
            {
                ViewBag.erro = "Marque pelo menos um item!";
            }
            else if (ModelState.IsValid)
            {
                if(model.coletaId != model.novoColetaId)
                {
                    ClientesColetas cc = db.ClientesColetas.Find(model.coletaId, clienteId);
                    db.ClientesColetas.Remove(cc);
                    cc = new ClientesColetas()
                    {
                        clienteId = clienteId,
                        coletaId = model.novoColetaId,
                        coletado = "A",
                        horaDaColeta = null
                    };
                    db.ClientesColetas.Add(cc);
                }

                ClientesColetas cc1 = db.ClientesColetas.Find(model.coletaId, clienteId);
                List<MateriaisColetados> mc = db.MateriaisColetados.Where(m => m.coletaId == model.coletaId && cc1.clienteId == clienteId).ToList();

                //List<MateriaisColetados> mc = db.MateriaisColetados.Where(m => m.coletaId == model.coletaId && m.clienteId == clienteId).ToList(); Substituído pelas 2 linhas acima
                db.MateriaisColetados.RemoveRange(mc);

                foreach (MateriaisColetados m in lista)
                {
                    m.coletaId = model.novoColetaId;
                    db.MateriaisColetados.Add(m);
                }

                db.SaveChanges();

                return RedirectToAction("Index", "Clientes");
            }

            #region Falha Validação
            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            var coletas = cliente.Cooperativas.Coletas
                                 .Where(x => x.coletado == "A" && x.horaAgendada > DateTime.Now)
                                 .OrderBy(x => x.horaAgendada).ToList();

            foreach (var cc in cliente.ClientesColetas)
            {
                Coletas c = coletas.Find(x => x.coletaId == cc.coletaId && x.coletaId != model.coletaId);
                coletas.Remove(c);
            }

            ViewBag.horario = coletas.Select(h => new SelectListItem
            {
                Value = h.coletaId.ToString(),
                Text = h.horaAgendada.Value.ToShortDateString() + " - " + h.horaAgendada.Value.ToShortTimeString()
            });

            #endregion

            return View(model);
        }



        public ActionResult DetalheColeta(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            ClientesColetas coleta = cliente.ClientesColetas.SingleOrDefault(c => c.coletaId == coletaId);

            if (coleta == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
                        
            return View(coleta);
        }



        public ActionResult DeletarColeta(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            ClientesColetas coleta = cliente.ClientesColetas.SingleOrDefault(c => c.coletaId == coletaId);

            if (coleta == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            return View(coleta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletarColeta(int coletaId)
        {
            if (cliente == null)
            {
                string clienteId = User.Identity.GetUserId();
                cliente = db.Clientes.Find(clienteId);
            }

            ClientesColetas coleta = cliente.ClientesColetas.SingleOrDefault(c => c.coletaId == coletaId);

            db.MateriaisColetados.RemoveRange(coleta.MateriaisColetados); //Substituídos pelas linhas abaixo
            db.ClientesColetas.Remove(coleta);
            db.SaveChanges();


            //ClientesColetas clienteColeta = cliente.ClientesColetas.SingleOrDefault(c => c.coletaId == coletaId);

            //db.MateriaisColetados.RemoveRange(clienteColeta.Coletas.MateriaisColetados);
            //db.ClientesColetas.Remove(clienteColeta);
            //db.SaveChanges();

            return RedirectToAction("Index", "Clientes");
        }

        #endregion

        #region Notificações
        public PartialViewResult notificacoesCount()
        {
            string id = User.Identity.GetUserId();
            int count = db.Notificacoes.Count(x => x.clienteId == id && x.ativa == true);
            return PartialView(count);
        }

        public ActionResult Notificacoes()
        {
            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            //return View(cliente.Notificacoes.OrderByDescending(n => n.dataHorario).ToList());                 Substituído pela linha abaixo
            return View(cliente.Cooperativas.Notificacoes.Where(c => c.clienteId == cliente.clienteId).OrderByDescending(n => n.dataHorario).ToList());

        }

        public bool desativaNotificacao(int notificacaoId)
        {
            try
            {
                string clienteId = User.Identity.GetUserId();
                db.Notificacoes.Find(notificacaoId, clienteId).ativa = false;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Carteira
        public ActionResult Carteira()
        {
            if (cliente == null)
            {
                string id = User.Identity.GetUserId();
                cliente = db.Clientes.Find(id);
            }

            List<Carteiras> coletas = cliente.Carteiras.OrderByDescending(x => x.dataUltimaMovimentacao).ToList();

            ViewBag.total = coletas.Sum(x => x.saldo);

            return View(coletas);
        }

        #endregion

        #region Aceita/Recusa Valor
        public ActionResult AceitarValor(int coletaId)
        {
            string clienteId = User.Identity.GetUserId();
            ClientesColetas cc = db.ClientesColetas.Find(coletaId, clienteId);
            

            if (cc.coletado != "A")
            {
                string mensagem = "Coleta já finalizada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            cc.coletado = "S";
            decimal total = 0, saldoAnt = 0;
            foreach (var mc in cc.MateriaisColetados)
            {
                total = total + mc.valorCompra.Value;
            }

            

            Carteiras carteira = new Carteiras()
            {
                clienteId = clienteId,
                dataUltimaMovimentacao = DateTime.Now,
                saldo = total
            };

            db.Carteiras.Add(carteira);
            db.SaveChanges();

            return RedirectToAction("Carteira", "Clientes");
        }

        public ActionResult RecusarValor(int coletaId)
        {
            string clienteId = User.Identity.GetUserId();
            ClientesColetas cc = db.ClientesColetas.Find(coletaId, clienteId);

            if (cc.coletado != "A")
            {
                string mensagem = "Coleta já finalizada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            cc.coletado = "A";

            foreach (var mc in cc.MateriaisColetados)
            {
                mc.coletado = "A";
                mc.valorCompra = null;
                mc.quantidade = null;
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        #endregion

    }
}
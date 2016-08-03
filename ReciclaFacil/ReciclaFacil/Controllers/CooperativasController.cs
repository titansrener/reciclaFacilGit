using Microsoft.AspNet.Identity;
using ReciclaFacil.Models;
using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ReciclaFacil.Controllers
{
    [Authorize(Roles = "Cooperativa")]
    public class CooperativasController : Controller
    {
        private Cooperativas cooperativa;
        private ReciclaFacil_Contexto db;
        
        public CooperativasController()
        {
            cooperativa = null;
            db = new ReciclaFacil_Contexto();
        }

        #region _#Coletas

        // GET: Coletas da Cooperativa
        public ActionResult Index()
        {
            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }
            return View(cooperativa.Coletas.OrderByDescending(x => x.horaAgendada).ToList());
        }

        public ActionResult CriarColeta()
        {
            var horaList = new List<int>();
            for (int i = 8; i <= 18; i++)
            {
               horaList.Add(i);
            }

            ViewBag.horas = horaList.Select(h => new SelectListItem
            {
                Value = h.ToString(),
                Text = h < 10 ? ("0" + h + ":00 hs") : (h + ":00 hs")
            });
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CriarColeta(ColetaViewModel model)
        {
            if (ModelState.IsValid)
            {
                Coletas c = new Coletas();
                string idCoop = User.Identity.GetUserId();

                c.horaAgendada = model.data.AddHours(model.hora);
                c.quantidade = null;
                c.coletado = "A";
                c.cooperativaId = idCoop;

                db.Coletas.Add(c);
                db.SaveChanges();

                return RedirectToAction("Index", "Cooperativas");
            }

            #region ValidaçãoFalsa
            var horaList = new List<int>();
            for (int i = 8; i <= 18; i++)
            {
                horaList.Add(i);
            }

            ViewBag.horas = horaList.Select(h => new SelectListItem
            {
                Value = h.ToString(),
                Text = h < 10 ? ("0" + h + ":00 hs") : (h + ":00 hs")
            });
            #endregion
            return View(model);
        }

        public ActionResult EditarColeta(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Coletas c = db.Coletas.Find(id);

            if (c == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if(c.ClientesColetas.Count() > 0)
            {
                if (c.coletado == "F")
                {
                    ViewBag.Aviso = "Atenção: Coleta já finalizada!";
                }
                else {
                    ViewBag.Aviso = "Atenção: a coleta já possui cliente(s)!";
                }
            }

            ColetaViewModel cvm = new ColetaViewModel();
            cvm.id = c.coletaId;
            cvm.idCoop = c.cooperativaId;
            DateTime d = c.horaAgendada.Value;
            cvm.data = new DateTime(d.Year, d.Month, d.Day);
            cvm.hora = c.horaAgendada.Value.Hour;

            var horaList = new List<int>();
            for (int i = 8; i <= 18; i++)
            {
                horaList.Add(i);
            }

            ViewBag.horas = horaList.Select(h => new SelectListItem
            {
                Value = h.ToString(),
                Text = h < 10 ? ("0" + h + ":00 hs") : (h + ":00 hs")
            });

            return View(cvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarColeta(ColetaViewModel model)
        {
            if (ModelState.IsValid)
            {
                Coletas c = db.Coletas.Find(model.id);
                c.horaAgendada = model.data;
                c.horaAgendada = c.horaAgendada.Value.AddHours(model.hora);

                db.Entry(c).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Cooperativas");
            }

            #region ValidaçãoFalsa
            var horaList = new List<int>();
            for (int i = 8; i <= 18; i++)
            {
                horaList.Add(i);
            }

            ViewBag.horas = horaList.Select(h => new SelectListItem
            {
                Value = h.ToString(),
                Text = h < 10 ? ("0" + h + ":00 hs") : (h + ":00 hs")
            });
            #endregion

            return View(model);
        }

        public ActionResult DetalheColeta(int? id, int? aba)
        {
            if (id == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Coletas c = db.Coletas.Find(id);

            if (c == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (aba.HasValue)
                ViewBag.aba = aba.Value;

            return View(c);
        }

        public ActionResult DeletarColeta(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Coletas c = db.Coletas.Find(id);

            if (c == null)
            {
                string mensagem = "Coleta não encontrada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (c.ClientesColetas.Count() > 0)
            {
                if (c.coletado == "F")
                {
                    ViewBag.Aviso = "Atenção: Coleta já finalizada!";
                }
                else {
                    ViewBag.Aviso = "Atenção: a coleta já possui cliente(s)!";
                }
            }
            
            return View(c);
        }

        [HttpPost, ActionName("DeletarColeta")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletarColeta(int id)
        {
            Coletas c = db.Coletas.Find(id);

            db.Coletas.Remove(c);
            db.SaveChanges();

            return RedirectToAction("Index", "Cooperativas");
        }

        public ActionResult IniciarColeta(int? coletaId)
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

            c.coletado = "I";
            List<Notificacoes> notificacoes = new List<Notificacoes>();
            foreach (var cc in c.ClientesColetas.ToList())
            {
                Notificacoes n = new Notificacoes()
                {
                    clienteId = cc.Clientes.clienteId,
                    coletaId = c.coletaId,
                    cooperativaId = c.cooperativaId,
                    dataHorario = DateTime.Now,
                    descricao = "Coleta do dia " + c.horaAgendada.Value.ToShortDateString() + 
                                " - " + c.horaAgendada.Value.ToShortTimeString() + " foi iniciada.",
                    ativa = true,
                    tipo = "A"
                };
                notificacoes.Add(n);
            }
            db.Notificacoes.AddRange(notificacoes);
            db.SaveChanges();

            return RedirectToAction("DetalheColeta", "Cooperativas", new { id = coletaId });
        }

        public ActionResult FinalizarColeta(int? coletaId)
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

            List<Notificacoes> notificacoes = new List<Notificacoes>();
            foreach (var cc in c.ClientesColetas)
            {
                if(cc.coletado == "A")
                {
                    cc.coletado = "N";
                    foreach (var m in cc.MateriaisColetados)
                    {
                        m.coletado = "N";
                    }
                }

                Notificacoes n = new Notificacoes()
                {
                    clienteId = cc.Clientes.clienteId,
                    coletaId = c.coletaId,
                    cooperativaId = c.cooperativaId,
                    dataHorario = DateTime.Now,
                    descricao = "Coleta do dia " + c.horaAgendada.Value.ToShortDateString() +
                                " - " + c.horaAgendada.Value.ToShortTimeString() + " foi finalizada.",
                    ativa = true,
                    tipo = "A"
                };
                notificacoes.Add(n);
            }

            c.coletado = "F";
            db.Notificacoes.AddRange(notificacoes);
            db.SaveChanges();

            return RedirectToAction("DetalheColeta", "Cooperativas", new { id = coletaId });
        }

        #endregion

        #region _#Caminhões

        //Get: Caminhões da Cooperativa
        public ActionResult Caminhoes()
        {
            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }
            return View(cooperativa.Caminhoes);
        }

        public ActionResult CriarCaminhao()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CriarCaminhao(CaminhaoViewModel model)
        {
            if(ModelState.IsValid)
            {
                Caminhoes caminhao = db.Caminhoes.Where(r => r.placa == model.placa).FirstOrDefault();
                if (caminhao == null)
                {
                    ViewBag.Erro = null;
                    caminhao = new Caminhoes();
                    caminhao.descricao = model.descricao;
                    caminhao.placa = model.placa;
                    caminhao.cooperativaId = User.Identity.GetUserId();

                    db.Caminhoes.Add(caminhao);
                    db.SaveChanges();

                    return RedirectToAction("Caminhoes", "Cooperativas");
                }
                else
                {
                    ViewBag.Erro = "Placa já cadastrada!";
                }
            }
            
            return View(model);
        }

        public ActionResult EditarCaminhao(int? id)
        {
            if(id == null)
            {
                string mensagem = "Id do caminhão não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Caminhoes c = db.Caminhoes.Find(id);

            if (c == null)
            {
                string mensagem = "Caminhão não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            return View(c);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCaminhao(Caminhoes model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Caminhoes", "Cooperativas");
            }

            return View(model);
        }

        public ActionResult DetalheCaminhao(int? id, int? coletaId, string urlRetorno)
        {
            if (id == null)
            {
                string mensagem = "Id do caminhão não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            Caminhoes c = db.Caminhoes.Find(id);
            if (c == null)
            {
                string mensagem = "Caminhão não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("Caminhoes", "Cooperativas");
            if (urlRetorno != null)
            {
                ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 2 });
            }
            return View(c);
        }

        public ActionResult DeletarCaminhao(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do caminhão não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            Caminhoes c = db.Caminhoes.Find(id);
            if (c == null)
            {
                string mensagem = "Caminhão não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            return View(c);
        }

        [HttpPost, ActionName("DeletarCaminhao")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCaminhao(int id)
        {
            Caminhoes c = db.Caminhoes.Find(id);

            db.Caminhoes.Remove(c);
            db.SaveChanges();

            return RedirectToAction("Caminhoes", "Cooperativas");
        }

        public ActionResult AssociarCaminhoes(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }

            var caminhoesCoop = cooperativa.Caminhoes.ToList();
            var caminhoes = new List<CaminhaoColetaViewModel>();
            Coletas coleta = cooperativa.Coletas.Where(x => x.coletaId == coletaId.Value).SingleOrDefault();

            foreach (var c in caminhoesCoop)
            {
                if (!coleta.Caminhoes.Contains(c))
                {
                    caminhoes.Add(new CaminhaoColetaViewModel()
                    {
                        caminhaoId = c.caminhaoId,
                        placa = c.placa,
                        descricao = c.descricao,
                        selecionado = false
                    });
                }
            }

            AssociarCaminhaoViewModel model = new AssociarCaminhaoViewModel()
            {
                coletaId = coletaId.Value,
                caminhoes = caminhoes
            };

            ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 2 });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssociarCaminhoes(AssociarCaminhaoViewModel model)
        {
            Coletas coleta = db.Coletas.Find(model.coletaId);
            bool achou = false;

            foreach (var mc in model.caminhoes)
            {
                if (mc.selecionado) {
                    achou = true;
                    Caminhoes c = db.Caminhoes.Find(mc.caminhaoId);
                    coleta.Caminhoes.Add(c);
                }
            }

            if (achou)
            {
                db.SaveChanges();
                return RedirectToAction("DetalheColeta", "Cooperativas", new { id = model.coletaId, aba = 2 });
            }

            ViewBag.aviso = "Selecione pelo menos um caminhão!";

            return View(model);
        }

        public ActionResult DesassociarCaminhao(int? caminhaoId, int? coletaId)
        {
            if (caminhaoId == null)
            {
                string mensagem = "Id do caminhão não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Caminhoes c = db.Caminhoes.Find(caminhaoId);

            if (c == null)
            {
                string mensagem = "Caminhão não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 2 });
            ViewBag.coletaId = coletaId.Value;
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DesassociarCaminhao(int caminhaoId, int? coletaId)
        {
            Coletas coleta = db.Coletas.Find(coletaId);
            if (coleta != null)
            {
                Caminhoes c = coleta.Caminhoes.Where(x => x.caminhaoId == caminhaoId).SingleOrDefault();
                coleta.Caminhoes.Remove(c);
                db.SaveChanges(); 
            }

            return RedirectToAction("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 2 });
        }

        #endregion

        #region _#Materiais

        public ActionResult Materiais()
        {
            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }
            return View(cooperativa.MateriaisComercializados);
        }

        public ActionResult AssociaMateriais()
        {
            List<Materiais> materiais = db.Materiais.ToList();

            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }

            foreach (var mc in cooperativa.MateriaisComercializados)
            {
                Materiais m = materiais.Find(x => x.materialId == mc.materialId);
                if (m != null)
                {
                    materiais.Remove(m);
                }
            }

            return View(materiais);
        }
                
        [HttpPost]
        public ActionResult AssociaMateriais(List<Materiais> materiais)
        {
            string id = User.Identity.GetUserId();
            bool achou = false;
            foreach (Materiais m in materiais)
            {
                if (m.selecionado)
                { 
                    achou = true;
                    MateriaisComercializados mc = new MateriaisComercializados();
                    mc.cooperativaId = id;
                    mc.materialId = m.materialId;
                    mc.valorRevenda = null;
                    db.MateriaisComercializados.Add(mc);
                }
            }

            if (!achou) {
                ViewBag.erro = "Marque pelo menos um item!";
            }
            else
            {
                db.SaveChanges();
                return RedirectToAction("Materiais", "Cooperativas");
            }
            return View(materiais);
        }

        public ActionResult DetalheMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            string idCoop = User.Identity.GetUserId();
            MateriaisComercializados mc = db.MateriaisComercializados.Find(id, idCoop);

            if (mc == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            
            return View(mc);
        }

        public ActionResult EditarMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            string idCoop = User.Identity.GetUserId();
            MateriaisComercializados mc = db.MateriaisComercializados.Find(id, idCoop);

            if (mc == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            return View(mc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMaterial(MateriaisComercializados mc)
        {
            if (ModelState.IsValid)
            {
                
                db.Entry(mc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Materiais", "Cooperativas");
            }
            return View(mc);
        }

        public ActionResult DeletarMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            string idCoop = User.Identity.GetUserId();
            MateriaisComercializados mc = db.MateriaisComercializados.Find(id, idCoop);

            if (mc == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            return View(mc);
        }

        [HttpPost, ActionName("DeletarMaterial")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMaterial(int id)
        {
            string idCoop = User.Identity.GetUserId();
            MateriaisComercializados mc = db.MateriaisComercializados.Find(id, idCoop);
            if(mc != null)
            {
                db.MateriaisComercializados.Remove(mc);
                db.SaveChanges();
                return RedirectToAction("Materiais", "Cooperativas");
            }
            else {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
        }

        #endregion

        #region _#Funcionarios

        public ActionResult Funcionarios()
        {
            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }

            return View(cooperativa.Funcionarios);
        }
        
        public ActionResult DetalheFuncionario(string id, int? coletaId, string urlRetorno)
        {
            if (id == null)
            {
                string mensagem = "Id do funcionário não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("Funcionarios", "Cooperativas");
            if (urlRetorno != null)
            {
                ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 1 });
            }

            Funcionarios f = db.Funcionarios.Find(id);

            if (f == null)
            {
                string mensagem = "Funcionário não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Usuarios u = db.Usuarios.Find(id);
            RegFuncionarioViewModel fvm = new RegFuncionarioViewModel()
            {
                idFunc = id,
                nome = f.nome,
                dataNascimento = f.dataNascimento,
                email = u.Email
            };

            return View(fvm);
        }

        public ActionResult EditarFuncionario(string id)
        {
            if (id == null)
            {
                string mensagem = "Id do funcionário não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Funcionarios f = db.Funcionarios.Find(id);

            if (f == null)
            {
                string mensagem = "Funcionário não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            RegFuncionarioViewModel fvm = new RegFuncionarioViewModel()
            {
                idFunc = id,
                nome = f.nome,
                dataNascimento = f.dataNascimento,
                email = "email@email.com",
                Password = "12345678",
                ConfirmPassword = "12345678"
            };

            return View(fvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarFuncionario(RegFuncionarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                Funcionarios f = db.Funcionarios.Find(model.idFunc);
                f.nome = model.nome;
                f.dataNascimento = model.dataNascimento;

                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Funcionarios", "Cooperativas");
            }

            return View(model);
        }

        public ActionResult DeletarFuncionario(string id)
        {
            if (id == null)
            {
                string mensagem = "Id do funcionário não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Funcionarios f = db.Funcionarios.Find(id);

            if (f == null)
            {
                string mensagem = "Funcionário não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Usuarios u = db.Usuarios.Find(id);
            RegFuncionarioViewModel fvm = new RegFuncionarioViewModel()
            {
                idFunc = id,
                nome = f.nome,
                email = u.Email
            };

            return View(fvm);
        }

        [HttpPost, ActionName("DeletarFuncionario")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFuncionario(string id)
        {
            Funcionarios f = db.Funcionarios.Find(id);
            Usuarios u = db.Usuarios.Find(id);
            UsuarioRole ur = db.UsuarioRole.Where(x => x.UserId == id).SingleOrDefault();

            db.Funcionarios.Remove(f);
            db.Usuarios.Remove(u);
            db.UsuarioRole.Remove(ur);
            db.SaveChanges();

            return RedirectToAction("Funcionarios", "Cooperativas");
        }
                
        public ActionResult AssociarFuncionario(int? coletaId)
        {
            if (coletaId == null)
            {
                string mensagem = "Id da coleta não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            if (cooperativa == null)
            {
                string id = User.Identity.GetUserId();
                cooperativa = db.Cooperativas.Find(id);
            }

            var funcionariosCoop = cooperativa.Funcionarios.ToList();
            var funcionarios = new List<FuncionarioColetaViewModel>();
            Coletas c = db.Coletas.Find(coletaId);

            foreach (var f in funcionariosCoop)
            {
                if (!c.Funcionarios.Contains(f))
                {
                    funcionarios.Add(new FuncionarioColetaViewModel()
                    {
                        funcionarioId = f.funcionarioId,
                        nome = f.nome,
                        selecionado = false
                    });
                }
            }

            AssociarFuncionarioViewModel model = new AssociarFuncionarioViewModel()
            {
                coletaId = coletaId.Value,
                funcionarios = funcionarios
            };

            ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 1 });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssociarFuncionario(AssociarFuncionarioViewModel model)
        {
            Coletas c = db.Coletas.Find(model.coletaId);
            bool achou = false;

            foreach (var mf in model.funcionarios)
            {
                if (mf.selecionado)
                {
                    achou = true;
                    Funcionarios f = db.Funcionarios.Find(mf.funcionarioId);
                    c.Funcionarios.Add(f);
                }
            }

            if(achou)
            {
                db.SaveChanges();
                return RedirectToAction("DetalheColeta", "Cooperativas", new { id = model.coletaId, aba = 1 });
            }

            ViewBag.aviso = "Selecione pelo menos um funcionário!";
            return View(model);
        }

        public ActionResult DesassociarFuncionario(string funcionarioId, int? coletaId)
        {
            if (funcionarioId == null)
            {
                string mensagem = "Id do funcionário não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Funcionarios f = db.Funcionarios.Find(funcionarioId);

            if (f == null)
            {
                string mensagem = "Funcionário não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 1 });
            ViewBag.coletaId = coletaId.Value;
            return View(f);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DesassociarFuncionario(string funcionarioId, int coletaId)
        {
            Coletas c = db.Coletas.Find(coletaId);
            if(c != null)
            {
                Funcionarios f = c.Funcionarios.Where(x => x.funcionarioId == funcionarioId).SingleOrDefault();
                c.Funcionarios.Remove(f);
                db.SaveChanges();
            }
            return RedirectToAction("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 1 });
        }

        #endregion

        #region _#Clientes

        public ActionResult DetalheCliente(string id, int? coletaId)
        {
            if(id == null)
            {
                string mensagem = "Id do cliente não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ViewBag.retorno = @Url.Action("DetalheColeta", "Cooperativas", new { id = coletaId, aba = 3 });

            ClientesColetas cc = db.ClientesColetas.Find(coletaId, id);
            
            return View(cc);
        }

        #endregion

    }
}
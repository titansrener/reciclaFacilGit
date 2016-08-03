using ReciclaFacil.Models;
using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReciclaFacil.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            #region Preenchedo Lista de Estados do BR

            var EstadoList = new List<dynamic>();
            EstadoList.Add(new { Id = "AC", Text = "Acre" });
            EstadoList.Add(new { Id = "AL", Text = "Alagoas" });
            EstadoList.Add(new { Id = "AP", Text = "Amapá" });
            EstadoList.Add(new { Id = "AM", Text = "Amazonas" });
            EstadoList.Add(new { Id = "BA", Text = "Bahia" });
            EstadoList.Add(new { Id = "CE", Text = "Ceará" });
            EstadoList.Add(new { Id = "DF", Text = "Distrito Federal" });
            EstadoList.Add(new { Id = "ES", Text = "Espírito Santo" });
            EstadoList.Add(new { Id = "GO", Text = "Goiás" });
            EstadoList.Add(new { Id = "MA", Text = "Maranhão" });
            EstadoList.Add(new { Id = "MT", Text = "Mato Grosso" });
            EstadoList.Add(new { Id = "MS", Text = "Mato Grosso do Sul" });
            EstadoList.Add(new { Id = "MG", Text = "Minas Gerais" });
            EstadoList.Add(new { Id = "PA", Text = "Pará" });
            EstadoList.Add(new { Id = "PB", Text = "Paraíba" });
            EstadoList.Add(new { Id = "PR", Text = "Paraná" });
            EstadoList.Add(new { Id = "PE", Text = "Pernambuco" });
            EstadoList.Add(new { Id = "PI", Text = "Piauí" });
            EstadoList.Add(new { Id = "RJ", Text = "Rio de Janeiro" });
            EstadoList.Add(new { Id = "RN", Text = "Rio Grande do Norte" });
            EstadoList.Add(new { Id = "RS", Text = "Rio Grande do Sul" });
            EstadoList.Add(new { Id = "RO", Text = "Rondônia" });
            EstadoList.Add(new { Id = "RR", Text = "Roraima" });
            EstadoList.Add(new { Id = "SC", Text = "Santa Catarina" });
            EstadoList.Add(new { Id = "SP", Text = "São Paulo" });
            EstadoList.Add(new { Id = "SE", Text = "Sergipe" });
            EstadoList.Add(new { Id = "TO", Text = "Tocantins" });

            ViewBag.estado = new SelectList(EstadoList, "Id", "Text");

            #endregion
            return View();
        }

        public ActionResult Erro(string Mensagem)
        {
            ViewBag.Erro = Mensagem;
            return View();
        }

        #region Cooperativas
        public PartialViewResult MapaCooperativa(string razaoSocial, string cidade, string estado)
        {
            ReciclaFacil_Contexto db = new ReciclaFacil_Contexto();
            Cooperativas[] cooperativas;
            string busca = "";

            busca = razaoSocial != "" ? busca + 1 : busca;
            busca = cidade != "" ? busca + 2 : busca;
            busca = estado != "" ? busca + 3 : busca;

            switch (busca)
            {
                case "1":
                    cooperativas = db.Cooperativas.Where(c => c.razaoSocial.Contains(razaoSocial)).ToArray();
                    break;
                case "2":
                    cooperativas = db.Cooperativas.Where(c => c.cidade.Contains(cidade)).ToArray();
                    break;
                case "3":
                    cooperativas = db.Cooperativas.Where(c => c.estado.Contains(estado)).ToArray();
                    break;
                case "12":
                    cooperativas = db.Cooperativas.Where(c => c.razaoSocial.Contains(razaoSocial) && c.cidade.Contains(cidade)).ToArray();
                    break;
                case "13":
                    cooperativas = db.Cooperativas.Where(c => c.razaoSocial.Contains(razaoSocial) && c.estado.Contains(estado)).ToArray();
                    break;
                case "23":
                    cooperativas = db.Cooperativas.Where(c => c.cidade.Contains(cidade) && c.estado.Contains(estado)).ToArray();
                    break;
                case "123":
                    cooperativas = db.Cooperativas.Where(c => c.razaoSocial.Contains(razaoSocial) && c.cidade.Contains(cidade) && c.estado.Contains(estado)).ToArray();
                    break;
                default:
                    cooperativas = db.Cooperativas.ToArray();
                    break;
            }

            CooperativaMapa[] cm = new CooperativaMapa[cooperativas.Count()];
            for (int i = 0; i < cooperativas.Count(); i++)
            {
                cm[i] = new CooperativaMapa()
                {
                    nome = cooperativas.ElementAt(i).razaoSocial,
                    latitude = cooperativas.ElementAt(i).enderecoCoordenada.YCoordinate.Value.ToString().Replace(",", "."),
                    longitude = cooperativas.ElementAt(i).enderecoCoordenada.XCoordinate.Value.ToString().Replace(",", "."),
                    url = @Url.Action("DetalheCooperativa", "Home", new { cooperativaId = cooperativas[i].cooperativaId })
            };
            }

            MapaCooperativaViewModel model = new MapaCooperativaViewModel()
            {
                cooperativas = cm
            };

            return PartialView(model);
        }

        public ActionResult DetalheCooperativa(string cooperativaId)
        {
            if (String.IsNullOrEmpty(cooperativaId))
            {
                string mensagem = "Cooperativa não informada!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            ReciclaFacil_Contexto db = new ReciclaFacil_Contexto();
            Cooperativas c = db.Cooperativas.Find(cooperativaId);

            return View(c);
        }
        #endregion
    }
}
using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReciclaFacil.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministradorController : Controller
    {
        ReciclaFacil_Contexto db;
        public AdministradorController()
        {
            db = new ReciclaFacil_Contexto();
        }

        // GET: Administrador
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Materiais()
        {
            List<Materiais> materiais = db.Materiais.ToList();
            return View(materiais);
        }

        public ActionResult CriarMaterial()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CriarMaterial(Materiais material)
        {
            material.selecionado = false;
            if (ModelState.IsValid)
            {
                db.Materiais.Add(material);
                db.SaveChanges();

                return RedirectToAction("Materiais", "Administrador");
            }

            return View(material);
        }

        public ActionResult DetalheMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Materiais m = db.Materiais.Find(id);

            if (m == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            return View(m);
        }

        public ActionResult EditarMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Materiais m = db.Materiais.Find(id);

            if (m == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMaterial(Materiais model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Materiais", "Administrador");
            }

            return View(model);
        }

        public ActionResult DeleteMaterial(int? id)
        {
            if (id == null)
            {
                string mensagem = "Id do material não informado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }

            Materiais m = db.Materiais.Find(id);

            if (m == null)
            {
                string mensagem = "Material não encontrado!";
                return RedirectToAction("Erro", "Home", new { Mensagem = mensagem });
            }
            return View(m);
        }

        [HttpPost, ActionName("DeleteMaterial")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletaMaterial(int id)
        {
            Materiais m = db.Materiais.Find(id);

            db.Materiais.Remove(m);
            db.SaveChanges();

            return RedirectToAction("Materiais", "Administrador");
        }

    }
}
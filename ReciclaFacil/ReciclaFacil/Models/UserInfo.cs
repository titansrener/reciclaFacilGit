using ReciclaFacil.Models.Entities_RF;
using System;
using System.Linq;


namespace ReciclaFacil.Models
{
    public class UserInfo
    {   
        private ReciclaFacil_Contexto db = new ReciclaFacil_Contexto();

        public string getNome(string id)
        {
            UsuarioRole ur = db.UsuarioRole.SingleOrDefault(x => x.UserId == id);

            if (ur != null)
            {
                switch (ur.RoleId)
                {
                    case "0":
                        return "Administrador";
                    case "1":
                        return db.Clientes.Find(id).nome;

                    case "2":
                        return db.Cooperativas.Find(id).razaoSocial;

                    case "3":
                        return db.Empresas.Find(id).razaoSocial;

                    case "4":
                        return db.Funcionarios.Find(id).nome;

                    default:
                        return "Usuário";
                }
            }
            return "Usuário";
        }


        public string getRoles(string id)
        {
            UsuarioRole ur = db.UsuarioRole.SingleOrDefault(x => x.UserId == id);

            return ur != null && ur.RoleId == "0" ? "Admin" :
                   ur != null && ur.RoleId == "1" ? "Cliente" : 
                   ur != null && ur.RoleId == "2" ? "Cooperativa" : 
                   ur != null && ur.RoleId == "3" ? "Empresa" :
                   ur != null && ur.RoleId == "4" ? "Funcionario" :
                   " ";
        }

        public string getEmail(string id)
        {
            try
            {
                return db.Usuarios.Find(id).Email; 
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string getTipoCliente(string id)
        {
            try
            {
                return db.Clientes.Find(id).tipo;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
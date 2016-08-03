using ReciclaFacil.Models.Entities_RF;
using System.Collections.Generic;

namespace ReciclaFacil.Models
{
    public class DetalheClienteViewModel
    {
        public Clientes cliente { get; set; }

        public ClientesColetas clienteColeta { get; set; }

        public Coletas coleta { get; set; }

        public List<MateriaisColetados> materiais { get; set; }
    }

    public class RotasViewModel
    {
        public Coordenada cooperativaCoord;

        public Coordenada[] clientesCoord;

        public bool faltaCliente;
    }

    public class Coordenada
    {
        public string latitude { get; set; }

        public string longitude { get; set; }
    }
    
}
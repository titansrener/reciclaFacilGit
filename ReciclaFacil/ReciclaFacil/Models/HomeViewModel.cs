using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReciclaFacil.Models
{
    public class MapaCooperativaViewModel
    {
        public CooperativaMapa[] cooperativas { get; set; }
    }

    public class CooperativaMapa
    {
        public string url { get; set; }

        public string latitude { get; set; }

        public string longitude { get; set; }

        public string nome { get; set; }
    }


}
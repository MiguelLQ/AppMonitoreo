using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMonitoreo.Models
{
    public class Ubicacion
    {
        public string Id { get; set; } // se asigna luego del post
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}

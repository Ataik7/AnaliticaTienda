using AnaliticaTienda.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaliticaTienda
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ProductoId { get; set; }
        public int Unidades { get; set; }
        public decimal DescuentoPct { get; set; } // 0..100
        public MetodoPago MetodoPago { get; set; }
        public string Ciudad { get; set; }
        public string Vendedor { get; set; }
    }
}

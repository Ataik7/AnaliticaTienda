using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaliticaTienda.Modelos
{
    public class VentaDetalle
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string Categoria { get; set; }

        public int Unidades { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }

        public decimal DescuentoPct { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string Ciudad { get; set; }
        public string Vendedor { get; set; }

        // Campos calculados 

        public decimal Subtotal
        {
            get
            {
                return Unidades * PrecioVenta;
            }
        }

        public decimal ImporteDescuento
        {
            get
            {
                return Subtotal * (DescuentoPct / 100m);
            }
        }

        public decimal TotalVenta
        {
            get
            {
                return Subtotal - ImporteDescuento;
            }
        }

        public decimal Coste
        {
            get
            {
                return Unidades * PrecioCompra;
            }
        }

        public decimal Beneficio
        {
            get
            {
                return TotalVenta - Coste;
            }
        }
    }
}

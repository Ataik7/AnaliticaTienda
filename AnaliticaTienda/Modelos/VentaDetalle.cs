using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaliticaTienda.Modelos
{
    // Vista detallada de una venta (Venta + datos del Producto). Es útil para tablas y gráficos
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

        // Unidades * PrecioVenta
        public decimal Subtotal
        {
            get
            {
                return Unidades * PrecioVenta;
            }
        }

        // Subtotal * % descuento
        public decimal ImporteDescuento
        {
            get
            {
                return Subtotal * (DescuentoPct / 100m);
            }
        }

        // Subtotal - descuento
        public decimal TotalVenta
        {
            get
            {
                return Subtotal - ImporteDescuento;
            }
        }

        // Unidades * PrecioCompra
        public decimal Coste
        {
            get
            {
                return Unidades * PrecioCompra;
            }
        }

        // TotalVenta - Coste
        public decimal Beneficio
        {
            get
            {
                return TotalVenta - Coste;
            }
        }
    }
}

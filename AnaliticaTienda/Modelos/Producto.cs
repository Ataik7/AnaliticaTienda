using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaliticaTienda
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Proveedor { get; set; }

        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }

        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }

        // Campos calculados (para tablas)
        public decimal MargenUnitario
        {
            get
            {
                return PrecioVenta - PrecioCompra;
            }
        }
        public decimal MargenPct
        {
            get
            {
                return PrecioVenta == 0
                    ? 0
                    : (MargenUnitario / PrecioVenta) * 100m;
            }
        }
        public decimal ValorStockVenta
        {
            get
            {
                return Stock * PrecioVenta;
            }
        }
    }
}

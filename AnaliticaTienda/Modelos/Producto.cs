using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaliticaTienda.Modelos
{
    // Producto del catálogo (para análisis de stock, márgenes y ventas)
    [Serializable]
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

        // Margen por unidad (venta - compra)
        public decimal MargenUnitario
        {
            get
            {
                return PrecioVenta - PrecioCompra;
            }
        }
        // Margen en % sobre el precio de venta
        public decimal MargenPct
        {
            get
            {
                return PrecioVenta == 0
                    ? 0
                    : (MargenUnitario / PrecioVenta) * 100m;
            }
        }
        // Valor del stock a precio de venta
        public decimal ValorStockVenta
        {
            get
            {
                return Stock * PrecioVenta;
            }
        }
    }
}

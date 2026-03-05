using System;
using System.Collections.Generic;
using System.Linq;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    // Genera datos de ejemplo (mínimo 50) para que la app siempre tenga contenido.
    public static class ServicioDatosIniciales
    {
        public static List<Producto> GenerarProductos(int cantidad)
        {
            var rnd = new Random(12345);

            string[] categorias = { "Bebidas", "Snacks", "Hogar", "Tecnologia", "Limpieza", "Moda", "Papeleria", "Salud" };
            string[] proveedores = { "Proveedor Norte", "Proveedor Centro", "Proveedor Sur", "Distribuciones Sol", "GlobalTrade", "IberSupply" };

            var productos = new List<Producto>(cantidad);

            for (int i = 1; i <= cantidad; i++)
            {
                var categoria = categorias[rnd.Next(categorias.Length)];
                var proveedor = proveedores[rnd.Next(proveedores.Length)];

                var precioCompra = Math.Round((decimal)(rnd.NextDouble() * 79.0 + 1.0), 2);
                var margen = (decimal)(rnd.NextDouble() * 0.50 + 0.10);
                var precioVenta = Math.Round(precioCompra * (1m + margen), 2);

                productos.Add(new Producto
                {
                    Id = i,
                    Nombre = $"Producto {i:000}",
                    Categoria = categoria,
                    Proveedor = proveedor,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    Stock = rnd.Next(0, 250),
                    FechaAlta = DateTime.Today.AddDays(-rnd.Next(0, 365 * 2)),
                    Activo = rnd.Next(0, 100) >= 10
                });
            }

            return productos;
        }

        public static List<Venta> GenerarVentas(int cantidad, IReadOnlyList<Producto> productos)
        {
            if (productos == null || productos.Count == 0)
                throw new ArgumentException("Se necesitan productos para generar ventas.", nameof(productos));

            var rnd = new Random(54321);

            string[] ciudades = { "Madrid", "Barcelona", "Valencia", "Sevilla", "Bilbao", "Zaragoza", "Malaga", "Valladolid" };
            string[] vendedores = { "Ana", "Pablo", "Ivan", "Lucia", "Marta", "Sergio", "Carlos", "Noelia" };
            var metodos = Enum.GetValues(typeof(MetodoPago)).Cast<MetodoPago>().ToArray();

            var ventas = new List<Venta>(cantidad);

            for (int i = 1; i <= cantidad; i++)
            {
                var producto = productos[rnd.Next(productos.Count)];

                ventas.Add(new Venta
                {
                    Id = i,
                    Fecha = DateTime.Today.AddDays(-rnd.Next(0, 180)).AddMinutes(rnd.Next(0, 24 * 60)),
                    ProductoId = producto.Id,
                    Unidades = rnd.Next(1, 8),
                    DescuentoPct = Math.Round((decimal)(rnd.NextDouble() * 25.0), 2),
                    MetodoPago = metodos[rnd.Next(metodos.Length)],
                    Ciudad = ciudades[rnd.Next(ciudades.Length)],
                    Vendedor = vendedores[rnd.Next(vendedores.Length)]
                });
            }

            return ventas;
        }

        // Une Venta + Producto para sacar columnas y cálculos (TotalVenta, Beneficio...) en tablas.
        public static List<VentaDetalle> ConstruirVentasDetalle(IReadOnlyList<Venta> ventas, IReadOnlyDictionary<int, Producto> productosPorId)
        {
            var detalles = new List<VentaDetalle>(ventas.Count);

            foreach (var v in ventas)
            {
                if (!productosPorId.TryGetValue(v.ProductoId, out var p))
                    continue;

                detalles.Add(new VentaDetalle
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ProductoId = v.ProductoId,
                    ProductoNombre = p.Nombre,
                    Categoria = p.Categoria,
                    Unidades = v.Unidades,
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta,
                    DescuentoPct = v.DescuentoPct,
                    MetodoPago = v.MetodoPago,
                    Ciudad = v.Ciudad,
                    Vendedor = v.Vendedor
                });
            }

            return detalles;
        }
    }
}
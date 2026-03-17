using System;
using System.Collections.Generic;
using System.Linq;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    public sealed class InformeDashboardService
    {
        public sealed class FiltrosInforme
        {
            public string Categoria { get; set; } = "Todas";
            public int StockMinimo { get; set; } = 0;
        }

        public sealed class ResultadoInforme
        {
            // TABLAS
            public object HistoricoVentas { get; set; }
            public object MetricasPorCategoria { get; set; }
            public object Inventario { get; set; }
            public object TopRentables { get; set; }
            public object Vendedores { get; set; }
            public object Pagos { get; set; }
            public object Ciudades { get; set; }
            public object CosteVsIngresoCategoria { get; set; }

            // SERIES (para gráficos)
            public List<(string X, decimal Y)> EvolucionVentas { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> DistribucionIngresosCategoria { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> StockPorCategoria { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> TopProductosBeneficio { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> BeneficioPorVendedor { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> ImportePorMetodoPago { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> IngresosPorCiudad { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> CostePorCategoria { get; set; } = new List<(string X, decimal Y)>();
            public List<(string X, decimal Y)> IngresosPorCategoria { get; set; } = new List<(string X, decimal Y)>();
        }

        public ResultadoInforme Generar(
            IReadOnlyList<Producto> productos,
            IReadOnlyList<VentaDetalle> ventasDetalle,
            FiltrosInforme filtros)
        {
            var res = new ResultadoInforme();

            // --- TAB 1: Historico + Metricas ---
            var ventasDetalleFiltradas = AplicarFiltroCategoria(ventasDetalle, filtros.Categoria);

            res.HistoricoVentas = ventasDetalleFiltradas
                .OrderByDescending(v => v.Fecha)
                .ToList();

            var metricas = ventasDetalleFiltradas
                .GroupBy(v => v.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    TotalUnidades = g.Sum(x => x.Unidades),
                    TotalIngresos = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    BeneficioTotal = Math.Round(g.Sum(x => x.Beneficio), 2)
                })
                .OrderByDescending(x => x.TotalIngresos)
                .ToList();

            res.MetricasPorCategoria = metricas;

            // Series chart 1 (evolución)
            var ventasPorDia = ventasDetalleFiltradas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.TotalVenta) })
                .OrderBy(x => x.Fecha)
                .ToList();

            res.EvolucionVentas = ventasPorDia
                .Select(x => (x.Fecha.ToShortDateString(), (decimal)x.Total))
                .ToList();

            // Series chart 2 (distribución)
            res.DistribucionIngresosCategoria = metricas
                .Select(m => (m.Categoria, (decimal)m.TotalIngresos))
                .ToList();

            // --- TAB 2: Inventario + Top ---
            res.Inventario = productos
                .Where(p => p.Stock >= filtros.StockMinimo)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Categoria,
                    p.Stock,
                    PrecioVenta = Math.Round(p.PrecioVenta, 2),
                    ValorStockVenta = Math.Round(p.ValorStockVenta, 2)
                })
                .OrderByDescending(x => x.ValorStockVenta)
                .ToList();

            res.TopRentables = productos
                .OrderByDescending(p => p.MargenUnitario)
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Categoria,
                    PrecioCompra = Math.Round(p.PrecioCompra, 2),
                    PrecioVenta = Math.Round(p.PrecioVenta, 2),
                    MargenUnitario = Math.Round(p.MargenUnitario, 2),
                    MargenPct = Math.Round(p.MargenPct, 2).ToString() + "%"
                })
                .ToList();

            var stockCategoria = productos
                .GroupBy(p => p.Categoria)
                .Select(g => new { Categoria = g.Key, StockTotal = g.Sum(x => x.Stock) })
                .OrderByDescending(x => x.StockTotal)
                .ToList();

            res.StockPorCategoria = stockCategoria
                .Select(x => (x.Categoria, (decimal)x.StockTotal))
                .ToList();

            var beneficioProd = ventasDetalle
                .GroupBy(v => v.ProductoNombre)
                .Select(g => new { Producto = g.Key, Beneficio = g.Sum(x => x.Beneficio) })
                .OrderByDescending(x => x.Beneficio)
                .Take(5)
                .ToList();

            res.TopProductosBeneficio = beneficioProd
                .Select(x => (x.Producto, (decimal)Math.Round(x.Beneficio, 2)))
                .ToList();

            // --- TAB 3: Vendedores + Pagos ---
            var vendedores = ventasDetalleFiltradas
                .GroupBy(v => v.Vendedor)
                .Select(g => new
                {
                    Vendedor = g.Key,
                    VentasRealizadas = g.Count(),
                    UnidadesVendidas = g.Sum(x => x.Unidades),
                    TotalFacturado = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    BeneficioGenerado = Math.Round(g.Sum(x => x.Beneficio), 2)
                })
                .OrderByDescending(x => x.BeneficioGenerado)
                .ToList();

            res.Vendedores = vendedores;
            res.BeneficioPorVendedor = vendedores
                .Select(x => (x.Vendedor, (decimal)x.BeneficioGenerado))
                .ToList();

            var pagos = ventasDetalleFiltradas
                .GroupBy(v => v.MetodoPago)
                .Select(g => new
                {
                    MetodoPago = g.Key.ToString(),
                    Transacciones = g.Count(),
                    ImporteTotal = Math.Round(g.Sum(x => x.TotalVenta), 2)
                })
                .OrderByDescending(x => x.ImporteTotal)
                .ToList();

            res.Pagos = pagos;
            res.ImportePorMetodoPago = pagos
                .Select(x => (x.MetodoPago, (decimal)x.ImporteTotal))
                .ToList();

            // --- TAB 4: Ciudades + Coste/Ingreso ---
            var ciudades = ventasDetalleFiltradas
                .GroupBy(v => v.Ciudad)
                .Select(g => new
                {
                    Ciudad = g.Key,
                    NumeroVentas = g.Count(),
                    Ingresos = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    Beneficio = Math.Round(g.Sum(x => x.Beneficio), 2)
                })
                .OrderByDescending(x => x.Ingresos)
                .ToList();

            res.Ciudades = ciudades;
            res.IngresosPorCiudad = ciudades
                .Select(x => (x.Ciudad, (decimal)x.Ingresos))
                .ToList();

            var costIng = ventasDetalleFiltradas
                .GroupBy(v => v.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    CostoVentas = Math.Round(g.Sum(x => x.Coste), 2),
                    IngresosTotales = Math.Round(g.Sum(x => x.TotalVenta), 2)
                })
                .OrderByDescending(x => x.IngresosTotales)
                .ToList();

            res.CosteVsIngresoCategoria = costIng;
            res.CostePorCategoria = costIng.Select(x => (x.Categoria, (decimal)x.CostoVentas)).ToList();
            res.IngresosPorCategoria = costIng.Select(x => (x.Categoria, (decimal)x.IngresosTotales)).ToList();

            return res;
        }

        private static List<VentaDetalle> AplicarFiltroCategoria(IReadOnlyList<VentaDetalle> ventas, string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria) || categoria == "Todas")
                return ventas.ToList();

            return ventas.Where(v => v.Categoria == categoria).ToList();
        }
    }
}
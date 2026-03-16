using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AnaliticaTienda.Modelos;
using AnaliticaTienda.Servicios;

namespace AnaliticaTienda
{
    public partial class Form1 : Form
    {
        private List<Producto> _productos;
        private List<Venta> _ventas;
        private List<VentaDetalle> _ventasDetalle;

        private TabControl _tabControl;

        // Tab 1
        private DataGridView _gridHistoricoVentas;
        private ComboBox _cboCategoriaFiltro;
        private DataGridView _gridMetricasGlobales;
        private Chart _chartEvolucionVentas;
        private Chart _chartDistribucionVentas;

        // Tab 2
        private DataGridView _gridInventario;
        private NumericUpDown _numStockMinimo;
        private DataGridView _gridTopRentables;
        private Chart _chartStockCategoria;
        private Chart _chartTopRentables;

        // Tab 3
        private DataGridView _gridAnalisisVendedor;
        private DataGridView _gridAnalisisMetodoPago;
        private Chart _chartBeneficioVendedor;
        private Chart _chartDistribucionPagos;

        // Tab 4
        private DataGridView _gridRendimientoCiudad;
        private DataGridView _gridCosteIngresoCategoria;
        private Chart _chartVentasCiudad;
        private Chart _chartCosteIngreso;

        public Form1()
        {
            InitializeComponent();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Analítica Tienda - Dashboard Avanzado de Informes";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 768);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarDatos();
            ConstruirUI();
            PoblarFiltrosYDatosGenerales();
        }

        private void CargarDatos()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var formato = FormatoDatos.Json;

            var srvProductos = new Productos(baseDir, formato);
            _productos = srvProductos.CargarOGenerar(50);

            var srvVentas = new Ventas(baseDir, formato);
            _ventas = srvVentas.CargarOGenerar(_productos, 50);

            var productosPorId = _productos.ToDictionary(p => p.Id, p => p);
            _ventasDetalle = DatosIniciales.ConstruirVentasDetalle(_ventas, productosPorId);
        }

        private void PoblarFiltrosYDatosGenerales()
        {
            var categorias = _productos.Select(p => p.Categoria).Distinct().ToList();
            categorias.Insert(0, "Todas");
            _cboCategoriaFiltro.DataSource = categorias;
            _cboCategoriaFiltro.SelectedIndex = 0;
            _cboCategoriaFiltro.SelectedIndexChanged += (s, ev) => ActualizarTab1Filtro();

            _numStockMinimo.ValueChanged += (s, ev) => ActualizarTab2Filtro();

            ActualizarDatosTab1();
            ActualizarTab1Filtro();
            ActualizarTab2Filtro();
            ActualizarDatosTab2();
            ActualizarDatosTab3();
            ActualizarDatosTab4();
        }

        private void ActualizarDatosTab1()
        {
            var metricas = _ventasDetalle.GroupBy(v => v.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    TotalUnidades = g.Sum(x => x.Unidades),
                    TotalIngresos = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    BeneficioTotal = Math.Round(g.Sum(x => x.Beneficio), 2)
                }).ToList();
            _gridMetricasGlobales.DataSource = metricas;

            // Chart 1: Evolucion Ventas (Eje X: Fecha, Eje Y: TotalVenta)
            var ventasPorDia = _ventasDetalle.GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.TotalVenta) })
                .OrderBy(x => x.Fecha).ToList();
            _chartEvolucionVentas.Series[0].Points.Clear();
            foreach (var v in ventasPorDia)
                _chartEvolucionVentas.Series[0].Points.AddXY(v.Fecha.ToShortDateString(), v.Total);

            // Chart 2: Distribucion Ventas (Pie Chart de Ingresos por Categoria)
            _chartDistribucionVentas.Series[0].Points.Clear();
            foreach (var m in metricas)
                _chartDistribucionVentas.Series[0].Points.AddXY(m.Categoria, m.TotalIngresos);
        }

        private void ActualizarTab1Filtro()
        {
            string categoria = _cboCategoriaFiltro.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(categoria) || categoria == "Todas")
                _gridHistoricoVentas.DataSource = _ventasDetalle.OrderByDescending(v => v.Fecha).ToList();
            else
                _gridHistoricoVentas.DataSource = _ventasDetalle.Where(v => v.Categoria == categoria).OrderByDescending(v => v.Fecha).ToList();
        }

        private void ActualizarDatosTab2()
        {
            var topRentables = _productos.OrderByDescending(p => p.PrecioVenta - p.PrecioCompra)
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
                }).ToList();
            _gridTopRentables.DataSource = topRentables;

            // Chart 3: Stock por Categoria
            var stockCategoria = _productos.GroupBy(p => p.Categoria)
                .Select(g => new { Categoria = g.Key, StockTotal = g.Sum(x => x.Stock) }).ToList();
            _chartStockCategoria.Series[0].Points.Clear();
            foreach (var s in stockCategoria)
                _chartStockCategoria.Series[0].Points.AddXY(s.Categoria, s.StockTotal);

            // Chart 4: Top 5 Productos Mayor Beneficio Total Histórico
            var beneficioProd = _ventasDetalle.GroupBy(v => v.ProductoNombre)
                .Select(g => new { Producto = g.Key, Beneficio = g.Sum(x => x.Beneficio) })
                .OrderByDescending(x => x.Beneficio).Take(5).ToList();
            _chartTopRentables.Series[0].Points.Clear();
            foreach (var bp in beneficioProd)
                _chartTopRentables.Series[0].Points.AddXY(bp.Producto, Math.Round(bp.Beneficio, 2));
        }

        private void ActualizarTab2Filtro()
        {
            int stockMinimo = (int)_numStockMinimo.Value;
            var listado = _productos.Where(p => p.Stock >= stockMinimo).Select(p => new
            {
                p.Id,
                p.Nombre,
                p.Categoria,
                p.Stock,
                p.PrecioVenta,
                ValorStockVenta = Math.Round(p.ValorStockVenta, 2)
            }).ToList();
            _gridInventario.DataSource = listado;
        }

        private void ActualizarDatosTab3()
        {
            // Grid 5: Vendedores
            var vendedores = _ventasDetalle.GroupBy(v => v.Vendedor)
                .Select(g => new
                {
                    Vendedor = g.Key,
                    VentasRealizadas = g.Count(),
                    UnidadesVendidas = g.Sum(x => x.Unidades),
                    TotalFacturado = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    BeneficioGenerado = Math.Round(g.Sum(x => x.Beneficio), 2)
                }).ToList();
            _gridAnalisisVendedor.DataSource = vendedores;

            // Grid 6: Metodos de Pago
            var pagos = _ventasDetalle.GroupBy(v => v.MetodoPago)
                .Select(g => new
                {
                    MetodoPago = g.Key.ToString(),
                    Transacciones = g.Count(),
                    ImporteTotal = Math.Round(g.Sum(x => x.TotalVenta), 2)
                }).ToList();
            _gridAnalisisMetodoPago.DataSource = pagos;

            // Chart 5: Beneficio Vendedor
            _chartBeneficioVendedor.Series[0].Points.Clear();
            foreach (var v in vendedores)
                _chartBeneficioVendedor.Series[0].Points.AddXY(v.Vendedor, v.BeneficioGenerado);

            // Chart 6: Distribucion Metodos Pago
            _chartDistribucionPagos.Series[0].Points.Clear();
            foreach (var p in pagos)
                _chartDistribucionPagos.Series[0].Points.AddXY(p.MetodoPago, p.ImporteTotal);
        }

        private void ActualizarDatosTab4()
        {
            // Grid 7: Ciudades
            var ciudades = _ventasDetalle.GroupBy(v => v.Ciudad)
                .Select(g => new
                {
                    Ciudad = g.Key,
                    NumeroVentas = g.Count(),
                    Ingresos = Math.Round(g.Sum(x => x.TotalVenta), 2),
                    Beneficio = Math.Round(g.Sum(x => x.Beneficio), 2)
                }).ToList();
            _gridRendimientoCiudad.DataSource = ciudades;

            // Grid 8: Costos vs Ingresos Categoria
            var costIngCol = _ventasDetalle.GroupBy(v => v.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    CostoVentas = Math.Round(g.Sum(x => x.Coste), 2),
                    IngresosTotales = Math.Round(g.Sum(x => x.TotalVenta), 2)
                }).ToList();
            _gridCosteIngresoCategoria.DataSource = costIngCol;

            // Chart 7: Ventas por Ciudad
            _chartVentasCiudad.Series[0].Points.Clear();
            foreach (var c in ciudades)
                _chartVentasCiudad.Series[0].Points.AddXY(c.Ciudad, c.Ingresos);

            // Chart 8: Comparativa Costo/Ingreso
            _chartCosteIngreso.Series[0].Points.Clear();
            _chartCosteIngreso.Series[1].Points.Clear();
            foreach (var ci in costIngCol)
            {
                _chartCosteIngreso.Series[0].Points.AddXY(ci.Categoria, ci.CostoVentas);
                _chartCosteIngreso.Series[1].Points.AddXY(ci.Categoria, ci.IngresosTotales);
            }
        }

        // --- Helper de UI ---
        private void ConstruirUI()
        {
            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Regular) };

            // ---------------- TAB 1 ----------------
            var tab1 = new TabPage("1. Visión General ");
            var split1Vertical = CrearSplit(Orientation.Vertical);
            var split1Izq = CrearSplit(Orientation.Horizontal);
            var split1Der = CrearSplit(Orientation.Horizontal);

            _gridHistoricoVentas = CrearGrid();
            _cboCategoriaFiltro = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            var panelGrid1 = CrearContenedorFiltro("Tabla 1: Histórico de Ventas (Filtro Categoría)", "Filtrar Categoría:", _cboCategoriaFiltro, _gridHistoricoVentas);
            
            _gridMetricasGlobales = CrearGrid();
            var gbMeticas = CrearGroupBox("Tabla 2: Métricas Globales por Categoría", _gridMetricasGlobales);

            _chartEvolucionVentas = CrearGrafico(SeriesChartType.Line, "Evolución Ventas Diario");
            var gbChart1 = CrearGroupBox("Gráfico 1: Evolución General", _chartEvolucionVentas);

            _chartDistribucionVentas = CrearGrafico(SeriesChartType.Doughnut, "Ingresos por Categoría");
            var gbChart2 = CrearGroupBox("Gráfico 2: Distribución de Ingresos", _chartDistribucionVentas);

            split1Izq.Panel1.Controls.Add(panelGrid1);
            split1Izq.Panel2.Controls.Add(gbMeticas);
            split1Der.Panel1.Controls.Add(gbChart1);
            split1Der.Panel2.Controls.Add(gbChart2);

            split1Vertical.Panel1.Controls.Add(split1Izq);
            split1Vertical.Panel2.Controls.Add(split1Der);
            tab1.Controls.Add(split1Vertical);

            // ---------------- TAB 2 ----------------
            var tab2 = new TabPage("2. Inventario ");
            var split2V = CrearSplit(Orientation.Vertical);
            var split2Izq = CrearSplit(Orientation.Horizontal);
            var split2Der = CrearSplit(Orientation.Horizontal);

            _gridInventario = CrearGrid();
            _numStockMinimo = new NumericUpDown { Width = 100, Minimum = 0, Maximum = 1000, Value = 0 };
            var panelGrid3 = CrearContenedorFiltro("Tabla 3: Inventario Actual", "Stock Mínimo:", _numStockMinimo, _gridInventario);

            _gridTopRentables = CrearGrid();
            var gbGrid4 = CrearGroupBox("Tabla 4: Top 10 Productos Más Rentables", _gridTopRentables);

            _chartStockCategoria = CrearGrafico(SeriesChartType.Bar, "Stock Total");
            var gbChart3 = CrearGroupBox("Gráfico 3: Nivel de Stock por Categoría", _chartStockCategoria);

            _chartTopRentables = CrearGrafico(SeriesChartType.Column, "Beneficio Absoluto");
            var gbChart4 = CrearGroupBox("Gráfico 4: Top 5 Productos con más Beneficios", _chartTopRentables);

            split2Izq.Panel1.Controls.Add(panelGrid3);
            split2Izq.Panel2.Controls.Add(gbGrid4);
            split2Der.Panel1.Controls.Add(gbChart3);
            split2Der.Panel2.Controls.Add(gbChart4);

            split2V.Panel1.Controls.Add(split2Izq);
            split2V.Panel2.Controls.Add(split2Der);
            tab2.Controls.Add(split2V);

            // ---------------- TAB 3 ----------------
            var tab3 = new TabPage("3. RRHH y Pagos ");
            var split3V = CrearSplit(Orientation.Vertical);
            var split3Izq = CrearSplit(Orientation.Horizontal);
            var split3Der = CrearSplit(Orientation.Horizontal);

            _gridAnalisisVendedor = CrearGrid();
            var gbGrid5 = CrearGroupBox("Tabla 5: Análisis por Vendedor", _gridAnalisisVendedor);

            _gridAnalisisMetodoPago = CrearGrid();
            var gbGrid6 = CrearGroupBox("Tabla 6: Análisis de Métodos de Pago", _gridAnalisisMetodoPago);

            _chartBeneficioVendedor = CrearGrafico(SeriesChartType.Column, "Beneficio Generado");
            var gbChart5 = CrearGroupBox("Gráfico 5: Aporte Promedio/Total Vendedor", _chartBeneficioVendedor);

            _chartDistribucionPagos = CrearGrafico(SeriesChartType.Pie, "Importe Total");
            var gbChart6 = CrearGroupBox("Gráfico 6: Importe por Medio de Pago", _chartDistribucionPagos);

            split3Izq.Panel1.Controls.Add(gbGrid5);
            split3Izq.Panel2.Controls.Add(gbGrid6);
            split3Der.Panel1.Controls.Add(gbChart5);
            split3Der.Panel2.Controls.Add(gbChart6);

            split3V.Panel1.Controls.Add(split3Izq);
            split3V.Panel2.Controls.Add(split3Der);
            tab3.Controls.Add(split3V);

            // ---------------- TAB 4 ----------------
            var tab4 = new TabPage("4. Costos y Geografía ");
            var split4V = CrearSplit(Orientation.Vertical);
            var split4Izq = CrearSplit(Orientation.Horizontal);
            var split4Der = CrearSplit(Orientation.Horizontal);

            _gridRendimientoCiudad = CrearGrid();
            var gbGrid7 = CrearGroupBox("Tabla 7: Rendimiento por Ciudad", _gridRendimientoCiudad);

            _gridCosteIngresoCategoria = CrearGrid();
            var gbGrid8 = CrearGroupBox("Tabla 8: Costos e Ingresos por Categoría", _gridCosteIngresoCategoria);

            _chartVentasCiudad = CrearGrafico(SeriesChartType.Bar, "Ingresos");
            var gbChart7 = CrearGroupBox("Gráfico 7: Ingresos Totales por Ciudad", _chartVentasCiudad);

            _chartCosteIngreso = CrearGrafico(SeriesChartType.Column, "Costos");
            var serie2 = new Series("Ingresos") { ChartType = SeriesChartType.Column, IsValueShownAsLabel = true };
            _chartCosteIngreso.Series.Add(serie2);
            var gbChart8 = CrearGroupBox("Gráfico 8: Comparativa Costos vs Ingresos", _chartCosteIngreso);

            split4Izq.Panel1.Controls.Add(gbGrid7);
            split4Izq.Panel2.Controls.Add(gbGrid8);
            split4Der.Panel1.Controls.Add(gbChart7);
            split4Der.Panel2.Controls.Add(gbChart8);

            split4V.Panel1.Controls.Add(split4Izq);
            split4V.Panel2.Controls.Add(split4Der);
            tab4.Controls.Add(split4V);


            _tabControl.TabPages.Add(tab1);
            _tabControl.TabPages.Add(tab2);
            _tabControl.TabPages.Add(tab3);
            _tabControl.TabPages.Add(tab4);

            this.Controls.Add(_tabControl);
        }

        private SplitContainer CrearSplit(Orientation orientation)
        {
            return new SplitContainer { Dock = DockStyle.Fill, Orientation = orientation, BorderStyle = BorderStyle.FixedSingle };
        }

        private DataGridView CrearGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 8.5F)
            };
        }

        private Chart CrearGrafico(SeriesChartType tipo, string nombreSerie)
        {
            var chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea();
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            chart.ChartAreas.Add(area);
            
            var serie = new Series(nombreSerie) 
            { 
                ChartType = tipo, 
                IsValueShownAsLabel = (tipo != SeriesChartType.Line) && (tipo != SeriesChartType.Doughnut) && (tipo != SeriesChartType.Pie),
                Font = new Font("Segoe UI", 8F),
                BorderWidth = 3
            };
            chart.Series.Add(serie);
            
            chart.Legends.Add(new Legend { Docking = Docking.Bottom, Font = new Font("Segoe UI", 8F) });
            return chart;
        }

        private GroupBox CrearGroupBox(string titulo, Control interior)
        {
            var gb = new GroupBox
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Padding(10)
            };
            gb.Controls.Add(interior);
            return gb;
        }

        private GroupBox CrearContenedorFiltro(string titulo, string textoFiltro, Control controlFiltro, DataGridView grid)
        {
            var gb = CrearGroupBox(titulo, grid);
            
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 35 };
            var lbl = new Label { Text = textoFiltro, Location = new Point(0, 5), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            controlFiltro.Location = new Point(130, 2);
            controlFiltro.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            
            topPanel.Controls.Add(lbl);
            topPanel.Controls.Add(controlFiltro);
            
            grid.Dock = DockStyle.Fill;
            gb.Controls.Add(topPanel);
            
            grid.BringToFront();
            return gb;
        }
    }
}
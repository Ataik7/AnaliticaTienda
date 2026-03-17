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

        private readonly InformeDashboardService _informeSrv = new InformeDashboardService();

        private TabControl _tabControl;

        // --- Estructura del informe (cabecera) ---
        private Panel _panelHeader;
        private Label _lblTituloInforme;
        private Label _lblFechaGeneracion;
        private Label _lblPagina;
        private DateTime _fechaGeneracionInforme;

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
            Text = "Analítica Tienda - Dashboard Avanzado de Informes";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1024, 768);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _fechaGeneracionInforme = DateTime.Now;

            CargarDatos();
            ConstruirUI();
            PoblarFiltros();

            RefrescarInforme();
            ActualizarEncabezadoInforme();
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

        private void PoblarFiltros()
        {
            var categorias = _productos.Select(p => p.Categoria).Distinct().OrderBy(x => x).ToList();
            categorias.Insert(0, "Todas");

            _cboCategoriaFiltro.DataSource = categorias;
            _cboCategoriaFiltro.SelectedIndex = 0;

            _cboCategoriaFiltro.SelectedIndexChanged += (s, e) => RefrescarInforme();
            _numStockMinimo.ValueChanged += (s, e) => RefrescarInforme();
        }

        private InformeDashboardService.FiltrosInforme LeerFiltros()
        {
            return new InformeDashboardService.FiltrosInforme
            {
                Categoria = _cboCategoriaFiltro.SelectedItem?.ToString() ?? "Todas",
                StockMinimo = (int)_numStockMinimo.Value
            };
        }

        private void RefrescarInforme()
        {
            var filtros = LeerFiltros();
            var res = _informeSrv.Generar(_productos, _ventasDetalle, filtros);

            // TABLAS
            _gridHistoricoVentas.DataSource = res.HistoricoVentas;
            _gridMetricasGlobales.DataSource = res.MetricasPorCategoria;
            _gridInventario.DataSource = res.Inventario;
            _gridTopRentables.DataSource = res.TopRentables;
            _gridAnalisisVendedor.DataSource = res.Vendedores;
            _gridAnalisisMetodoPago.DataSource = res.Pagos;
            _gridRendimientoCiudad.DataSource = res.Ciudades;
            _gridCosteIngresoCategoria.DataSource = res.CosteVsIngresoCategoria;

            // GRÁFICOS
            PintarSerie(_chartEvolucionVentas, 0, res.EvolucionVentas);
            PintarSerie(_chartDistribucionVentas, 0, res.DistribucionIngresosCategoria);
            PintarSerie(_chartStockCategoria, 0, res.StockPorCategoria);
            PintarSerie(_chartTopRentables, 0, res.TopProductosBeneficio);
            PintarSerie(_chartBeneficioVendedor, 0, res.BeneficioPorVendedor);
            PintarSerie(_chartDistribucionPagos, 0, res.ImportePorMetodoPago);
            PintarSerie(_chartVentasCiudad, 0, res.IngresosPorCiudad);

            // Chart 8 tiene 2 series (Coste e Ingresos)
            PintarSerie(_chartCosteIngreso, 0, res.CostePorCategoria);
            PintarSerie(_chartCosteIngreso, 1, res.IngresosPorCategoria);
        }

        private static void PintarSerie(Chart chart, int serieIndex, List<(string X, decimal Y)> puntos)
        {
            if (chart == null) return;
            if (chart.Series.Count <= serieIndex) return;

            var serie = chart.Series[serieIndex];
            serie.Points.Clear();

            foreach (var (x, y) in puntos)
                serie.Points.AddXY(x, y);
        }

        // --- UI ---
        private void ConstruirUI()
        {
            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Regular) };

            // Cabecera informe
            _panelHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
            _lblTituloInforme = new Label { AutoSize = true, Location = new Point(12, 6), Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            _lblFechaGeneracion = new Label { AutoSize = true, Location = new Point(12, 28), Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            _lblPagina = new Label { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Top = 16, Left = 0, Anchor = AnchorStyles.Top | AnchorStyles.Right };

            _panelHeader.Controls.Add(_lblTituloInforme);
            _panelHeader.Controls.Add(_lblFechaGeneracion);
            _panelHeader.Controls.Add(_lblPagina);

            _lblFechaGeneracion.Text = "Generado: " + _fechaGeneracionInforme.ToString("dd/MM/yyyy HH:mm");

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

            Controls.Add(_tabControl);
            Controls.Add(_panelHeader);

            _tabControl.SelectedIndexChanged += (s, e) => ActualizarEncabezadoInforme();
            Resize += (s, e) => ActualizarEncabezadoInforme();

            ActualizarEncabezadoInforme();
        }

        private void ActualizarEncabezadoInforme()
        {
            if (_tabControl == null || _lblPagina == null || _lblTituloInforme == null) return;

            int total = _tabControl.TabPages.Count;
            int actual = _tabControl.SelectedIndex + 1;

            _lblPagina.Text = $"Página {actual} de {total}";
            _lblPagina.Left = ClientSize.Width - _lblPagina.Width - 20;

            var seccion = _tabControl.SelectedTab?.Text?.Trim();
            _lblTituloInforme.Text = string.IsNullOrWhiteSpace(seccion)
                ? "Analítica Tienda - Informe"
                : $"Analítica Tienda - Informe ({seccion})";
        }

        private SplitContainer CrearSplit(Orientation orientation)
            => new SplitContainer { Dock = DockStyle.Fill, Orientation = orientation, BorderStyle = BorderStyle.FixedSingle };

        private DataGridView CrearGrid()
            => new DataGridView
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
            var lbl = new Label
            {
                Text = textoFiltro,
                Location = new Point(0, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

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
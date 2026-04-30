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
        private readonly FormatoDatos _formatoDatos = FormatoDatos.Json; // Para cambiar a XML o BIN

        private readonly BindingSource _bsHistorico = new BindingSource();
        private readonly BindingSource _bsMetricas = new BindingSource();
        private readonly BindingSource _bsInventario = new BindingSource();
        private readonly BindingSource _bsTopRentables = new BindingSource();
        private readonly BindingSource _bsVendedores = new BindingSource();
        private readonly BindingSource _bsPagos = new BindingSource();
        private readonly BindingSource _bsCiudades = new BindingSource();
        private readonly BindingSource _bsCosteIngreso = new BindingSource();

        private TabControl _tabControl;

        // --- Estructura del informe (cabecera) ---
        private Panel _panelHeader;
        private Label _lblTituloInforme;
        private Label _lblFechaGeneracion;
        private Label _lblPagina;
        private Panel _panelFooter;
        private Label _lblFooter;
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

            var srvProductos = new Productos(baseDir, _formatoDatos);
            _productos = srvProductos.CargarOGenerar(50);

            var srvVentas = new Ventas(baseDir, _formatoDatos);
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
            _numStockMinimo.MouseUp += (s, e) => RefrescarInforme();
            _numStockMinimo.KeyUp += (s, e) => RefrescarInforme();
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

            // TABLAS (BindingSource)
            _bsHistorico.DataSource = res.HistoricoVentas;
            _bsMetricas.DataSource = res.MetricasPorCategoria;
            _bsInventario.DataSource = res.Inventario;
            _bsTopRentables.DataSource = res.TopRentables;
            _bsVendedores.DataSource = res.Vendedores;
            _bsPagos.DataSource = res.Pagos;
            _bsCiudades.DataSource = res.Ciudades;
            _bsCosteIngreso.DataSource = res.CosteVsIngresoCategoria;

            _gridHistoricoVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _gridMetricasGlobales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridTopRentables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridAnalisisVendedor.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridAnalisisMetodoPago.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridRendimientoCiudad.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridCosteIngresoCategoria.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            FormatearTablas();

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

            // Pie informe (footer)
            _panelFooter = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.WhiteSmoke };
            _lblFooter = new Label { AutoSize = true, Location = new Point(12, 6), Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            _panelFooter.Controls.Add(_lblFooter);

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
            Controls.Add(_panelFooter); 
            Controls.Add(_panelHeader);

            // Enlazar grids a BindingSource (para poder reordenar)
            _gridHistoricoVentas.DataSource = _bsHistorico;
            _gridMetricasGlobales.DataSource = _bsMetricas;
            _gridInventario.DataSource = _bsInventario;
            _gridTopRentables.DataSource = _bsTopRentables;
            _gridAnalisisVendedor.DataSource = _bsVendedores;
            _gridAnalisisMetodoPago.DataSource = _bsPagos;
            _gridRendimientoCiudad.DataSource = _bsCiudades;
            _gridCosteIngresoCategoria.DataSource = _bsCosteIngreso;

            // Habilitar orden asc/desc al clickar en cabeceras
            HabilitarOrdenClick(_gridHistoricoVentas, _bsHistorico);
            HabilitarOrdenClick(_gridMetricasGlobales, _bsMetricas);
            HabilitarOrdenClick(_gridInventario, _bsInventario);
            HabilitarOrdenClick(_gridTopRentables, _bsTopRentables);
            HabilitarOrdenClick(_gridAnalisisVendedor, _bsVendedores);
            HabilitarOrdenClick(_gridAnalisisMetodoPago, _bsPagos);
            HabilitarOrdenClick(_gridRendimientoCiudad, _bsCiudades);
            HabilitarOrdenClick(_gridCosteIngresoCategoria, _bsCosteIngreso);

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

            if (_lblFooter != null)
            {
                _lblFooter.Text =
                    $"Grupo: Iván Gastineau Laine & Pablo Nicolás Gallego | " +
                    $"Formato: {_formatoDatos.ToString().ToUpper()} | " +
                    $"Productos: {_productos?.Count ?? 0} | Ventas: {_ventas?.Count ?? 0} | " +
                    $"Fuente: Data/";
            }
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
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                ScrollBars = ScrollBars.Both,

                AllowUserToOrderColumns = true
            };


        private Chart CrearGrafico(SeriesChartType tipo, string nombreSerie)
        {
            var chart = new Chart { Dock = DockStyle.Fill };

            var area = new ChartArea();
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);

            if (tipo != SeriesChartType.Pie && tipo != SeriesChartType.Doughnut)
            {
                area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.Angle = -45;
                area.AxisX.LabelStyle.IsStaggered = true;
            }

            if (tipo == SeriesChartType.Bar)
            {
                area.AxisY.Interval = 1;
            }

            chart.ChartAreas.Add(area);

            var serie = new Series(nombreSerie)
            {
                ChartType = tipo,
                IsValueShownAsLabel = (tipo != SeriesChartType.Line) &&
                                      (tipo != SeriesChartType.Doughnut) &&
                                      (tipo != SeriesChartType.Pie),
                Font = new Font("Segoe UI", 8F),
                BorderWidth = (tipo == SeriesChartType.Line) ? 2 : 3
            };

            if (tipo == SeriesChartType.Pie || tipo == SeriesChartType.Doughnut)
            {
                serie.IsValueShownAsLabel = true;
                serie.Label = "#VALX (#PERCENT{P0})";
                serie["PieLabelStyle"] = "Outside";
                serie["PieLineColor"] = "Gray";
            }
            else if (serie.IsValueShownAsLabel)
            {
                serie.LabelFormat = "N0";
            }

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
            var gb = new GroupBox
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Padding(10)
            };

            var content = new Panel { Dock = DockStyle.Fill };

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 35 };
            var lbl = new Label
            {
                Text = textoFiltro,
                Location = new Point(0, 7),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            controlFiltro.Location = new Point(130, 4);
            controlFiltro.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            topPanel.Controls.Add(lbl);
            topPanel.Controls.Add(controlFiltro);

            grid.Dock = DockStyle.Fill;

            // Orden correcto: Fill primero, luego Top (para que el Top quede arriba)
            content.Controls.Add(grid);
            content.Controls.Add(topPanel);

            gb.Controls.Add(content);
            return gb;
        }

        private void FormatearTablas()
        {
            // ===== RENOMBRAR HEADERS (sin ocultar) =====

            // Histórico
            Renombrar(_gridHistoricoVentas, "Id", "ID");
            Renombrar(_gridHistoricoVentas, "Fecha", "Fecha");
            Renombrar(_gridHistoricoVentas, "ProductoId", "ProdID");
            Renombrar(_gridHistoricoVentas, "ProductoNombre", "Producto");
            Renombrar(_gridHistoricoVentas, "Categoria", "Cat.");
            Renombrar(_gridHistoricoVentas, "Unidades", "Uds");
            Renombrar(_gridHistoricoVentas, "PrecioCompra", "P.Compra");
            Renombrar(_gridHistoricoVentas, "PrecioVenta", "P.Venta");
            Renombrar(_gridHistoricoVentas, "DescuentoPct", "Desc.%");
            Renombrar(_gridHistoricoVentas, "MetodoPago", "Pago");
            Renombrar(_gridHistoricoVentas, "Ciudad", "Ciudad");
            Renombrar(_gridHistoricoVentas, "Vendedor", "Vend.");
            Renombrar(_gridHistoricoVentas, "Subtotal", "Subt.");
            Renombrar(_gridHistoricoVentas, "ImporteDescuento", "Desc.€");
            Renombrar(_gridHistoricoVentas, "TotalVenta", "Total €");
            Renombrar(_gridHistoricoVentas, "Coste", "Coste€");
            Renombrar(_gridHistoricoVentas, "Beneficio", "Ben.€");

            // Métricas por categoría
            Renombrar(_gridMetricasGlobales, "TotalUnidades", "Uds");
            Renombrar(_gridMetricasGlobales, "TotalIngresos", "Ingresos €");
            Renombrar(_gridMetricasGlobales, "BeneficioTotal", "Ben.€");

            // Inventario
            Renombrar(_gridInventario, "Id", "ID");
            Renombrar(_gridInventario, "PrecioVenta", "P.Venta");
            Renombrar(_gridInventario, "ValorStockVenta", "Valor Stock");

            // Top rentables
            Renombrar(_gridTopRentables, "Id", "ID");
            Renombrar(_gridTopRentables, "PrecioCompra", "P.Compra");
            Renombrar(_gridTopRentables, "PrecioVenta", "P.Venta");
            Renombrar(_gridTopRentables, "MargenUnitario", "Margen €");
            Renombrar(_gridTopRentables, "MargenPct", "Margen %");

            // Vendedores
            Renombrar(_gridAnalisisVendedor, "VentasRealizadas", "Ventas");
            Renombrar(_gridAnalisisVendedor, "UnidadesVendidas", "Uds");
            Renombrar(_gridAnalisisVendedor, "TotalFacturado", "Total €");
            Renombrar(_gridAnalisisVendedor, "BeneficioGenerado", "Ben.€");

            // Pagos
            Renombrar(_gridAnalisisMetodoPago, "MetodoPago", "Pago");
            Renombrar(_gridAnalisisMetodoPago, "Transacciones", "Nº");
            Renombrar(_gridAnalisisMetodoPago, "ImporteTotal", "Total €");

            // Ciudades
            Renombrar(_gridRendimientoCiudad, "NumeroVentas", "Ventas");
            Renombrar(_gridRendimientoCiudad, "Ingresos", "Ingresos €");
            Renombrar(_gridRendimientoCiudad, "Beneficio", "Ben.€");

            // Coste vs ingresos
            Renombrar(_gridCosteIngresoCategoria, "CostoVentas", "Coste €");
            Renombrar(_gridCosteIngresoCategoria, "IngresosTotales", "Ingresos €");

            // ===== FORMATOS =====

            // Histórico: fecha con hora
            if (_gridHistoricoVentas?.Columns != null && _gridHistoricoVentas.Columns.Contains("Fecha"))
                _gridHistoricoVentas.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            // Decimales
            FormatearDecimales(_gridHistoricoVentas, "PrecioCompra", "PrecioVenta", "DescuentoPct",
                "Subtotal", "ImporteDescuento", "TotalVenta", "Coste", "Beneficio");

            FormatearDecimales(_gridMetricasGlobales, "TotalIngresos", "BeneficioTotal");

            FormatearDecimales(_gridInventario, "PrecioVenta", "ValorStockVenta");

            FormatearDecimales(_gridTopRentables, "PrecioCompra", "PrecioVenta", "MargenUnitario");

            FormatearDecimales(_gridAnalisisVendedor, "TotalFacturado", "BeneficioGenerado");

            FormatearDecimales(_gridAnalisisMetodoPago, "ImporteTotal");

            FormatearDecimales(_gridRendimientoCiudad, "Ingresos", "Beneficio");

            FormatearDecimales(_gridCosteIngresoCategoria, "CostoVentas", "IngresosTotales");
        }

        private static void FormatearDecimales(DataGridView g, params string[] columnas)
        {
            if (g?.Columns == null || g.Columns.Count == 0) return;

            foreach (var c in columnas)
            {
                if (!g.Columns.Contains(c)) continue;

                g.Columns[c].DefaultCellStyle.Format = "N2";
                g.Columns[c].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private static void Renombrar(DataGridView g, string col, string header)
        {
            if (g?.Columns != null && g.Columns.Contains(col))
                g.Columns[col].HeaderText = header;
        }

        private static void HabilitarOrdenClick(DataGridView grid, BindingSource bs)
        {
            if (grid == null || bs == null) return;

            grid.ColumnHeaderMouseClick += (s, e) =>
            {
                var col = grid.Columns[e.ColumnIndex];
                var prop = col.DataPropertyName;

                if (string.IsNullOrWhiteSpace(prop)) return;
                if (bs.DataSource == null) return;

                // alternar asc/desc por columna
                string key = $"sort:{prop}";
                bool asc = !(grid.Tag is string t && t == key);
                grid.Tag = asc ? key : "";

                var list = ((System.Collections.IEnumerable)bs.DataSource).Cast<object>().ToList();

                object GetPropValue(object x)
                {
                    var pi = x.GetType().GetProperty(prop);
                    return pi?.GetValue(x, null);
                }

                bs.DataSource = asc
                    ? list.OrderBy(GetPropValue).ToList()
                    : list.OrderByDescending(GetPropValue).ToList();
            };
        }
    }
}
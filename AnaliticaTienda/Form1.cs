using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AnaliticaTienda.Modelos;
using AnaliticaTienda.Servicios;

namespace AnaliticaTienda
{
    public partial class Form1 : Form
    {
        // Datos en memoria 
        private List<Producto> _productos;
        private List<Venta> _ventas;
        private List<VentaDetalle> _ventasDetalle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Carpeta donde se ejecuta el EXE
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Formato de persistencia: Json / Xml / Bin
            var formato = FormatoDatos.Json; 

            // 1) Cargar o generar productos y ventas
            var srvProductos = new Productos(baseDir, formato);
            _productos = srvProductos.CargarOGenerar(50);

            var srvVentas = new Ventas(baseDir, formato);
            _ventas = srvVentas.CargarOGenerar(_productos, 50);

            // 2) Construir VentaDetalle (para tablas, filtros y gráficos)
            var productosPorId = _productos.ToDictionary(p => p.Id, p => p);
            _ventasDetalle = DatosIniciales.ConstruirVentasDetalle(_ventas, productosPorId);

            // 3) Verificación rápida
            Text = $"AnaliticaTienda - Productos: {_productos.Count} | Ventas: {_ventas.Count} | Detalle: {_ventasDetalle.Count} ({formato})";

        }
    }
}
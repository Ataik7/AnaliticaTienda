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
        // Datos en memoria (los usarás luego para tablas/gráficos)
        private List<Producto> _productos;
        private List<Venta> _ventas;
        private List<VentaDetalle> _ventasDetalle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Carpeta donde se ejecuta el EXE (bin\Debug\ o bin\Release\)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Elige formato de persistencia: Json / Xml / Bin
            var formato = FormatoDatos.Json; // cambia a FormatoDatos.Xml o FormatoDatos.Bin para probar

            // 1) Cargar o generar (mínimo 50) productos y ventas
            var srvProductos = new Productos(baseDir, formato);
            _productos = srvProductos.CargarOGenerar(50);

            var srvVentas = new Ventas(baseDir, formato);
            _ventas = srvVentas.CargarOGenerar(_productos, 50);

            // 2) Construir VentaDetalle (para tablas, filtros y gráficos)
            var productosPorId = _productos.ToDictionary(p => p.Id, p => p);
            _ventasDetalle = DatosIniciales.ConstruirVentasDetalle(_ventas, productosPorId);

            // 3) Verificación rápida (puedes quitarlo luego)
            Text = $"AnaliticaTienda - Productos: {_productos.Count} | Ventas: {_ventas.Count} | Detalle: {_ventasDetalle.Count} ({formato})";

            // A partir de aquí, lo siguiente del proyecto es:
            // - Mostrar datos en DataGridView
            // - Añadir filtros
            // - Añadir gráficos
        }
    }
}
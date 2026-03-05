using System.Collections.Generic;
using System.IO;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    // Gestiona carga/guardado de ventas en Data/ventas.json
    public class ServicioVentas
    {
        private readonly ServicioAlmacenamientoJson _almacen = new ServicioAlmacenamientoJson();
        private readonly string _rutaFichero;

        public ServicioVentas(string directorioBase)
        {
            _rutaFichero = Path.Combine(directorioBase, "Data", "ventas.json");
        }

        // Carga ventas; si hay < minItems genera datos y los guarda
        public List<Venta> CargarOGenerar(IReadOnlyList<Producto> productos, int minimo = 50)
        {
            var ventas = _almacen.CargarLista<Venta>(_rutaFichero);

            if (ventas.Count < minimo)
            {
                ventas = ServicioDatosIniciales.GenerarVentas(minimo, productos);
                _almacen.GuardarLista(_rutaFichero, ventas);
            }

            return ventas;
        }

        public void Guardar(List<Venta> ventas)
        {
            _almacen.GuardarLista(_rutaFichero, ventas);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    // Gestiona carga/guardado de productos en Data/productos.json.
    public class ServicioProductos
    {
        private readonly ServicioAlmacenamientoJson _almacen = new ServicioAlmacenamientoJson();
        private readonly string _rutaFichero;

        public ServicioProductos(string directorioBase)
        {
            _rutaFichero = Path.Combine(directorioBase, "Data", "productos.json");
        }

        // Carga productos; si hay < minItems genera datos y los guarda.
        public List<Producto> CargarOGenerar(int minimo = 50)
        {
            var productos = _almacen.CargarLista<Producto>(_rutaFichero);

            if (productos.Count < minimo)
            {
                productos = ServicioDatosIniciales.GenerarProductos(minimo);
                _almacen.GuardarLista(_rutaFichero, productos);
            }

            return productos;
        }

        public void Guardar(List<Producto> productos)
        {
            _almacen.GuardarLista(_rutaFichero, productos);
        }
    }
}
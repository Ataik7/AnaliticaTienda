using System.Collections.Generic;
using System.IO;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    // Gestiona carga/guardado de productos en Data/productos.(json|xml|bin)
    public class Productos
    {
        private readonly AlmacenamientoJson _json = new AlmacenamientoJson();
        private readonly AlmacenamientoXML _xml = new AlmacenamientoXML();
        private readonly AlmacenamientoBin _bin = new AlmacenamientoBin();

        private readonly string _rutaBaseData;
        private readonly FormatoDatos _formato;

        public Productos(string directorioBase, FormatoDatos formato)
        {
            _rutaBaseData = Path.Combine(directorioBase, "Data");
            _formato = formato;
        }

        private string RutaProductos()
        {
            var ext = _formato == FormatoDatos.Json ? "json"
                    : _formato == FormatoDatos.Xml ? "xml"
                    : "bin";

            return Path.Combine(_rutaBaseData, $"productos.{ext}");
        }

        // Carga productos; si hay < minimo genera datos y los guarda
        public List<Producto> CargarOGenerar(int minimo = 50)
        {
            var ruta = RutaProductos();

            List<Producto> productos =
                _formato == FormatoDatos.Json ? _json.CargarLista<Producto>(ruta) :
                _formato == FormatoDatos.Xml ? _xml.CargarLista<Producto>(ruta) :
                _bin.CargarLista<Producto>(ruta);

            if (productos.Count < minimo)
            {
                productos = DatosIniciales.GenerarProductos(minimo);
                Guardar(productos);
            }

            return productos;
        }

        public void Guardar(List<Producto> productos)
        {
            var ruta = RutaProductos();

            if (_formato == FormatoDatos.Json) _json.GuardarLista(ruta, productos);
            else if (_formato == FormatoDatos.Xml) _xml.GuardarLista(ruta, productos);
            else _bin.GuardarLista(ruta, productos);
        }
    }
}
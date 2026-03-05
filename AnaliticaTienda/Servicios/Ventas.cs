using System.Collections.Generic;
using System.IO;
using AnaliticaTienda.Modelos;

namespace AnaliticaTienda.Servicios
{
    // Gestiona carga/guardado de ventas en Data/ventas.(json|xml|bin)
    public class Ventas
    {
        private readonly AlmacenamientoJson _json = new AlmacenamientoJson();
        private readonly AlmacenamientoXML _xml = new AlmacenamientoXML();
        private readonly AlmacenamientoBin _bin = new AlmacenamientoBin();

        private readonly string _rutaBaseData;
        private readonly FormatoDatos _formato;

        public Ventas(string directorioBase, FormatoDatos formato)
        {
            _rutaBaseData = Path.Combine(directorioBase, "Data");
            _formato = formato;
        }

        private string RutaVentas()
        {
            var ext = _formato == FormatoDatos.Json ? "json"
                    : _formato == FormatoDatos.Xml ? "xml"
                    : "bin";

            return Path.Combine(_rutaBaseData, $"ventas.{ext}");
        }

        // Carga ventas; si hay < minimo genera datos y los guarda
        public List<Venta> CargarOGenerar(IReadOnlyList<Producto> productos, int minimo = 50)
        {
            var ruta = RutaVentas();

            List<Venta> ventas =
                _formato == FormatoDatos.Json ? _json.CargarLista<Venta>(ruta) :
                _formato == FormatoDatos.Xml ? _xml.CargarLista<Venta>(ruta) :
                _bin.CargarLista<Venta>(ruta);

            if (ventas.Count < minimo)
            {
                ventas = DatosIniciales.GenerarVentas(minimo, productos);
                Guardar(ventas);
            }

            return ventas;
        }

        public void Guardar(List<Venta> ventas)
        {
            var ruta = RutaVentas();

            if (_formato == FormatoDatos.Json) _json.GuardarLista(ruta, ventas);
            else if (_formato == FormatoDatos.Xml) _xml.GuardarLista(ruta, ventas);
            else _bin.GuardarLista(ruta, ventas);
        }
    }
}
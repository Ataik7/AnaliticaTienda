using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AnaliticaTienda.Servicios
{
    // Lee/guarda listas en XML sin romper la app si hay errores de archivo/XML
    public class AlmacenamientoXML
    {
        public List<T> CargarLista<T>(string rutaFichero)
        {
            try
            {
                if (!File.Exists(rutaFichero)) return new List<T>();

                var fi = new FileInfo(rutaFichero);
                if (fi.Length == 0) return new List<T>();

                var serializer = new XmlSerializer(typeof(List<T>));
                using (var fs = File.OpenRead(rutaFichero))
                {
                    return (serializer.Deserialize(fs) as List<T>) ?? new List<T>();
                }
            }
            catch
            {
                return new List<T>();
            }
        }

        public bool IntentarGuardarLista<T>(string rutaFichero, List<T> items, out string error)
        {
            try
            {
                error = null;

                var dir = Path.GetDirectoryName(rutaFichero);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var serializer = new XmlSerializer(typeof(List<T>));
                using (var fs = File.Create(rutaFichero))
                {
                    serializer.Serialize(fs, items ?? new List<T>());
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public void GuardarLista<T>(string rutaFichero, List<T> items)
        {
            IntentarGuardarLista(rutaFichero, items, out _);
        }
    }
}
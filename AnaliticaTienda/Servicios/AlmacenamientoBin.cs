using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnaliticaTienda.Servicios
{
    // Carga/guarda listas en BIN
    public class AlmacenamientoBin
    {
        public List<T> CargarLista<T>(string rutaFichero)
        {
            try
            {
                if (!File.Exists(rutaFichero)) return new List<T>();

                var fi = new FileInfo(rutaFichero);
                if (fi.Length == 0) return new List<T>();

                var formatter = new BinaryFormatter();
                using (var fs = File.OpenRead(rutaFichero))
                {
                    return (formatter.Deserialize(fs) as List<T>) ?? new List<T>();
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

                var formatter = new BinaryFormatter();
                using (var fs = File.Create(rutaFichero))
                {
                    formatter.Serialize(fs, items ?? new List<T>());
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
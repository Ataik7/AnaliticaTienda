using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AnaliticaTienda.Servicios
{
    // Lee/guarda listas en JSON sin romper la app si hay errores de archivo/JSON
    public class AlmacenamientoJson
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateParseHandling = DateParseHandling.DateTime,
            NullValueHandling = NullValueHandling.Include
        };

        // Carga List<T> desde JSON; si falla devuelve lista vacía
        public List<T> CargarLista<T>(string rutaFichero)
        {
            try
            {
                if (!File.Exists(rutaFichero)) return new List<T>();

                var json = File.ReadAllText(rutaFichero);
                if (string.IsNullOrWhiteSpace(json)) return new List<T>();

                return JsonConvert.DeserializeObject<List<T>>(json, _settings) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        // Guarda List<T> en JSON; si falla devuelve false y un mensaje de error
        public bool IntentarGuardarLista<T>(string rutaFichero, List<T> items, out string error)
        {
            try
            {
                error = null;

                var dir = Path.GetDirectoryName(rutaFichero);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(items ?? new List<T>(), _settings);
                File.WriteAllText(rutaFichero, json);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Guardado rápido (ignora el error para no crashear)
        public void GuardarLista<T>(string rutaFichero, List<T> items)
        {
            IntentarGuardarLista(rutaFichero, items, out _);
        }
    }
}
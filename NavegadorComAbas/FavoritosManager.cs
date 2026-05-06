using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NavegadorComAbas
{
    public class FavoritoItem
    {
        public string Url { get; set; } = "";
        public string Titulo { get; set; } = "";
        public DateTime Data { get; set; }
    }

    public static class FavoritosManager
    {
        private static readonly string pastaDados =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MeuNavegador");

        private static readonly string arquivo =
            Path.Combine(pastaDados, "favoritos.json");

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static void GarantirPasta()
        {
            if (!Directory.Exists(pastaDados))
                Directory.CreateDirectory(pastaDados);
        }

        public static List<FavoritoItem> Carregar()
        {
            GarantirPasta();

            if (!File.Exists(arquivo))
                return new List<FavoritoItem>();

            try
            {
                string json = File.ReadAllText(arquivo);
                return JsonSerializer.Deserialize<List<FavoritoItem>>(json) ?? new List<FavoritoItem>();
            }
            catch
            {
                return new List<FavoritoItem>();
            }
        }

        public static bool Adicionar(string url, string titulo)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            GarantirPasta();

            var lista = Carregar();
            bool existe = lista.Any(x => string.Equals(x.Url, url, StringComparison.OrdinalIgnoreCase));

            if (existe)
                return false;

            lista.Add(new FavoritoItem
            {
                Url = url,
                Titulo = string.IsNullOrWhiteSpace(titulo) ? url : titulo,
                Data = DateTime.Now
            });

            lista = lista
                .OrderByDescending(x => x.Data)
                .ToList();

            string json = JsonSerializer.Serialize(lista, jsonOptions);
            File.WriteAllText(arquivo, json);
            return true;
        }

        public static void Limpar()
        {
            GarantirPasta();

            if (File.Exists(arquivo))
                File.Delete(arquivo);
        }
    }
}

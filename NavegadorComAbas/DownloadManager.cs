using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NavegadorComAbas
{
    public class DownloadItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Url { get; set; } = "";
        public string Arquivo { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public static class DownloadManager
    {
        private static readonly string pastaDados =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MeuNavegador");

        private static readonly string arquivo =
            Path.Combine(pastaDados, "downloads.json");

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static void GarantirPasta()
        {
            if (!Directory.Exists(pastaDados))
                Directory.CreateDirectory(pastaDados);
        }

        public static List<DownloadItem> Carregar()
        {
            GarantirPasta();

            if (!File.Exists(arquivo))
                return new List<DownloadItem>();

            try
            {
                string json = File.ReadAllText(arquivo);
                return JsonSerializer.Deserialize<List<DownloadItem>>(json) ?? new List<DownloadItem>();
            }
            catch
            {
                return new List<DownloadItem>();
            }
        }

        public static void SalvarOuAtualizar(DownloadItem item)
        {
            GarantirPasta();

            var lista = Carregar();
            int indice = lista.FindIndex(x => x.Id == item.Id);

            if (indice >= 0)
                lista[indice] = item;
            else
                lista.Add(item);

            lista = lista
                .OrderByDescending(x => x.DataInicio)
                .Take(200)
                .ToList();

            string json = JsonSerializer.Serialize(lista, jsonOptions);
            File.WriteAllText(arquivo, json);
        }

        public static void Limpar()
        {
            GarantirPasta();

            if (File.Exists(arquivo))
                File.Delete(arquivo);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NavegadorComAbas
{
    public class RascunhoItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Titulo { get; set; } = "";
        public string Conteudo { get; set; } = "";
        public DateTime DataAtualizacao { get; set; }
    }

    public static class RascunhoManager
    {
        private static readonly string pastaDados =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MeuNavegador");

        private static readonly string arquivo =
            Path.Combine(pastaDados, "rascunhos.json");

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static void GarantirPasta()
        {
            if (!Directory.Exists(pastaDados))
                Directory.CreateDirectory(pastaDados);
        }

        public static List<RascunhoItem> Carregar()
        {
            GarantirPasta();

            if (!File.Exists(arquivo))
                return new List<RascunhoItem>();

            try
            {
                string json = File.ReadAllText(arquivo);
                return JsonSerializer.Deserialize<List<RascunhoItem>>(json) ?? new List<RascunhoItem>();
            }
            catch
            {
                return new List<RascunhoItem>();
            }
        }

        public static void Salvar(RascunhoItem item)
        {
            GarantirPasta();

            var lista = Carregar();
            int indice = lista.FindIndex(x => x.Id == item.Id);

            item.DataAtualizacao = DateTime.Now;

            if (indice >= 0)
                lista[indice] = item;
            else
                lista.Add(item);

            lista = lista
                .OrderByDescending(x => x.DataAtualizacao)
                .ToList();

            string json = JsonSerializer.Serialize(lista, jsonOptions);
            File.WriteAllText(arquivo, json);
        }

        public static void Excluir(string id)
        {
            GarantirPasta();

            var lista = Carregar()
                .Where(x => x.Id != id)
                .ToList();

            string json = JsonSerializer.Serialize(lista, jsonOptions);
            File.WriteAllText(arquivo, json);
        }
    }
}

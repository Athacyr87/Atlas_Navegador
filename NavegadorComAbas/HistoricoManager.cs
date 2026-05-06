using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NavegadorComAbas
{
    public class HistoricoItem
    {
        public string Url { get; set; }
        public string Titulo { get; set; }
        public DateTime Data { get; set; }
    }

    public static class HistoricoManager
    {
        private static readonly string pastaHistorico =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MeuNavegador");

        private static readonly string arquivo =
            Path.Combine(pastaHistorico, "historico.txt");

        private static readonly string arquivoLegado =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "historico.txt");

        private static void GarantirPasta()
        {
            if (!Directory.Exists(pastaHistorico))
                Directory.CreateDirectory(pastaHistorico);
        }

        private static void MigrarArquivoLegadoSeNecessario()
        {
            GarantirPasta();

            if (!File.Exists(arquivo) && File.Exists(arquivoLegado))
                File.Move(arquivoLegado, arquivo);
        }

        public static List<HistoricoItem> Carregar()
        {
            var lista = new List<HistoricoItem>();

            MigrarArquivoLegadoSeNecessario();

            if (!File.Exists(arquivo))
                return lista;

            foreach (var linha in File.ReadAllLines(arquivo))
            {
                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                var partes = linha.Split('|');

                if (partes.Length < 3)
                    continue;

                DateTime data;
                if (!DateTime.TryParse(partes[2], out data))
                    data = DateTime.Now;

                lista.Add(new HistoricoItem
                {
                    Url = partes[0],
                    Titulo = string.IsNullOrWhiteSpace(partes[1]) ? partes[0] : partes[1],
                    Data = data
                });
            }

            return lista
                .OrderByDescending(x => x.Data)
                .ToList();
        }

        public static void Salvar(string url, string titulo)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http"))
                return;

            MigrarArquivoLegadoSeNecessario();

            titulo = (titulo ?? "").Replace("|", "-");

            List<string> linhas = File.Exists(arquivo)
                ? File.ReadAllLines(arquivo).ToList()
                : new List<string>();

            linhas.Add($"{url}|{titulo}|{DateTime.Now}");

            if (linhas.Count > 200)
                linhas = linhas.Skip(linhas.Count - 200).ToList();

            File.WriteAllLines(arquivo, linhas);
        }

        public static void Limpar()
        {
            MigrarArquivoLegadoSeNecessario();

            if (File.Exists(arquivo))
                File.Delete(arquivo);
        }
    }
}

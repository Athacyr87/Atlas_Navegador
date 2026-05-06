using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Reflection;

namespace NavegadorComAbas
{
    public partial class Form1 : Form
    {
        private sealed class EditorCodigoInfo
        {
            public string Origem { get; set; } = "";
        }

        private sealed class RascunhoTabState
        {
            public ListBox ListaRascunhos { get; set; }
            public TextBox TxtTitulo { get; set; }
            public WebView2 TxtConteudo { get; set; }
            public Label LblStatus { get; set; }
            public string RascunhoAtualId { get; set; }
            public bool EditorPronto { get; set; }
            public string ConteudoPendente { get; set; } = "";
        }

        private static readonly string pastaDadosApp =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MeuNavegador");

        // Animação suave as deslizar as abas
        private System.Windows.Forms.Timer animacaoTimer;
        private int indiceDestino = -1;
        private float animProgress = 0f;

        // Arrastar Abas
        private int abaArrastando = -1;

        // Menu nas abas novas
        private ContextMenuStrip menuAba;
        private int indiceAbaClicada = -1;

        // Aperdar botão direito e fazer abrir em uma aba
        private ContextMenuStrip menuLink;
        private string linkClicado = "";
        private bool alternarSelecaoTxtUrl = true;
        private bool modoLightAtivo;

        // Fazer busca na URL
        private bool PareceUrl(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            texto = texto.Trim();

            if (texto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                texto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return true;

            if (texto.Contains(" "))
                return false;

            if (texto.Contains(".") && !texto.StartsWith("."))
                return true;

            if (texto.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                texto.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private string GerarHtmlHistorico()
        {
            var lista = HistoricoManager.Carregar();
            if (lista == null)
                lista = new System.Collections.Generic.List<HistoricoItem>();

            var html = new StringBuilder();


            html.AppendLine("<html><head><meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("html, body { height: 100%; }");
            html.AppendLine("body { font-family: Segoe UI; background-color: #1e1e1e; color: white; padding: 20px; position: relative; overflow-x: hidden; }");
            html.AppendLine(".fundo-inicio { position: fixed; inset: 0; width: 100%; height: 100%; object-fit: contain; object-position: center center; opacity: 0.55; z-index: 0; pointer-events: none; }");
            html.AppendLine(".camada-conteudo { position: relative; z-index: 1; }");
            html.AppendLine("h1 { margin-bottom: 20px; }");
            html.AppendLine(".topo { margin-bottom: 25px; }");
            html.AppendLine(".busca-box { display: flex; gap: 10px; margin-bottom: 20px; }");
            html.AppendLine(".busca-input { flex: 1; padding: 12px; border-radius: 10px; border: 1px solid #444; background: #2d2d2d; color: white; font-size: 15px; outline: none; }");
            html.AppendLine(".busca-input:focus { border-color: #4fc3f7; }");
            html.AppendLine(".btn { padding: 10px 16px; border: none; border-radius: 10px; cursor: pointer; font-size: 14px; }");
            html.AppendLine(".btn-buscar { background: #4fc3f7; color: black; font-weight: bold; }");
            html.AppendLine(".btn-limpar { background: #c62828; color: white; margin-bottom: 20px; }");
            html.AppendLine(".item { padding: 12px; margin-bottom: 10px; background: #2d2d2d; border-radius: 10px; transition: 0.2s; }");
            html.AppendLine(".item:hover { background: #3a3a3a; }");
            html.AppendLine(".titulo { font-size: 16px; font-weight: bold; color: #4fc3f7; }");
            html.AppendLine(".url { font-size: 12px; color: #aaa; word-break: break-all; }");
            html.AppendLine(".data { font-size: 11px; color: #777; margin-top: 4px; }");
            html.AppendLine("a { text-decoration: none; }");
            html.AppendLine("</style>");

            html.AppendLine("<script>");
            html.AppendLine("function pareceUrl(texto) {");
            html.AppendLine("  texto = texto.trim();");
            html.AppendLine("  if (texto.startsWith('http://') || texto.startsWith('https://')) return true;");
            html.AppendLine("  if (texto.includes(' ') || texto.length === 0) return false;");
            html.AppendLine("  if (texto.includes('.') && !texto.startsWith('.')) return true;");
            html.AppendLine("  return false;");
            html.AppendLine("}");

            html.AppendLine("function buscarOuAbrir() {");
            html.AppendLine("  var texto = document.getElementById('txtBusca').value.trim();");
            html.AppendLine("  if (texto.length === 0) return;");
            html.AppendLine("");
            html.AppendLine("  if (pareceUrl(texto)) {");
            html.AppendLine("    window.chrome.webview.postMessage('abrir:' + texto);");
            html.AppendLine("  } else {");
            html.AppendLine("    window.chrome.webview.postMessage('buscar:' + texto);");
            html.AppendLine("  }");
            html.AppendLine("}");

            html.AppendLine("function teclaBusca(event) {");
            html.AppendLine("  if (event.key === 'Enter') {");
            html.AppendLine("    buscarOuAbrir();");
            html.AppendLine("  }");
            html.AppendLine("}");
            html.AppendLine("</script>");

            html.AppendLine("</head><body>");
            html.AppendLine($"<img class='fundo-inicio' src='{ObterImagemDataUri("rrt1.jpg")}' alt='' />");
            html.AppendLine("<div class='camada-conteudo'>");

            html.AppendLine("<div class='topo'>");
            html.AppendLine("<h1>Início</h1>");
            html.AppendLine("<div class='busca-box'>");
            html.AppendLine("<input id='txtBusca' class='busca-input' type='text' placeholder='Pesquisar no Google...' onkeydown='teclaBusca(event)' />");
            html.AppendLine("<button class='btn btn-buscar' onclick='buscarOuAbrir()'>Ir</button>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("<button class='btn btn-limpar' onclick=\"window.chrome.webview.postMessage('limpar')\">Limpar</button>");

            html.AppendLine("<h2>Histórico</h2>");

            if (lista.Count == 0)
            {
                html.AppendLine("<p>Sem histórico ainda...</p>");
            }
            else
            {
                foreach (var item in lista)
                {
                    string titulo = System.Net.WebUtility.HtmlEncode(item.Titulo ?? "");
                    string url = System.Net.WebUtility.HtmlEncode(item.Url ?? "");
                    string urlJs = JsonSerializer.Serialize(item.Url ?? "");
                    string data = System.Net.WebUtility.HtmlEncode(item.Data.ToString("dd/MM/yyyy HH:mm"));

                    html.AppendLine($@"
                        <div class='item'>
                            <a href='#' onclick='window.chrome.webview.postMessage(""abrir:"" + {urlJs}); return false;'>
                                <div class='titulo'>{titulo}</div>
                                <div class='url'>{url}</div>
                                <div class='data'>{data}</div>
                            </a>
                        </div>");
                }
            }

            html.AppendLine("</div>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private string GerarHtmlDownloads()
        {
            var lista = DownloadManager.Carregar();
            if (lista == null)
                lista = new System.Collections.Generic.List<DownloadItem>();

            var html = new StringBuilder();

            html.AppendLine("<html><head><meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Segoe UI; background: #1e1e1e; color: white; padding: 20px; }");
            html.AppendLine("h1 { margin-bottom: 12px; }");
            html.AppendLine(".descricao { color: #bbb; margin-bottom: 20px; }");
            html.AppendLine(".btn-limpar { padding: 10px 16px; border: none; border-radius: 10px; cursor: pointer; font-size: 14px; background: #c62828; color: white; margin-bottom: 20px; }");
            html.AppendLine(".item { padding: 14px; margin-bottom: 12px; background: #2d2d2d; border-radius: 12px; }");
            html.AppendLine(".arquivo { font-size: 16px; font-weight: bold; color: #4fc3f7; margin-bottom: 6px; }");
            html.AppendLine(".linha { font-size: 12px; color: #ccc; margin-top: 4px; word-break: break-all; }");
            html.AppendLine(".status { display: inline-block; margin-top: 8px; padding: 4px 10px; border-radius: 999px; background: #3a3a3a; color: #fff; font-size: 12px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<h1>Downloads</h1>");
            html.AppendLine("<div class='descricao'>Aqui aparece o histórico do que foi baixado pelo navegador.</div>");
            html.AppendLine("<button class='btn-limpar' onclick=\"window.chrome.webview.postMessage('limpar-downloads')\">Limpar histórico</button>");

            if (lista.Count == 0)
            {
                html.AppendLine("<p>Nenhum download registrado ainda...</p>");
            }
            else
            {
                foreach (var item in lista)
                {
                    string arquivo = System.Net.WebUtility.HtmlEncode(
                        string.IsNullOrWhiteSpace(item.Arquivo) ? "Arquivo sem nome" : Path.GetFileName(item.Arquivo));
                    string caminho = System.Net.WebUtility.HtmlEncode(item.Arquivo ?? "");
                    string url = System.Net.WebUtility.HtmlEncode(item.Url ?? "");
                    string status = System.Net.WebUtility.HtmlEncode(item.Status ?? "");
                    string inicio = System.Net.WebUtility.HtmlEncode(item.DataInicio.ToString("dd/MM/yyyy HH:mm"));
                    string fim = System.Net.WebUtility.HtmlEncode(item.DataFim?.ToString("dd/MM/yyyy HH:mm") ?? "-");

                    html.AppendLine($@"
                        <div class='item'>
                            <div class='arquivo'>{arquivo}</div>
                            <div class='linha'><strong>Status:</strong> {status}</div>
                            <div class='linha'><strong>Origem:</strong> {url}</div>
                            <div class='linha'><strong>Salvo em:</strong> {caminho}</div>
                            <div class='linha'><strong>Iniciado em:</strong> {inicio}</div>
                            <div class='linha'><strong>Finalizado em:</strong> {fim}</div>
                        </div>");
                }
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private string GerarHtmlFavoritos()
        {
            var lista = FavoritosManager.Carregar();
            if (lista == null)
                lista = new System.Collections.Generic.List<FavoritoItem>();

            var html = new StringBuilder();

            html.AppendLine("<html><head><meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Segoe UI; background: #1e1e1e; color: white; padding: 20px; }");
            html.AppendLine("h1 { margin-bottom: 20px; }");
            html.AppendLine(".btn-limpar { padding: 10px 16px; border: none; border-radius: 10px; cursor: pointer; font-size: 14px; background: #c62828; color: white; margin-bottom: 20px; }");
            html.AppendLine(".item { padding: 12px; margin-bottom: 10px; background: #2d2d2d; border-radius: 10px; transition: 0.2s; }");
            html.AppendLine(".item:hover { background: #3a3a3a; }");
            html.AppendLine(".titulo { font-size: 16px; font-weight: bold; color: #ffd54f; }");
            html.AppendLine(".url { font-size: 12px; color: #aaa; word-break: break-all; }");
            html.AppendLine(".data { font-size: 11px; color: #777; margin-top: 4px; }");
            html.AppendLine("a { text-decoration: none; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<h1>Favoritos</h1>");
            html.AppendLine("<button class='btn-limpar' onclick=\"window.chrome.webview.postMessage('limpar-favoritos')\">Limpar favoritos</button>");

            if (lista.Count == 0)
            {
                html.AppendLine("<p>Nenhum favorito salvo ainda...</p>");
            }
            else
            {
                foreach (var item in lista)
                {
                    string titulo = System.Net.WebUtility.HtmlEncode(item.Titulo ?? "");
                    string url = System.Net.WebUtility.HtmlEncode(item.Url ?? "");
                    string data = System.Net.WebUtility.HtmlEncode(item.Data.ToString("dd/MM/yyyy HH:mm"));

                    html.AppendLine($@"
                        <div class='item'>
                            <a href='{url}'>
                                <div class='titulo'>{titulo}</div>
                                <div class='url'>{url}</div>
                                <div class='data'>{data}</div>
                            </a>
                        </div>");
                }
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        public Form1()
        {
            InitializeComponent();

            animacaoTimer = new System.Windows.Forms.Timer();
            animacaoTimer.Interval = 15; // ~60 FPS

            animacaoTimer.Tick += (s, e) =>
            {
                animProgress += 0.3f;

                if (animProgress >= 1f)
                {
                    animacaoTimer.Stop();

                    if (indiceDestino >= 0 && abaArrastando >= 0)
                    {
                        var tab = tabControl1.TabPages[abaArrastando];

                        tabControl1.TabPages.Remove(tab);
                        tabControl1.TabPages.Insert(indiceDestino, tab);

                        tabControl1.SelectedTab = tab;
                        abaArrastando = indiceDestino;
                    }

                    animProgress = 0f;
                    indiceDestino = -1;

                    tabControl1.Refresh(); // só atualiza no final
                }
            };

            CriarMenuContextoAbas();
            CriarMenuContextoLinks();
            AtualizarIndicadoresModoLight();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.BackColor = Color.FromArgb(248, 249, 250);

            this.BackColor = Color.FromArgb(245, 245, 245);


            if (panelTop != null)
                panelTop.BackColor = Color.FromArgb(240, 240, 240);

            if (tabControl1 != null)
            {
                tabControl1.BackColor = Color.FromArgb(245, 245, 245);
                tabControl1.TabPages.Clear();
                await CriarAbaHistorico();
                GarantirAbaAdicionar();
            }

        }

        private async System.Threading.Tasks.Task CriarNovaAba(string url)
        {
            TabPage novaPagina = new TabPage("Nova guia");
            novaPagina.BackColor = Color.FromArgb(248, 249, 250);
            WebView2 navegador = new WebView2
            {
                Dock = DockStyle.Fill
            };

            novaPagina.Controls.Add(navegador);
            InserirAntesDaAbaAdicionar(novaPagina);
            tabControl1.SelectedTab = novaPagina;

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuNavegador");

            var env = await CoreWebView2Environment.CreateAsync(null, pasta);

            await navegador.EnsureCoreWebView2Async(env);
            ConfigurarMenuNativoComExtras(navegador);
            RegistrarDownloads(navegador);
            RegistrarAberturaNovaJanelaEmAba(navegador);
            navegador.CoreWebView2.DocumentTitleChanged += (s, e) =>
            {
                if (tabControl1.TabPages.Contains(novaPagina))
                {
                    novaPagina.Text = navegador.CoreWebView2.DocumentTitle;
                }
            };

            navegador.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                string urlAtual = navegador.Source?.ToString() ?? "";
                string titulo = navegador.CoreWebView2.DocumentTitle ?? urlAtual;

                txtUrl.Text = urlAtual;
                alternarSelecaoTxtUrl = true;

                if (!string.IsNullOrWhiteSpace(urlAtual) && urlAtual.StartsWith("http"))
                {
                    HistoricoManager.Salvar(urlAtual, titulo);
                    AtualizarAbaHistorico();
                }

                AtualizarBotoes();
                _ = AplicarModoLightSeNecessarioAsync(navegador);
            };

            navegador.Source = new Uri(AjustarUrl(url));
        }

        private void AtualizarIndicadoresModoLight()
        {
            if (btnModoLight != null)
            {
                btnModoLight.BackColor = modoLightAtivo ? Color.FromArgb(215, 241, 221) : SystemColors.Control;
                btnModoLight.Text = modoLightAtivo ? "Light: ON" : "Modo light";
            }

            if (menuModoLight != null)
                menuModoLight.Checked = modoLightAtivo;

            if (menuModoLightPrincipal != null)
                menuModoLightPrincipal.Checked = modoLightAtivo;
        }

        private void AlternarModoLight()
        {
            modoLightAtivo = !modoLightAtivo;
            AtualizarIndicadoresModoLight();

            WebView2 navegadorAtual = ObterNavegadorAtual();
            if (navegadorAtual != null)
                _ = AplicarModoLightSeNecessarioAsync(navegadorAtual);
        }

        private bool EhPaginaWebNormal(Uri uri)
        {
            if (uri == null)
                return false;

            return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) ||
                   uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }

        private bool EhPaginaChatGpt(Uri uri)
        {
            if (uri == null)
                return false;

            string host = uri.Host?.ToLowerInvariant() ?? "";
            return host.Contains("chatgpt.com") || host.Contains("chat.openai.com");
        }

        private async System.Threading.Tasks.Task AplicarModoLightSeNecessarioAsync(WebView2 navegador)
        {
            if (navegador?.CoreWebView2 == null || navegador.Source == null || !EhPaginaWebNormal(navegador.Source))
                return;

            bool paginaChatGpt = EhPaginaChatGpt(navegador.Source);

            string script = $$"""
(() => {
  const enabled = {{(modoLightAtivo ? "true" : "false")}};
  const isChatGpt = {{(paginaChatGpt ? "true" : "false")}};
  const styleId = 'codex-chatgpt-light-style';
  const head = document.head || document.documentElement;
  if (!head) return 'no-head';

  const css = `
    html.codex-site-light *,
    html.codex-site-light *::before,
    html.codex-site-light *::after {
      animation: none !important;
      transition: none !important;
      scroll-behavior: auto !important;
      backdrop-filter: none !important;
      box-shadow: none !important;
    }

    html.codex-site-light main > *,
    html.codex-site-light [role="main"] > *,
    html.codex-site-light article,
    html.codex-site-light section,
    html.codex-site-light aside {
      content-visibility: auto !important;
      contain-intrinsic-size: 900px !important;
    }

    html.codex-site-light video,
    html.codex-site-light audio {
      will-change: auto !important;
    }

    html.codex-site-light [class*="backdrop-blur"],
    html.codex-site-light [style*="backdrop-filter"] {
      backdrop-filter: none !important;
    }

    html.codex-site-light img {
      image-rendering: auto !important;
    }

    html.codex-site-light video[autoplay],
    html.codex-site-light iframe[autoplay] {
      display: none !important;
    }

    html.codex-site-light.codex-chatgpt-light main article,
    html.codex-site-light.codex-chatgpt-light main [data-testid*="conversation-turn"],
    html.codex-site-light.codex-chatgpt-light main [class*="conversation"],
    html.codex-site-light.codex-chatgpt-light main [class*="message"] {
      content-visibility: auto !important;
      contain-intrinsic-size: 900px !important;
    }

    html.codex-site-light.codex-chatgpt-light video,
    html.codex-site-light.codex-chatgpt-light canvas,
    html.codex-site-light.codex-chatgpt-light iframe {
      display: none !important;
    }

    html.codex-site-light.codex-chatgpt-light [class*="backdrop-blur"],
    html.codex-site-light.codex-chatgpt-light [style*="backdrop-filter"] {
      backdrop-filter: none !important;
    }    
  `;

  let style = document.getElementById(styleId);
  if (enabled) {
    if (!style) {
      style = document.createElement('style');
      style.id = styleId;
      style.textContent = css;
      head.appendChild(style);
    }
    document.documentElement.classList.add('codex-site-light');
    if (isChatGpt)
      document.documentElement.classList.add('codex-chatgpt-light');
    else
      document.documentElement.classList.remove('codex-chatgpt-light');

    document.querySelectorAll('img').forEach(img => {
      img.loading = 'lazy';
      img.decoding = 'async';
    });
    document.querySelectorAll('iframe').forEach(frame => frame.loading = 'lazy');
    document.querySelectorAll('video, audio').forEach(media => {
      media.autoplay = false;
      media.preload = 'metadata';
      try { media.pause(); } catch {}
    });
    return 'enabled';
  }

  if (style) style.remove();
  document.documentElement.classList.remove('codex-site-light');
  document.documentElement.classList.remove('codex-chatgpt-light');
  return 'disabled';
})();
""";

            try
            {
                await navegador.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch
            {
            }
        }

        private void ExecutarAcaoEdicao(string acao)
        {
            Control foco = ActiveControl;

            if (foco is TextBoxBase caixaTexto)
            {
                switch (acao)
                {
                    case "cut":
                        caixaTexto.Cut();
                        break;
                    case "copy":
                        caixaTexto.Copy();
                        break;
                    case "paste":
                        caixaTexto.Paste();
                        break;
                    case "selectAll":
                        caixaTexto.SelectAll();
                        break;
                }

                return;
            }

            WebView2 navegador = ObterNavegadorAtual();
            if (navegador?.CoreWebView2 == null)
                return;

            try
            {
                string script = "(function() { document.execCommand(" +
                    JsonSerializer.Serialize(acao) +
                    ", false, null); })();";
                _ = navegador.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch
            {
            }
        }

        private WebView2 ObterNavegadorAtual()
        {
            if (tabControl1.SelectedTab == null ||
                EhAbaAdicionar(tabControl1.SelectedTab) ||
                EhAbaEditorCodigo(tabControl1.SelectedTab) ||
                tabControl1.SelectedTab.Controls.Count == 0)
                return null;

            return tabControl1.SelectedTab.Controls[0] as WebView2;
        }

        private string AjustarUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "https://www.google.com";

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            return url;
        }

        private void NavegarParaUrl()
        {
            WebView2 navegador = ObterNavegadorAtual();
            if (navegador == null) return;

            string texto = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return;

            try
            {
                if (PareceUrl(texto))
                {
                    if (!texto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !texto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        texto = "https://" + texto;
                    }

                    navegador.Source = new Uri(texto);
                }
                else
                {
                    string buscaGoogle = "https://www.google.com/search?q=" + Uri.EscapeDataString(texto);
                    navegador.Source = new Uri(buscaGoogle);
                }
            }
            catch
            {
                string buscaGoogle = "https://www.google.com/search?q=" + Uri.EscapeDataString(texto);
                navegador.Source = new Uri(buscaGoogle);
            }
        }

        private void btnIr_Click(object sender, EventArgs e)
        {
            NavegarParaUrl();
        }

        private void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavegarParaUrl();
                e.SuppressKeyPress = true;
            }
        }

        private void TxtUrl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (string.IsNullOrEmpty(txtUrl.Text))
                return;

            if (alternarSelecaoTxtUrl)
            {
                txtUrl.SelectAll();
            }
            else
            {
                txtUrl.SelectionLength = 0;
                txtUrl.SelectionStart = txtUrl.TextLength;
            }

            alternarSelecaoTxtUrl = !alternarSelecaoTxtUrl;
        }

        private void btnNovaAba_Click(object sender, EventArgs e)
        {
            _ = CriarAbaHistorico();
        }

        private void menuAbrirPaginaWeb_Click(object sender, EventArgs e)
        {
            string? url = SolicitarUrl("Abrir página da web", "Digite o endereço da página:");

            if (!string.IsNullOrWhiteSpace(url))
                _ = CriarNovaAba(url.Trim());
        }

        private async void menuSalvarPaginaAtual_Click(object sender, EventArgs e)
        {
            if (EhAbaEditorCodigo(tabControl1.SelectedTab))
            {
                await SalvarEditorCodigoAtualAsync();
                return;
            }

            WebView2 navegador = ObterNavegadorAtual();

            if (navegador?.CoreWebView2 == null || navegador.Source == null)
            {
                MessageBox.Show("Abra uma página antes de salvar.");
                return;
            }

            using var dialogo = new SaveFileDialog();
            dialogo.Title = "Salvar página atual";
            dialogo.Filter =
                "Página da Web (*.html)|*.html|" +
                "Página da Web (*.htm)|*.htm|" +
                "Arquivo PHP (*.php)|*.php|" +
                "Arquivo ASPX (*.aspx)|*.aspx|" +
                "Arquivo TXT (*.txt)|*.txt|" +
                "Todos os arquivos (*.*)|*.*";
            dialogo.DefaultExt = "html";
            dialogo.AddExtension = true;
            dialogo.FileName = GerarNomeArquivoPagina(navegador);

            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string extensao = Path.GetExtension(dialogo.FileName).ToLowerInvariant();
                string conteudo;

                if (extensao == ".txt")
                {
                    conteudo = await ObterTextoPaginaAsync(navegador);
                }
                else
                {
                    conteudo = await ObterHtmlPaginaAsync(navegador);
                }

                File.WriteAllText(dialogo.FileName, conteudo, Encoding.UTF8);
                MessageBox.Show("Página salva com sucesso.");
            }
            catch
            {
                MessageBox.Show("Não foi possível salvar a página atual.");
            }
        }

        private void menuSairNavegador_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuAbrirPagina_Click(object sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog();
            dialogo.Title = "Abrir página";
            dialogo.Filter = "Páginas da web (*.html;*.htm)|*.html;*.htm|Todos os arquivos (*.*)|*.*";
            dialogo.CheckFileExists = true;
            dialogo.Multiselect = false;

            if (dialogo.ShowDialog(this) == DialogResult.OK)
            {
                string caminhoArquivo = new Uri(dialogo.FileName).AbsoluteUri;
                _ = CriarNovaAba(caminhoArquivo);
            }
        }

        private void menuDownloads_Click(object sender, EventArgs e)
        {
            _ = CriarAbaDownloads();
        }

        private void menuRecortar_Click(object sender, EventArgs e)
        {
            ExecutarAcaoEdicao("cut");
        }

        private void menuCopiar_Click(object sender, EventArgs e)
        {
            ExecutarAcaoEdicao("copy");
        }

        private void menuColar_Click(object sender, EventArgs e)
        {
            ExecutarAcaoEdicao("paste");
        }

        private void menuSelecionarTudo_Click(object sender, EventArgs e)
        {
            ExecutarAcaoEdicao("selectAll");
        }

        private void menuModoLight_Click(object sender, EventArgs e)
        {
            AlternarModoLight();
        }

        private async void menuAbrirEditor_Click(object sender, EventArgs e)
        {
            await AbrirEditorEmBrancoAsync();
        }

        private async void menuJogoOffline_Click(object sender, EventArgs e)
        {
            await CriarAbaJogoOffline();
        }

        private void menuRascunhos_Click(object sender, EventArgs e)
        {
            AbrirOuFocarAbaRascunhos();
        }

        private async void menuCodigoFonte_Click(object sender, EventArgs e)
        {
            WebView2 navegador = ObterNavegadorAtual();

            if (navegador != null)
                await AbrirCodigoFonteDoNavegadorAsync(navegador);
        }

        private void menuFavoritos_Click(object sender, EventArgs e)
        {
            _ = CriarAbaFavoritos();
        }

        private void btnFavoritos_Click(object sender, EventArgs e)
        {
            AdicionarPaginaAtualAosFavoritos();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.F)
            {
                AdicionarPaginaAtualAosFavoritos();
                e.SuppressKeyPress = true;
            }
        }

        private void btnDownloads_Click(object sender, EventArgs e)
        {
            pnlDownloads.Visible = !pnlDownloads.Visible;

            if (pnlDownloads.Visible)
            {
                AtualizarPainelDownloads();
                pnlDownloads.BringToFront();
            }
        }

        private void btnModoLight_Click(object sender, EventArgs e)
        {
            AlternarModoLight();
        }

        private async void lnkVerMaisDownloads_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pnlDownloads.Visible = false;
            await CriarAbaDownloads();
        }

        private string? SolicitarUrl(string titulo, string mensagem)
        {
            using var janela = new Form();
            using var lblMensagem = new Label();
            using var txtEntrada = new TextBox();
            using var btnOk = new Button();
            using var btnCancelar = new Button();

            janela.Text = titulo;
            janela.FormBorderStyle = FormBorderStyle.FixedDialog;
            janela.StartPosition = FormStartPosition.CenterParent;
            janela.ClientSize = new Size(420, 130);
            janela.MaximizeBox = false;
            janela.MinimizeBox = false;
            janela.ShowInTaskbar = false;

            lblMensagem.AutoSize = true;
            lblMensagem.Location = new Point(12, 15);
            lblMensagem.Text = mensagem;

            txtEntrada.Location = new Point(12, 42);
            txtEntrada.Size = new Size(392, 23);
            txtEntrada.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            btnOk.Text = "Abrir";
            btnOk.Location = new Point(248, 85);
            btnOk.Size = new Size(75, 27);
            btnOk.DialogResult = DialogResult.OK;

            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(329, 85);
            btnCancelar.Size = new Size(75, 27);
            btnCancelar.DialogResult = DialogResult.Cancel;

            janela.Controls.Add(lblMensagem);
            janela.Controls.Add(txtEntrada);
            janela.Controls.Add(btnOk);
            janela.Controls.Add(btnCancelar);
            janela.AcceptButton = btnOk;
            janela.CancelButton = btnCancelar;

            return janela.ShowDialog(this) == DialogResult.OK
                ? txtEntrada.Text
                : null;
        }

        private string GerarNomeArquivoPagina(WebView2 navegador)
        {
            string titulo = navegador.CoreWebView2?.DocumentTitle;

            if (string.IsNullOrWhiteSpace(titulo))
                titulo = "pagina";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                titulo = titulo.Replace(c, '_');
            }

            return titulo + ".html";
        }

        private async System.Threading.Tasks.Task AbrirCodigoFontePaginaAtual()
        {
            WebView2 navegador = ObterNavegadorAtual();

            if (navegador != null)
                await AbrirCodigoFonteDoNavegadorAsync(navegador);
        }

        private async System.Threading.Tasks.Task AbrirEditorEmBrancoAsync()
        {
            await CriarAbaEditorCodigo("novo-arquivo.html", "");
        }

        private void AbrirOuFocarAbaRascunhos()
        {
            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (EhAbaAdicionar(aba))
                    continue;

                if (aba.Tag is RascunhoTabState)
                {
                    tabControl1.SelectedTab = aba;
                    AtualizarListaRascunhosNaAba(aba);
                    return;
                }
            }

            CriarAbaRascunhos();
        }

        private void CriarAbaRascunhos()
        {
            TabPage pagina = new TabPage("Rascunhos");
            pagina.BackColor = Color.FromArgb(248, 249, 250);

            Panel painelEsquerdo = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblLista = new Label
            {
                Text = "Seus rascunhos",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            ListBox listaRascunhos = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            painelEsquerdo.Controls.Add(listaRascunhos);
            painelEsquerdo.Controls.Add(lblLista);

            Panel painelDireito = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White
            };

            Panel barraAcoes = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38
            };

            Button btnNovo = new Button
            {
                Text = "Novo",
                Location = new Point(0, 6),
                Size = new Size(70, 26)
            };

            Button btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(76, 6),
                Size = new Size(70, 26)
            };

            Button btnExcluir = new Button
            {
                Text = "Excluir",
                Location = new Point(152, 6),
                Size = new Size(70, 26)
            };

            Button btnSalvarTxt = new Button
            {
                Text = "Salvar .txt",
                Location = new Point(228, 6),
                Size = new Size(90, 26)
            };

            Label lblStatus = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(324, 8),
                Width = 336,
                Height = 22,
                ForeColor = Color.FromArgb(90, 90, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            barraAcoes.Controls.Add(btnNovo);
            barraAcoes.Controls.Add(btnSalvar);
            barraAcoes.Controls.Add(btnExcluir);
            barraAcoes.Controls.Add(btnSalvarTxt);
            barraAcoes.Controls.Add(lblStatus);

            TextBox txtTitulo = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                PlaceholderText = "Titulo do rascunho",
                Height = 34
            };

            WebView2 txtConteudo = new WebView2
            {
                Dock = DockStyle.Fill
            };

            painelDireito.Controls.Add(txtConteudo);
            painelDireito.Controls.Add(txtTitulo);
            painelDireito.Controls.Add(barraAcoes);

            pagina.Controls.Add(painelDireito);
            pagina.Controls.Add(painelEsquerdo);

            var estado = new RascunhoTabState
            {
                ListaRascunhos = listaRascunhos,
                TxtTitulo = txtTitulo,
                TxtConteudo = txtConteudo,
                LblStatus = lblStatus
            };

            txtConteudo.Tag = estado;
            pagina.Tag = estado;

            listaRascunhos.SelectedIndexChanged += async (s, e) =>
            {
                if (listaRascunhos.SelectedItem is RascunhoItem item)
                    await CarregarRascunhoNaAbaAsync(estado, item);
            };

            btnNovo.Click += (s, e) => LimparEditorRascunho(estado);
            btnSalvar.Click += async (s, e) => await SalvarRascunhoDaAbaAsync(estado, pagina);
            btnExcluir.Click += (s, e) => ExcluirRascunhoDaAba(estado, pagina);
            btnSalvarTxt.Click += async (s, e) => await SalvarRascunhoComoTxtAsync(estado);

            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;
            _ = InicializarEditorRascunhoAsync(estado);
            AtualizarListaRascunhosNaAba(pagina);
            LimparEditorRascunho(estado);
        }

        private void AtualizarListaRascunhosNaAba(TabPage pagina)
        {
            if (pagina?.Tag is not RascunhoTabState estado)
                return;

            var lista = RascunhoManager.Carregar();
            estado.ListaRascunhos.BeginUpdate();
            estado.ListaRascunhos.DataSource = null;
            estado.ListaRascunhos.DisplayMember = nameof(RascunhoItem.Titulo);
            estado.ListaRascunhos.ValueMember = nameof(RascunhoItem.Id);
            estado.ListaRascunhos.DataSource = lista;
            estado.ListaRascunhos.EndUpdate();
        }

        private async System.Threading.Tasks.Task CarregarRascunhoNaAbaAsync(RascunhoTabState estado, RascunhoItem item)
        {
            estado.RascunhoAtualId = item.Id;
            estado.TxtTitulo.Text = item.Titulo;
            await DefinirConteudoRascunhoAsync(estado.TxtConteudo, item.Conteudo);
            estado.LblStatus.Text = $"Atualizado em {item.DataAtualizacao:dd/MM/yyyy HH:mm}";
        }

        private void LimparEditorRascunho(RascunhoTabState estado)
        {
            estado.RascunhoAtualId = null;
            estado.TxtTitulo.Text = "";
            _ = DefinirConteudoRascunhoAsync(estado.TxtConteudo, "");
            estado.LblStatus.Text = "Novo rascunho";
            estado.ListaRascunhos.ClearSelected();
            estado.TxtTitulo.Focus();
        }

        private async System.Threading.Tasks.Task SalvarRascunhoDaAbaAsync(RascunhoTabState estado, TabPage pagina)
        {
            string titulo = estado.TxtTitulo.Text.Trim();
            string conteudo = await ObterConteudoRascunhoAsync(estado.TxtConteudo);

            if (string.IsNullOrWhiteSpace(titulo) && string.IsNullOrWhiteSpace(conteudo))
            {
                MessageBox.Show("Digite alguma coisa antes de salvar o rascunho.");
                return;
            }

            if (string.IsNullOrWhiteSpace(titulo))
                titulo = "Sem título";

            var item = new RascunhoItem
            {
                Id = string.IsNullOrWhiteSpace(estado.RascunhoAtualId) ? Guid.NewGuid().ToString("N") : estado.RascunhoAtualId,
                Titulo = titulo,
                Conteudo = conteudo
            };

            RascunhoManager.Salvar(item);
            estado.RascunhoAtualId = item.Id;
            estado.LblStatus.Text = $"Salvo em {DateTime.Now:dd/MM/yyyy HH:mm}";
            AtualizarListaRascunhosNaAba(pagina);

            foreach (var entry in estado.ListaRascunhos.Items)
            {
                if (entry is RascunhoItem rascunho && rascunho.Id == item.Id)
                {
                    estado.ListaRascunhos.SelectedItem = entry;
                    break;
                }
            }
        }

        private void ExcluirRascunhoDaAba(RascunhoTabState estado, TabPage pagina)
        {
            if (string.IsNullOrWhiteSpace(estado.RascunhoAtualId))
            {
                MessageBox.Show("Selecione um rascunho salvo para excluir.");
                return;
            }

            RascunhoManager.Excluir(estado.RascunhoAtualId);
            AtualizarListaRascunhosNaAba(pagina);
            LimparEditorRascunho(estado);
            estado.LblStatus.Text = "Rascunho excluído";
        }

        private async System.Threading.Tasks.Task InicializarEditorRascunhoAsync(RascunhoTabState estado)
        {
            WebView2 editor = estado.TxtConteudo;
            var env = await CoreWebView2Environment.CreateAsync(null, pastaDadosApp);
            await editor.EnsureCoreWebView2Async(env);
            editor.CoreWebView2.WebMessageReceived += async (s, e) =>
            {
                if (e.TryGetWebMessageAsString() == "salvar-rascunho-txt")
                    await SalvarRascunhoComoTxtAsync(estado);
            };
            editor.NavigationCompleted += async (s, e) =>
            {
                estado.EditorPronto = true;
                await DefinirConteudoRascunhoAsync(editor, estado.ConteudoPendente);
            };
            editor.CoreWebView2.NavigateToString(GerarHtmlEditorRascunho());
        }

        private async System.Threading.Tasks.Task<string> ObterConteudoRascunhoAsync(WebView2 editor)
        {
            if (editor?.CoreWebView2 == null)
                return "";

            string resultado = await editor.CoreWebView2.ExecuteScriptAsync("window.getDraftContent ? window.getDraftContent() : ''");
            return JsonSerializer.Deserialize<string>(resultado) ?? "";
        }

        private async System.Threading.Tasks.Task<string> ObterTextoPlanoRascunhoAsync(WebView2 editor)
        {
            if (editor?.CoreWebView2 == null)
                return "";

            string resultado = await editor.CoreWebView2.ExecuteScriptAsync("window.getDraftText ? window.getDraftText() : ''");
            return JsonSerializer.Deserialize<string>(resultado) ?? "";
        }

        private async System.Threading.Tasks.Task DefinirConteudoRascunhoAsync(WebView2 editor, string conteudo)
        {
            var estado = editor?.Tag as RascunhoTabState;
            if (estado != null)
                estado.ConteudoPendente = conteudo ?? "";

            if (editor?.CoreWebView2 == null || estado?.EditorPronto == false)
                return;

            string conteudoJson = JsonSerializer.Serialize(conteudo ?? "");
            await editor.CoreWebView2.ExecuteScriptAsync($"window.setDraftContent && window.setDraftContent({conteudoJson});");
        }

        private async System.Threading.Tasks.Task SalvarRascunhoComoTxtAsync(RascunhoTabState estado)
        {
            string texto = await ObterTextoPlanoRascunhoAsync(estado.TxtConteudo);

            if (string.IsNullOrWhiteSpace(texto))
            {
                MessageBox.Show("Escreva alguma coisa antes de salvar em .txt.");
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Salvar rascunho como texto",
                Filter = "Arquivo de texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*",
                FileName = string.IsNullOrWhiteSpace(estado.TxtTitulo.Text) ? "rascunho.txt" : $"{estado.TxtTitulo.Text.Trim()}.txt",
                DefaultExt = "txt"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllText(dialog.FileName, texto, Encoding.UTF8);
            estado.LblStatus.Text = $"Texto exportado em {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        private string GerarHtmlEditorRascunho()
        {
            return $$"""
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <style>
    html, body {
      margin: 0;
      height: 100%;
    }
    body {
      display: flex;
      flex-direction: column;
      background-color: #ffffff;
      background-image: url('{{ObterImagemDataUri("fundozap.jpg")}}');
      background-repeat: repeat;
      background-size: auto;
      background-position: top left;
      overflow: hidden;
    }
    .toolbar {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 12px;
      border-bottom: 1px solid rgba(0, 0, 0, 0.08);
      background: rgba(255, 255, 255, 0.88);
      backdrop-filter: blur(4px);
      font-family: "Segoe UI", sans-serif;
    }
    .toolbar select, .toolbar button {
      height: 30px;
      border: 1px solid #c9ced6;
      border-radius: 8px;
      background: #fff;
      padding: 0 10px;
      font-family: "Segoe UI", sans-serif;
      font-size: 13px;
      cursor: pointer;
    }
    .toolbar button:hover, .toolbar select:hover {
      border-color: #8ea6c0;
      background: #f5f8fb;
    }
    .toolbar input[type="color"] {
      width: 36px;
      height: 30px;
      padding: 2px;
      border: 1px solid #c9ced6;
      border-radius: 8px;
      background: #fff;
      cursor: pointer;
    }
    .editor-wrap {
      flex: 1;
      overflow: auto;
      padding: 18px;
    }
    #draft {
      min-height: 100%;
      box-sizing: border-box;
      border: none;
      outline: none;
      font-family: "Segoe UI", sans-serif;
      font-size: 18px;
      line-height: 1.5;
      color: #232323;
      background: transparent;
      white-space: pre-wrap;
      word-break: break-word;
    }
    #draft b, #draft strong {
      font-weight: 700;
    }
    .with-shadow {
      text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.30);
    }
  </style>
</head>
<body>
  <div class="toolbar">
    <select id="fontFamily" onchange="changeFontFamily(this.value)">
      <option value="Segoe UI">Segoe UI</option>
      <option value="Arial">Arial</option>
      <option value="Georgia">Georgia</option>
      <option value="Tahoma">Tahoma</option>
      <option value="Verdana">Verdana</option>
      <option value="Courier New">Courier New</option>
    </select>
    <select id="fontSize" onchange="changeFontSize(this.value)">
      <option value="14px">14</option>
      <option value="16px">16</option>
      <option value="18px" selected>18</option>
      <option value="20px">20</option>
      <option value="24px">24</option>
      <option value="28px">28</option>
      <option value="32px">32</option>
    </select>
    <input id="textColor" type="color" value="#232323" title="Cor do texto" onchange="changeTextColor(this.value)" />
    <button type="button" onclick="toggleBold()">Negrito</button>
    <button type="button" onclick="toggleShadow()">Sombra</button>
    <button type="button" onclick="saveAsTxt()">Salvar .txt</button>
  </div>
  <div class="editor-wrap">
    <div id="draft" contenteditable="true" spellcheck="true"></div>
  </div>
  <script>
    const editor = document.getElementById('draft');

    function focusEditor() {
      editor.focus();
    }

    function applyToSelection(styleBuilder) {
      focusEditor();
      const selection = window.getSelection();
      if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
        styleBuilder(editor);
        return;
      }

      const range = selection.getRangeAt(0);
      const span = document.createElement('span');
      styleBuilder(span);

      try {
        range.surroundContents(span);
      } catch {
        const content = range.extractContents();
        span.appendChild(content);
        range.insertNode(span);
      }

      selection.removeAllRanges();
      const newRange = document.createRange();
      newRange.selectNodeContents(span);
      selection.addRange(newRange);
    }

    function toggleBold() {
      focusEditor();
      document.execCommand('bold', false, null);
    }

    function changeFontFamily(value) {
      applyToSelection(target => {
        target.style.fontFamily = value;
      });
    }

    function changeFontSize(value) {
      applyToSelection(target => {
        target.style.fontSize = value;
      });
    }

    function toggleShadow() {
      applyToSelection(target => {
        const atual = target.style.textShadow;
        target.style.textShadow = atual ? '' : '1px 1px 2px rgba(0,0,0,0.30)';
      });
    }

    function changeTextColor(value) {
      applyToSelection(target => {
        target.style.color = value;
      });
    }

    function saveAsTxt() {
      if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage('salvar-rascunho-txt');
      }
    }

    window.getDraftContent = function() {
      return editor.innerHTML || '';
    };
    window.getDraftText = function() {
      return editor.innerText || '';
    };
    window.setDraftContent = function(value) {
      const texto = value || '';
      if (/[<][a-z!/]/i.test(texto))
        editor.innerHTML = texto;
      else
        editor.innerText = texto;
    };
  </script>
</body>
</html>
""";
        }

        private string ObterImagemDataUri(string nomeArquivo)
        {
            Assembly assembly = typeof(Form1).Assembly;
            string nomeRecurso = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(nome => nome.EndsWith(nomeArquivo, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(nomeRecurso))
                return "";

            using Stream stream = assembly.GetManifestResourceStream(nomeRecurso);

            if (stream == null)
                return "";

            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            string base64 = Convert.ToBase64String(ms.ToArray());
            return "data:image/jpeg;base64," + base64;
        }

        private async System.Threading.Tasks.Task AbrirCodigoFonteDoNavegadorAsync(WebView2 navegador)
        {
            if (navegador?.Source == null)
            {
                MessageBox.Show("Abra uma página antes de ver o código-fonte.");
                return;
            }

            string urlAtual = navegador.Source.ToString();

            if (string.IsNullOrWhiteSpace(urlAtual) || urlAtual == "about:blank")
            {
                MessageBox.Show("Essa página não possui código-fonte disponível.");
                return;
            }

            try
            {
                string codigoFonte = await CarregarCodigoFonteAsync(urlAtual, navegador);
                await CriarAbaEditorCodigo(urlAtual, codigoFonte);
            }
            catch
            {
                MessageBox.Show("Não foi possível abrir o código-fonte dessa página.");
            }
        }

        private async System.Threading.Tasks.Task<string> CarregarCodigoFonteAsync(string urlAtual, WebView2 navegador)
        {
            if (urlAtual.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                string caminho = new Uri(urlAtual).LocalPath;
                return File.ReadAllText(caminho, Encoding.UTF8);
            }

            if (urlAtual.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                urlAtual.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                string htmlDaPagina = await ObterHtmlAtualDoWebViewAsync(navegador);
                if (!string.IsNullOrWhiteSpace(htmlDaPagina))
                    return htmlDaPagina;

                using var client = new HttpClient();
                return await client.GetStringAsync(urlAtual);
            }

            return await ObterHtmlPaginaAsync(navegador);
        }

        private async System.Threading.Tasks.Task<string> ObterHtmlAtualDoWebViewAsync(WebView2 navegador)
        {
            if (navegador?.CoreWebView2 == null)
                return "";

            try
            {
                string resultado = await navegador.CoreWebView2.ExecuteScriptAsync("""
(() => {
  const doc = document.documentElement ? document.documentElement.outerHTML : '';
  const doctype = document.doctype ? '<!DOCTYPE ' + document.doctype.name + '>' : '';
  return doctype + doc;
})();
""");

                return JsonSerializer.Deserialize<string>(resultado) ?? "";
            }
            catch
            {
                return "";
            }
        }

        private async System.Threading.Tasks.Task CriarAbaEditorCodigo(string origem, string codigoFonte)
        {
            TabPage pagina = new TabPage("Código-fonte");
            pagina.BackColor = Color.FromArgb(30, 30, 30);
            pagina.Tag = new EditorCodigoInfo { Origem = origem };

            WebView2 editor = new WebView2
            {
                Dock = DockStyle.Fill
            };

            pagina.Controls.Add(editor);
            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuNavegador");
            var env = await CoreWebView2Environment.CreateAsync(null, pasta);

            await editor.EnsureCoreWebView2Async(env);
            editor.CoreWebView2.WebMessageReceived += async (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();

                if (msg == "save-source" && tabControl1.SelectedTab == pagina)
                    await SalvarEditorCodigoAtualAsync();
                else if (msg == "open-source-file" && tabControl1.SelectedTab == pagina)
                    await AbrirArquivoNoEditorAtualAsync();
            };

            editor.CoreWebView2.NavigateToString(GerarHtmlEditorMonaco(origem, codigoFonte));
        }

        private void SalvarCodigoFonteComo(string conteudo, string origem)
        {
            using var dialogo = new SaveFileDialog();
            dialogo.Title = "Salvar código-fonte";
            dialogo.Filter =
                "Página HTML (*.html)|*.html|" +
                "Página HTM (*.htm)|*.htm|" +
                "Arquivo PHP (*.php)|*.php|" +
                "Arquivo CSS (*.css)|*.css|" +
                "Arquivo JavaScript (*.js)|*.js|" +
                "Arquivo TXT (*.txt)|*.txt|" +
                "Todos os arquivos (*.*)|*.*";
            dialogo.DefaultExt = "html";
            dialogo.AddExtension = true;
            dialogo.FileName = GerarNomeArquivoCodigoFonte(origem);

            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dialogo.FileName, conteudo, Encoding.UTF8);
                MessageBox.Show("Código-fonte salvo com sucesso.");
            }
            catch
            {
                MessageBox.Show("Não foi possível salvar o código-fonte.");
            }
        }

        private string GerarNomeArquivoCodigoFonte(string origem)
        {
            string nomeBase;

            try
            {
                var uri = new Uri(origem);
                nomeBase = Path.GetFileName(uri.LocalPath);

                if (string.IsNullOrWhiteSpace(nomeBase))
                    nomeBase = uri.Host;
            }
            catch
            {
                nomeBase = "codigo-fonte";
            }

            if (string.IsNullOrWhiteSpace(nomeBase))
                nomeBase = "codigo-fonte";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                nomeBase = nomeBase.Replace(c, '_');
            }

            if (!Path.HasExtension(nomeBase))
                nomeBase += ".html";

            return nomeBase;
        }

        private bool EhAbaEditorCodigo(TabPage aba)
        {
            return aba?.Tag is EditorCodigoInfo;
        }

        private async System.Threading.Tasks.Task SalvarEditorCodigoAtualAsync()
        {
            WebView2 editorAtual = ObterEditorCodigoAtual();
            EditorCodigoInfo info = tabControl1.SelectedTab?.Tag as EditorCodigoInfo;

            if (editorAtual?.CoreWebView2 == null || info == null)
            {
                MessageBox.Show("Abra uma aba de código-fonte antes de salvar.");
                return;
            }

            try
            {
                string resultado = await editorAtual.CoreWebView2.ExecuteScriptAsync("window.getEditorContent ? window.getEditorContent() : ''");
                string conteudo = JsonSerializer.Deserialize<string>(resultado) ?? "";
                SalvarCodigoFonteComo(conteudo, info.Origem);
            }
            catch
            {
                MessageBox.Show("Não foi possível salvar o código-fonte.");
            }
        }

        private async System.Threading.Tasks.Task AbrirArquivoNoEditorAtualAsync()
        {
            WebView2 editorAtual = ObterEditorCodigoAtual();
            EditorCodigoInfo info = tabControl1.SelectedTab?.Tag as EditorCodigoInfo;

            if (editorAtual?.CoreWebView2 == null || info == null)
                return;

            using var dialogo = new OpenFileDialog();
            dialogo.Title = "Abrir arquivo no editor";
            dialogo.Filter =
                "Arquivos de código (*.html;*.htm;*.css;*.js;*.json;*.xml;*.php;*.txt;*.md)|*.html;*.htm;*.css;*.js;*.json;*.xml;*.php;*.txt;*.md|" +
                "Todos os arquivos (*.*)|*.*";
            dialogo.CheckFileExists = true;
            dialogo.Multiselect = false;

            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string conteudo = File.ReadAllText(dialogo.FileName, Encoding.UTF8);
                string origem = new Uri(dialogo.FileName).AbsoluteUri;
                info.Origem = origem;

                string conteudoJson = JsonSerializer.Serialize(conteudo);
                string origemJson = JsonSerializer.Serialize(origem);
                await editorAtual.CoreWebView2.ExecuteScriptAsync($"window.setEditorContent && window.setEditorContent({conteudoJson}, {origemJson});");

                if (tabControl1.SelectedTab != null)
                    tabControl1.SelectedTab.Text = "Código-fonte";
            }
            catch
            {
                MessageBox.Show("Não foi possível abrir o arquivo no editor.");
            }
        }

        private string GerarHtmlEditorMonaco(string origem, string codigoFonte)
        {
            string origemJson = JsonSerializer.Serialize(origem);
            string codigoJson = JsonSerializer.Serialize(codigoFonte);
            return $$"""
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <meta http-equiv="Content-Security-Policy" content="default-src 'self' 'unsafe-inline' data: blob: https://cdnjs.cloudflare.com; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; font-src 'self' data: https://cdnjs.cloudflare.com; connect-src *;">
  <style>
    html, body {
      margin: 0;
      height: 100%;
      background: #1e1e1e;
      color: #d4d4d4;
      font-family: Segoe UI, sans-serif;
      overflow: hidden;
    }
    .app {
      height: 100%;
      display: flex;
      flex-direction: column;
    }
    .toolbar {
      height: 42px;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 0 12px;
      background: #252526;
      border-bottom: 1px solid #333;
      flex-shrink: 0;
    }
    .menu-bar {
      display: flex;
      gap: 2px;
      align-items: center;
    }
    .menu {
      position: relative;
    }
    .menu-button {
      border: none;
      background: transparent;
      color: #d4d4d4;
      padding: 6px 10px;
      border-radius: 4px;
      cursor: pointer;
      font-size: 12px;
    }
    .menu-button:hover {
      background: #2a2d2e;
    }
    .menu-items {
      display: none;
      position: absolute;
      top: 30px;
      left: 0;
      min-width: 170px;
      background: #252526;
      border: 1px solid #3f3f46;
      box-shadow: 0 8px 20px rgba(0,0,0,.35);
      z-index: 5;
      padding: 6px 0;
    }
    .menu.open .menu-items {
      display: block;
    }
    .menu-item {
      padding: 8px 12px;
      font-size: 12px;
      color: #d4d4d4;
      cursor: pointer;
      white-space: nowrap;
    }
    .menu-item:hover {
      background: #094771;
    }
    .origin {
      flex: 1;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      color: #cccccc;
      font-size: 13px;
    }
    .status {
      color: #9cdcfe;
      font-size: 12px;
    }
    .btn {
      border: 1px solid #3f3f46;
      background: #0e639c;
      color: white;
      border-radius: 4px;
      padding: 6px 12px;
      cursor: pointer;
      font-size: 12px;
    }
    .btn:hover {
      filter: brightness(1.08);
    }
    #editor {
      flex: 1;
    }
  </style>
</head>
<body>
  <div class="app">
    <div class="toolbar">
      <div class="menu-bar">
        <div class="menu">
          <button class="menu-button" onclick="toggleMenu(event, this)">Arquivo</button>
          <div class="menu-items">
            <div class="menu-item" onclick="openSourceFile()">Abrir arquivo</div>
            <div class="menu-item" onclick="saveSource()">Salvar como</div>
          </div>
        </div>
        <div class="menu">
          <button class="menu-button" onclick="toggleMenu(event, this)">Editar</button>
          <div class="menu-items">
            <div class="menu-item" onclick="runEditorAction('undo')">Desfazer</div>
            <div class="menu-item" onclick="runEditorAction('redo')">Refazer</div>
            <div class="menu-item" onclick="runEditorAction('actions.find')">Localizar</div>
            <div class="menu-item" onclick="runEditorAction('editor.action.selectAll')">Selecionar tudo</div>
          </div>
        </div>
      </div>
      <div class="origin" id="origin"></div>
      <div class="status" id="status">Carregando editor...</div>
      <button class="btn" onclick="saveSource()">Salvar como</button>
    </div>
    <div id="editor"></div>
  </div>

  <script>window.MonacoEnvironment = { getWorkerUrl: function() { return 'data:text/javascript;charset=utf-8,' + encodeURIComponent('self.MonacoEnvironment = { baseUrl: "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.2/min/" }; importScripts("https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.2/min/vs/base/worker/workerMain.min.js");'); } };</script>
  <script src="https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.2/min/vs/loader.min.js"></script>
  <script>
    const origem = {{origemJson}};
    const codigoInicial = {{codigoJson}};
    let editor;

    document.getElementById('origin').textContent = origem;

    function detectarLinguagem(url) {
      const base = (url || '').toLowerCase();
      if (base.endsWith('.js')) return 'javascript';
      if (base.endsWith('.css')) return 'css';
      if (base.endsWith('.json')) return 'json';
      if (base.endsWith('.xml')) return 'xml';
      if (base.endsWith('.md')) return 'markdown';
      if (base.endsWith('.txt')) return 'plaintext';
      if (base.endsWith('.php')) return 'php';
      return 'html';
    }

    function saveSource() {
      if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage('save-source');
      }
    }

    function openSourceFile() {
      closeMenus();
      if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage('open-source-file');
      }
    }

    function runEditorAction(action) {
      closeMenus();
      if (!editor) return;
      editor.focus();
      editor.trigger('keyboard', action, null);
    }

    function toggleMenu(event, button) {
      event.stopPropagation();
      const menu = button.parentElement;
      const open = menu.classList.contains('open');
      closeMenus();
      if (!open) menu.classList.add('open');
    }

    function closeMenus() {
      document.querySelectorAll('.menu.open').forEach(m => m.classList.remove('open'));
    }

    window.getEditorContent = function() {
      return editor ? editor.getValue() : '';
    };

    window.setEditorContent = function(content, newOrigin) {
      if (editor) {
        editor.setValue(content || '');
      }

      if (newOrigin) {
        document.getElementById('origin').textContent = newOrigin;
      }

      document.getElementById('status').textContent = 'Arquivo aberto';
    };

    window.addEventListener('click', closeMenus);

    require.config({ paths: { vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.2/min/vs' } });
    require(['vs/editor/editor.main'], function() {
      editor = monaco.editor.create(document.getElementById('editor'), {
        value: codigoInicial,
        language: detectarLinguagem(origem),
        theme: 'vs-dark',
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 14,
        wordWrap: 'off',
        scrollBeyondLastLine: false,
        renderWhitespace: 'selection',
        tabSize: 2
      });

      document.getElementById('status').textContent = 'Editor pronto';
    });
  </script>
</body>
</html>
""";
        }

        private WebView2 ObterEditorCodigoAtual()
        {
            if (tabControl1.SelectedTab == null || EhAbaAdicionar(tabControl1.SelectedTab))
                return null;

            foreach (Control control in tabControl1.SelectedTab.Controls)
            {
                if (control is WebView2 editor && EhAbaEditorCodigo(tabControl1.SelectedTab))
                    return editor;
            }

            return null;
        }

        private async System.Threading.Tasks.Task<string> ObterHtmlPaginaAsync(WebView2 navegador)
        {
            string resultado = await navegador.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            return JsonSerializer.Deserialize<string>(resultado) ?? "";
        }

        private async System.Threading.Tasks.Task<string> ObterTextoPaginaAsync(WebView2 navegador)
        {
            string resultado = await navegador.CoreWebView2.ExecuteScriptAsync("document.body ? document.body.innerText : ''");
            return JsonSerializer.Deserialize<string>(resultado) ?? "";
        }

        private void btnFecharAba_Click(object sender, EventArgs e)
        {
            if (ContarAbasNormais() > 1)
            {
                TabPage abaAtual = tabControl1.SelectedTab;

                if (abaAtual == null || EhAbaAdicionar(abaAtual))
                    return;

                if (abaAtual.Controls.Count > 0 && abaAtual.Controls[0] is WebView2 navegador)
                {
                    navegador.Dispose();
                }

                tabControl1.TabPages.Remove(abaAtual);
            }
            else
            {
                MessageBox.Show("Não é possível fechar a última aba.");
            }

            AtualizarBotoes();
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            WebView2 navegador = ObterNavegadorAtual();
            if (navegador != null && navegador.CanGoBack)
                navegador.GoBack();
        }

        private void btnAvancar_Click(object sender, EventArgs e)
        {
            WebView2 navegador = ObterNavegadorAtual();
            if (navegador != null && navegador.CanGoForward)
                navegador.GoForward();
        }

        private void btnRecarregar_Click(object sender, EventArgs e)
        {
            WebView2 navegador = ObterNavegadorAtual();
            navegador?.Reload();
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && EhAbaAdicionar(tabControl1.SelectedTab))
            {
                _ = CriarAbaHistorico();
                return;
            }

            WebView2 navegador = ObterNavegadorAtual();
            if (navegador != null && navegador.Source != null)
            {
                txtUrl.Text = navegador.Source.ToString();
                alternarSelecaoTxtUrl = true;
            }
            else
            {
                txtUrl.Text = "";
                alternarSelecaoTxtUrl = true;
            }

            AtualizarBotoes();
        }

        private void AtualizarBotoes()
        {
            WebView2 navegador = ObterNavegadorAtual();

            if (navegador == null)
            {
                btnVoltar.Enabled = false;
                btnAvancar.Enabled = false;

                return;
            }
            btnVoltar.Enabled = navegador.CanGoBack;
            btnAvancar.Enabled = navegador.CanGoForward;
        }

        // Método para gerar o histórico
        private async System.Threading.Tasks.Task CriarAbaHistorico()
        {
            TabPage pagina = new TabPage("Início");
            pagina.BackColor = Color.FromArgb(248, 249, 250);

            WebView2 navegador = new WebView2
            {
                Dock = DockStyle.Fill
            };

            pagina.Controls.Add(navegador);
            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuNavegador");

            var env = await CoreWebView2Environment.CreateAsync(null, pasta);

            await navegador.EnsureCoreWebView2Async(env);
            ConfigurarMenuNativoComExtras(navegador);
            RegistrarDownloads(navegador);
            RegistrarAberturaNovaJanelaEmAba(navegador);
            navegador.CoreWebView2.DocumentTitleChanged += (s, e) =>
            {
                if (!tabControl1.TabPages.Contains(pagina))
                    return;

                string urlAtual = navegador.Source?.ToString() ?? "";

                if (urlAtual == "about:blank" || string.IsNullOrWhiteSpace(urlAtual))
                    pagina.Text = "Início";
                else
                    pagina.Text = navegador.CoreWebView2.DocumentTitle;
            };

            navegador.NavigationCompleted += (s, e) =>
            {
                string urlAtual = navegador.Source?.ToString() ?? "";
                string titulo = navegador.CoreWebView2?.DocumentTitle ?? urlAtual;

                if (!string.IsNullOrWhiteSpace(urlAtual) && urlAtual != "about:blank")
                {
                    txtUrl.Text = urlAtual;
                    alternarSelecaoTxtUrl = true;
                }

                if (!string.IsNullOrWhiteSpace(urlAtual) && urlAtual.StartsWith("http"))
                {
                    HistoricoManager.Salvar(urlAtual, titulo);
                    AtualizarAbaHistorico();
                }

                AtualizarBotoes();
            };

            navegador.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                if (msg == "limpar")
                {
                    HistoricoManager.Limpar();
                    navegador.CoreWebView2.NavigateToString(GerarHtmlHistorico());
                }
                else if (msg.StartsWith("buscar:"))
                {
                    string termo = msg.Substring("buscar:".Length).Trim();

                    if (!string.IsNullOrWhiteSpace(termo))
                    {
                        string urlBusca = "https://www.google.com/search?q=" + Uri.EscapeDataString(termo);
                        _ = CriarNovaAba(urlBusca);
                    }
                }
                else if (msg.StartsWith("abrir:"))
                {
                    string endereco = msg.Substring("abrir:".Length).Trim();

                    if (!string.IsNullOrWhiteSpace(endereco))
                    {
                        if (!endereco.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                            !endereco.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            endereco = "https://" + endereco;
                        }

                        _ = CriarNovaAba(endereco);
                    }
                }
            };
            navegador.CoreWebView2.NavigateToString(GerarHtmlHistorico());
        }

        private async System.Threading.Tasks.Task CriarAbaJogoOffline()
        {
            TabPage pagina = new TabPage("Jogo offline");
            pagina.BackColor = Color.FromArgb(18, 22, 34);

            WebView2 navegador = new WebView2
            {
                Dock = DockStyle.Fill
            };

            pagina.Controls.Add(navegador);
            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;

            var env = await CoreWebView2Environment.CreateAsync(null, pastaDadosApp);
            await navegador.EnsureCoreWebView2Async(env);
            navegador.CoreWebView2.NavigateToString(GerarHtmlJogoOffline());
        }

        private string GerarHtmlJogoOffline()
        {
            return """
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Jogo offline</title>
  <style>
    html, body {
      margin: 0;
      height: 100%;
      overflow: hidden;
      background: radial-gradient(circle at top, #1d3557 0%, #0a1120 48%, #04070f 100%);
      font-family: "Segoe UI", sans-serif;
      color: white;
    }
    .hud {
      position: absolute;
      top: 14px;
      left: 16px;
      right: 16px;
      display: flex;
      justify-content: space-between;
      font-size: 14px;
      z-index: 2;
    }
    .pill {
      background: rgba(255,255,255,0.10);
      border: 1px solid rgba(255,255,255,0.12);
      border-radius: 999px;
      padding: 8px 12px;
      backdrop-filter: blur(6px);
    }
    .hint {
      position: absolute;
      bottom: 14px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 2;
      font-size: 13px;
      color: #d7e3ff;
      background: rgba(0,0,0,0.24);
      border-radius: 999px;
      padding: 8px 14px;
      white-space: nowrap;
    }
    canvas {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  </style>
</head>
<body>
  <div class="hud">
    <div class="pill">Atlas Arcade</div>
    <div class="pill" id="score">Pontos: 0</div>
  </div>
  <canvas id="game"></canvas>
  <div class="hint">Setas ou A/D para mover, Espaço para atirar, Enter para reiniciar</div>
  <script>
    const canvas = document.getElementById('game');
    const ctx = canvas.getContext('2d');
    const scoreEl = document.getElementById('score');

    let started = false;

    function resize() {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;

      if (player) {
        player.x = Math.max(20, Math.min(canvas.width - 20, player.x || canvas.width / 2));
        player.y = canvas.height - 70;
      }
    }
    window.addEventListener('resize', resize);

    const keys = new Set();
    window.addEventListener('keydown', e => {
      const key = e.key.toLowerCase();
      keys.add(key);
      if (['arrowleft', 'arrowright', ' ', 'enter', 'a', 'd'].includes(key))
        e.preventDefault();
      if (gameOver && key === 'enter')
        resetGame();
    });
    window.addEventListener('keyup', e => keys.delete(e.key.toLowerCase()));

    let player, bullets, enemies, particles, score, spawnTimer, gameOver;

    function resetGame() {
      player = {
        x: canvas.width / 2,
        y: canvas.height - 70,
        w: 28,
        h: 28,
        speed: 6,
        cooldown: 0
      };
      bullets = [];
      enemies = [];
      particles = [];
      score = 0;
      spawnTimer = 0;
      gameOver = false;
      scoreEl.textContent = 'Pontos: 0';
    }

    function spawnEnemy() {
      const size = 18 + Math.random() * 20;
      enemies.push({
        x: 20 + Math.random() * (canvas.width - 40),
        y: -size,
        r: size / 2,
        speed: 1.6 + Math.random() * 2.2
      });
    }

    function shoot() {
      if (player.cooldown > 0 || gameOver) return;
      bullets.push({ x: player.x, y: player.y - 14, speed: 9 });
      player.cooldown = 12;
    }

    function burst(x, y, color) {
      for (let i = 0; i < 14; i++) {
        particles.push({
          x, y,
          vx: (Math.random() - 0.5) * 5,
          vy: (Math.random() - 0.5) * 5,
          life: 24 + Math.random() * 12,
          color
        });
      }
    }

    function update() {
      if (gameOver) {
        particles.forEach(p => {
          p.x += p.vx;
          p.y += p.vy;
          p.life -= 1;
        });
        particles = particles.filter(p => p.life > 0);
        return;
      }

      if (keys.has('arrowleft') || keys.has('a')) player.x -= player.speed;
      if (keys.has('arrowright') || keys.has('d')) player.x += player.speed;
      player.x = Math.max(20, Math.min(canvas.width - 20, player.x));

      if (keys.has(' ')) shoot();
      if (player.cooldown > 0) player.cooldown--;

      bullets.forEach(b => b.y -= b.speed);
      bullets = bullets.filter(b => b.y > -20);

      spawnTimer--;
      if (spawnTimer <= 0) {
        spawnEnemy();
        spawnTimer = Math.max(18, 54 - Math.floor(score / 6));
      }

      enemies.forEach(enemy => enemy.y += enemy.speed);

      for (let i = enemies.length - 1; i >= 0; i--) {
        const enemy = enemies[i];
        for (let j = bullets.length - 1; j >= 0; j--) {
          const bullet = bullets[j];
          const dx = bullet.x - enemy.x;
          const dy = bullet.y - enemy.y;
          if (Math.hypot(dx, dy) < enemy.r + 4) {
            burst(enemy.x, enemy.y, '#ffd166');
            enemies.splice(i, 1);
            bullets.splice(j, 1);
            score++;
            scoreEl.textContent = 'Pontos: ' + score;
            break;
          }
        }
      }

      enemies = enemies.filter(enemy => {
        const hitPlayer =
          enemy.x + enemy.r > player.x - player.w / 2 &&
          enemy.x - enemy.r < player.x + player.w / 2 &&
          enemy.y + enemy.r > player.y - player.h / 2 &&
          enemy.y - enemy.r < player.y + player.h / 2;

        if (hitPlayer) {
          gameOver = true;
          burst(player.x, player.y, '#ff4d6d');
          return false;
        }

        return enemy.y <= canvas.height + 40;
      });

      particles.forEach(p => {
        p.x += p.vx;
        p.y += p.vy;
        p.life -= 1;
      });
      particles = particles.filter(p => p.life > 0);
    }

    function drawShip() {
      ctx.save();
      ctx.translate(player.x, player.y);
      ctx.fillStyle = '#7bdff2';
      ctx.beginPath();
      ctx.moveTo(0, -18);
      ctx.lineTo(16, 14);
      ctx.lineTo(0, 8);
      ctx.lineTo(-16, 14);
      ctx.closePath();
      ctx.fill();
      ctx.fillStyle = '#c0f5ff';
      ctx.fillRect(-3, -8, 6, 18);
      ctx.restore();
    }

    function render() {
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      for (let i = 0; i < 90; i++) {
        const x = (i * 127.31) % canvas.width;
        const y = (i * 89.17 + performance.now() * 0.03) % canvas.height;
        ctx.fillStyle = 'rgba(255,255,255,0.45)';
        ctx.fillRect(x, y, 2, 2);
      }

      bullets.forEach(b => {
        ctx.fillStyle = '#b8f2e6';
        ctx.fillRect(b.x - 2, b.y - 10, 4, 12);
      });

      enemies.forEach(enemy => {
        ctx.fillStyle = '#ff7b72';
        ctx.beginPath();
        ctx.arc(enemy.x, enemy.y, enemy.r, 0, Math.PI * 2);
        ctx.fill();
      });

      particles.forEach(p => {
        ctx.globalAlpha = Math.max(0, p.life / 36);
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x, p.y, 3, 3);
        ctx.globalAlpha = 1;
      });

      if (!gameOver) {
        drawShip();
      } else {
        ctx.fillStyle = 'rgba(4,7,15,0.72)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = '#ffffff';
        ctx.textAlign = 'center';
        ctx.font = 'bold 34px Segoe UI';
        ctx.fillText('Fim de jogo', canvas.width / 2, canvas.height / 2 - 10);
        ctx.font = '18px Segoe UI';
        ctx.fillText('Pressione Enter para jogar de novo', canvas.width / 2, canvas.height / 2 + 28);
      }
    }

    function loop() {
      update();
      render();
      requestAnimationFrame(loop);
    }

    requestAnimationFrame(() => {
      resize();
      resetGame();
      if (!started) {
        started = true;
        loop();
      }
    });
  </script>
</body>
</html>
""";
        }

        private async System.Threading.Tasks.Task CriarAbaDownloads()
        {
            TabPage pagina = new TabPage("Downloads");
            pagina.BackColor = Color.FromArgb(248, 249, 250);

            WebView2 navegador = new WebView2
            {
                Dock = DockStyle.Fill
            };

            pagina.Controls.Add(navegador);
            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuNavegador");
            var env = await CoreWebView2Environment.CreateAsync(null, pasta);

            await navegador.EnsureCoreWebView2Async(env);
            ConfigurarMenuNativoComExtras(navegador);
            RegistrarDownloads(navegador);
            RegistrarAberturaNovaJanelaEmAba(navegador);

            navegador.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();

                if (msg == "limpar-downloads")
                {
                    DownloadManager.Limpar();
                    navegador.CoreWebView2.NavigateToString(GerarHtmlDownloads());
                }
            };

            navegador.CoreWebView2.NavigateToString(GerarHtmlDownloads());
        }

        // Atualiza o conteúdo da aba "Início" com o histórico atualizado
        private void AtualizarAbaHistorico()
        {
            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (EhAbaAdicionar(aba))
                    continue;

                if (aba.Text == "Início" && aba.Controls.Count > 0 && aba.Controls[0] is WebView2 navegador)
                {
                    navegador.CoreWebView2.NavigateToString(GerarHtmlHistorico());
                    break;
                }
            }
        }

        private void AtualizarAbaDownloads()
        {
            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (EhAbaAdicionar(aba))
                    continue;

                if (aba.Text == "Downloads" && aba.Controls.Count > 0 && aba.Controls[0] is WebView2 navegador)
                {
                    navegador.CoreWebView2.NavigateToString(GerarHtmlDownloads());
                    break;
                }
            }

            AtualizarPainelDownloads();
        }

        private async System.Threading.Tasks.Task CriarAbaFavoritos()
        {
            TabPage pagina = new TabPage("Favoritos");
            pagina.BackColor = Color.FromArgb(248, 249, 250);

            WebView2 navegador = new WebView2
            {
                Dock = DockStyle.Fill
            };

            pagina.Controls.Add(navegador);
            InserirAntesDaAbaAdicionar(pagina);
            tabControl1.SelectedTab = pagina;

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuNavegador");
            var env = await CoreWebView2Environment.CreateAsync(null, pasta);

            await navegador.EnsureCoreWebView2Async(env);
            ConfigurarMenuNativoComExtras(navegador);
            RegistrarDownloads(navegador);
            RegistrarAberturaNovaJanelaEmAba(navegador);

            navegador.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();

                if (msg == "limpar-favoritos")
                {
                    FavoritosManager.Limpar();
                    navegador.CoreWebView2.NavigateToString(GerarHtmlFavoritos());
                }
            };

            navegador.CoreWebView2.NavigateToString(GerarHtmlFavoritos());
        }

        private void AtualizarAbaFavoritos()
        {
            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (EhAbaAdicionar(aba))
                    continue;

                if (aba.Text == "Favoritos" && aba.Controls.Count > 0 && aba.Controls[0] is WebView2 navegador)
                {
                    navegador.CoreWebView2.NavigateToString(GerarHtmlFavoritos());
                    break;
                }
            }
        }

        private void AdicionarPaginaAtualAosFavoritos()
        {
            WebView2 navegador = ObterNavegadorAtual();

            if (navegador?.Source == null)
            {
                MessageBox.Show("Abra uma página para adicionar aos favoritos.");
                return;
            }

            string url = navegador.Source.ToString();

            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Só é possível favoritar páginas da web.");
                return;
            }

            string titulo = navegador.CoreWebView2?.DocumentTitle ?? url;
            bool adicionou = FavoritosManager.Adicionar(url, titulo);

            if (adicionou)
            {
                AtualizarAbaFavoritos();
                MessageBox.Show("Página adicionada aos favoritos.");
            }
            else
            {
                MessageBox.Show("Essa página já está nos favoritos.");
            }
        }

        private void AtualizarPainelDownloads()
        {
            if (flowDownloads == null)
                return;

            flowDownloads.SuspendLayout();
            flowDownloads.Controls.Clear();

            var downloads = DownloadManager.Carregar()
                .OrderByDescending(x => x.DataInicio)
                .Take(5)
                .ToList();

            if (downloads.Count == 0)
            {
                var vazio = new Label
                {
                    AutoSize = false,
                    Width = 290,
                    Height = 30,
                    Margin = new Padding(3, 8, 3, 0),
                    Text = "Nenhum download registrado ainda.",
                    ForeColor = Color.FromArgb(90, 90, 90)
                };

                flowDownloads.Controls.Add(vazio);
                flowDownloads.ResumeLayout();
                return;
            }

            foreach (var item in downloads)
            {
                flowDownloads.Controls.Add(CriarCardDownload(item));
            }

            flowDownloads.ResumeLayout();
        }

        private Control CriarCardDownload(DownloadItem item)
        {
            var card = new Panel
            {
                Width = 290,
                Height = 64,
                Margin = new Padding(3, 3, 3, 10),
                BackColor = Color.White
            };

            var lblArquivo = new Label
            {
                AutoEllipsis = true,
                Location = new Point(0, 4),
                Size = new Size(286, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Text = string.IsNullOrWhiteSpace(item.Arquivo) ? "Arquivo sem nome" : Path.GetFileName(item.Arquivo)
            };

            var lnkAbrir = new LinkLabel
            {
                AutoSize = true,
                Location = new Point(0, 28),
                Text = File.Exists(item.Arquivo) ? "Abrir arquivo" : "Arquivo indisponível",
                LinkColor = Color.FromArgb(11, 92, 196),
                Enabled = File.Exists(item.Arquivo)
            };

            var lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(0, 48),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(110, 110, 110),
                Text = $"{item.Status} - {item.DataInicio:dd/MM HH:mm}"
            };

            if (File.Exists(item.Arquivo))
            {
                lnkAbrir.LinkClicked += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = item.Arquivo,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        MessageBox.Show("Não foi possível abrir esse arquivo.");
                    }
                };
            }

            card.Controls.Add(lblArquivo);
            card.Controls.Add(lnkAbrir);
            card.Controls.Add(lblStatus);

            return card;
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                if (EhAbaAdicionar(tabControl1.TabPages[i]))
                    continue;

                Rectangle tabRect = tabControl1.GetTabRect(i);

                // Clique direito (menu)
                if (e.Button == MouseButtons.Right && tabRect.Contains(e.Location))
                {
                    indiceAbaClicada = i;
                    tabControl1.SelectedIndex = i;
                    menuAba.Show(tabControl1, e.Location);
                    return;
                }

                // Clique esquerdo (fechar ou iniciar drag)
                if (e.Button == MouseButtons.Left)
                {
                    Rectangle closeRect;

                    if (tabControl1 is ChromeTabControl chromeTabs)
                        closeRect = chromeTabs.GetCloseRect(i);
                    else
                        closeRect = Rectangle.Empty;

                    // Se clicou no X → fecha
                    if (closeRect.Contains(e.Location))
                    {
                        if (ContarAbasNormais() > 1)
                        {
                            TabPage aba = tabControl1.TabPages[i];

                            if (aba.Controls.Count > 0 && aba.Controls[0] is WebView2 nav)
                                nav.Dispose();

                            tabControl1.TabPages.RemoveAt(i);
                        }
                        return;
                    }

                    // Começa arrastar
                    if (tabRect.Contains(e.Location))
                    {
                        abaArrastando = i;
                    }
                }
            }
        }

        // Método para criar nova abas
        private void CriarMenuContextoAbas()
        {
            // Ajuda com novo Docker para abrir Inspector
            var itemInspecionar = new ToolStripMenuItem("Inspecionar");
            itemInspecionar.Click += (s, e) =>
            {
                WebView2 nav = ObterNavegadorAtual();
                nav?.CoreWebView2?.OpenDevToolsWindow();
            };

            menuAba = new ContextMenuStrip();

            var itemNovaAba = new ToolStripMenuItem("Nova aba");
            itemNovaAba.Click += (s, e) =>
            {
                _ = CriarAbaHistorico();
            };

            var itemFecharAba = new ToolStripMenuItem("Fechar aba atual");
            itemFecharAba.Click += (s, e) =>
            {
                if (indiceAbaClicada >= 0 && indiceAbaClicada < tabControl1.TabPages.Count)
                {
                    if (ContarAbasNormais() > 1)
                    {
                        TabPage aba = tabControl1.TabPages[indiceAbaClicada];

                        if (EhAbaAdicionar(aba))
                            return;

                        if (aba.Controls.Count > 0 && aba.Controls[0] is WebView2 nav)
                            nav.Dispose();

                        tabControl1.TabPages.RemoveAt(indiceAbaClicada);
                    }
                    else
                    {
                        MessageBox.Show("Não é possível fechar a última aba.");
                    }
                }
            };

            var itemNovaJanela = new ToolStripMenuItem("Abrir em nova janela");
            itemNovaJanela.Click += (s, e) =>
            {
                if (indiceAbaClicada >= 0 && indiceAbaClicada < tabControl1.TabPages.Count)
                {
                    TabPage aba = tabControl1.TabPages[indiceAbaClicada];

                    if (aba.Controls.Count > 0 && aba.Controls[0] is WebView2 nav)
                    {
                        string url = nav.Source?.ToString();

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            Form1 novaJanela = new Form1();
                            novaJanela.Show();
                            _ = novaJanela.AbrirUrlInicial(url);
                        }
                    }
                }
            };

            // Inspector
            menuAba.Items.Add(new ToolStripSeparator());
            menuAba.Items.Add(itemInspecionar);

            menuAba.Items.Add(itemNovaAba);
            menuAba.Items.Add(itemFecharAba);
            menuAba.Items.Add(new ToolStripSeparator());
            menuAba.Items.Add(itemNovaJanela);
        }

        // Método auxiliar para abrir abas
        public async System.Threading.Tasks.Task AbrirUrlInicial(string url)
        {
            if (tabControl1 == null)
                return;

            tabControl1.TabPages.Clear();
            await CriarNovaAba(url);
            GarantirAbaAdicionar();
        }

        // Abrir link em outra aba
        private void CriarMenuContextoLinks()
        {
            menuLink = new ContextMenuStrip();

            var itemNovaAba = new ToolStripMenuItem("Abrir link em nova aba");
            itemNovaAba.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(linkClicado))
                    _ = CriarNovaAba(linkClicado);
            };

            var itemNovaJanela = new ToolStripMenuItem("Abrir link em nova janela");
            itemNovaJanela.Click += async (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(linkClicado))
                {
                    Form1 novaJanela = new Form1();
                    novaJanela.Show();
                    await novaJanela.AbrirUrlInicial(linkClicado);
                }
            };

            menuLink.Items.Add(itemNovaAba);
            menuLink.Items.Add(itemNovaJanela);
        }

        private void ConfigurarMenuNativoComExtras(WebView2 navegador)
        {
            navegador.CoreWebView2.ContextMenuRequested += (sender, e) =>
            {
                try
                {
                    var itemCodigoFonte = navegador.CoreWebView2.Environment.CreateContextMenuItem(
                        "Abrir no editor de código",
                        null,
                        CoreWebView2ContextMenuItemKind.Command);

                    itemCodigoFonte.CustomItemSelected += async (s, ev) =>
                    {
                        await AbrirCodigoFonteDoNavegadorAsync(navegador);
                    };

                    // Sempre deixa essa opção visível no topo do menu.
                    e.MenuItems.Insert(0, itemCodigoFonte);

                    string linkUri = e.ContextMenuTarget.LinkUri;

                    // Só adiciona opções se for um link
                    if (!string.IsNullOrWhiteSpace(linkUri))
                    {
                        var itemNovaAba = navegador.CoreWebView2.Environment.CreateContextMenuItem(
                            "Abrir link em nova aba",
                            null,
                            CoreWebView2ContextMenuItemKind.Command);

                        itemNovaAba.CustomItemSelected += (s, ev) =>
                        {
                            _ = CriarNovaAba(linkUri);
                        };

                        var itemNovaJanela = navegador.CoreWebView2.Environment.CreateContextMenuItem(
                            "Abrir link em nova janela",
                            null,
                            CoreWebView2ContextMenuItemKind.Command);

                        itemNovaJanela.CustomItemSelected += async (s, ev) =>
                        {
                            Form1 novaJanela = new Form1();
                            novaJanela.Show();
                            await novaJanela.AbrirUrlInicial(linkUri);
                        };

                        // Insere no topo do menu
                        e.MenuItems.Insert(0, itemNovaJanela);
                        e.MenuItems.Insert(0, itemNovaAba);
                    }

                    // 🔍 Sempre adiciona "Inspecionar"
                    var itemInspecionar = navegador.CoreWebView2.Environment.CreateContextMenuItem(
                        "Inspecionar",
                        null,
                        CoreWebView2ContextMenuItemKind.Command);

                    itemInspecionar.CustomItemSelected += (s, ev) =>
                    {
                        navegador.CoreWebView2.OpenDevToolsWindow();
                    };

                    // adiciona no final do menu
                    e.MenuItems.Add(itemInspecionar);
                }
                catch
                {
                    // evita crash silencioso
                }
            };
        }

        private void RegistrarDownloads(WebView2 navegador)
        {
            navegador.CoreWebView2.DownloadStarting += (sender, e) =>
            {
                var downloadOperation = e.DownloadOperation;

                var item = new DownloadItem
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Url = downloadOperation.Uri ?? "",
                    Arquivo = downloadOperation.ResultFilePath ?? "",
                    Status = "Em andamento",
                    DataInicio = DateTime.Now
                };

                DownloadManager.SalvarOuAtualizar(item);
                AtualizarAbaDownloads();
                pnlDownloads.Visible = true;
                pnlDownloads.BringToFront();

                downloadOperation.StateChanged += (s, ev) =>
                {
                    item.Arquivo = downloadOperation.ResultFilePath ?? item.Arquivo;
                    item.Status = ObterDescricaoStatusDownload(downloadOperation.State);

                    if (downloadOperation.State == CoreWebView2DownloadState.Completed ||
                        downloadOperation.State == CoreWebView2DownloadState.Interrupted)
                    {
                        item.DataFim = DateTime.Now;
                    }

                    DownloadManager.SalvarOuAtualizar(item);
                    AtualizarAbaDownloads();
                };
            };
        }

        private void RegistrarAberturaNovaJanelaEmAba(WebView2 navegador)
        {
            navegador.CoreWebView2.NewWindowRequested += (sender, e) =>
            {
                string url = e.Uri ?? "";

                if (!string.IsNullOrWhiteSpace(url))
                {
                    e.Handled = true;
                    _ = CriarNovaAba(url);
                }
            };
        }

        private string ObterDescricaoStatusDownload(CoreWebView2DownloadState estado)
        {
            return estado switch
            {
                CoreWebView2DownloadState.InProgress => "Em andamento",
                CoreWebView2DownloadState.Completed => "Concluído",
                CoreWebView2DownloadState.Interrupted => "Interrompido",
                _ => "Desconhecido"
            };
        }

        private void tabControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (abaArrastando < 0 || animacaoTimer.Enabled)
                return;

            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                if (EhAbaAdicionar(tabControl1.TabPages[i]))
                    continue;

                if (i == abaArrastando)
                    continue;

                Rectangle rect = tabControl1.GetTabRect(i);

                if (rect.Contains(e.Location))
                {
                    indiceDestino = i;
                    animProgress = 0f;
                    animacaoTimer.Start();
                    break;
                }
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            abaArrastando = -1;
        }

        private bool EhAbaAdicionar(TabPage aba)
        {
            return string.Equals(aba.Tag as string, "ADD_TAB", StringComparison.Ordinal);
        }

        private void GarantirAbaAdicionar()
        {
            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (EhAbaAdicionar(aba))
                    return;
            }

            var abaAdicionar = new TabPage("+")
            {
                Tag = "ADD_TAB"
            };

            tabControl1.TabPages.Add(abaAdicionar);
        }

        private void InserirAntesDaAbaAdicionar(TabPage pagina)
        {
            GarantirAbaAdicionar();

            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                if (EhAbaAdicionar(tabControl1.TabPages[i]))
                {
                    tabControl1.TabPages.Insert(i, pagina);
                    return;
                }
            }

            tabControl1.TabPages.Add(pagina);
        }

        private int ContarAbasNormais()
        {
            int total = 0;

            foreach (TabPage aba in tabControl1.TabPages)
            {
                if (!EhAbaAdicionar(aba))
                    total++;
            }

            return total;
        }
    }
}

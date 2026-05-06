namespace NavegadorComAbas
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            menuArquivo = new ToolStripMenuItem();
            menuAbrirPagina = new ToolStripMenuItem();
            menuAbrirPaginaWeb = new ToolStripMenuItem();
            menuSalvarPaginaAtual = new ToolStripMenuItem();
            menuSairNavegador = new ToolStripMenuItem();
            menuEditar = new ToolStripMenuItem();
            menuRecortar = new ToolStripMenuItem();
            menuCopiar = new ToolStripMenuItem();
            menuColar = new ToolStripMenuItem();
            menuSelecionarTudo = new ToolStripMenuItem();
            menuModoLightPrincipal = new ToolStripMenuItem();
            menuFerramentas = new ToolStripMenuItem();
            menuModoLight = new ToolStripMenuItem();
            menuAbrirEditor = new ToolStripMenuItem();
            menuJogoOffline = new ToolStripMenuItem();
            menuRascunhos = new ToolStripMenuItem();
            menuCodigoFonte = new ToolStripMenuItem();
            menuFavoritos = new ToolStripMenuItem();
            menuDownloads = new ToolStripMenuItem();
            toolTip1 = new ToolTip(components);
            btnFavoritos = new Button();
            btnModoLight = new Button();
            tabControl1 = new ChromeTabControl();
            panelTop = new Panel();
            txtUrl = new TextBox();
            btnDownloads = new Button();
            btnAvancar = new Button();
            btnVoltar = new Button();
            lnkVerMaisDownloads = new LinkLabel();
            flowDownloads = new FlowLayoutPanel();
            lblDownloadsTitulo = new Label();
            pnlDownloads = new Panel();
            menuStrip1.SuspendLayout();
            panelTop.SuspendLayout();
            pnlDownloads.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuArquivo, menuEditar, menuFerramentas, menuModoLightPrincipal });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(900, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuArquivo
            // 
            menuArquivo.DropDownItems.AddRange(new ToolStripItem[] { menuAbrirPagina, menuAbrirPaginaWeb, menuSalvarPaginaAtual, menuSairNavegador });
            menuArquivo.Name = "menuArquivo";
            menuArquivo.Size = new Size(61, 20);
            menuArquivo.Text = "Arquivo";
            // 
            // menuAbrirPagina
            // 
            menuAbrirPagina.Name = "menuAbrirPagina";
            menuAbrirPagina.Size = new Size(180, 22);
            menuAbrirPagina.Text = "Abrir página";
            menuAbrirPagina.Click += menuAbrirPagina_Click;
            // 
            // menuAbrirPaginaWeb
            // 
            menuAbrirPaginaWeb.Name = "menuAbrirPaginaWeb";
            menuAbrirPaginaWeb.Size = new Size(180, 22);
            menuAbrirPaginaWeb.Text = "Abrir página da web";
            menuAbrirPaginaWeb.Click += menuAbrirPaginaWeb_Click;
            // 
            // menuSalvarPaginaAtual
            // 
            menuSalvarPaginaAtual.Name = "menuSalvarPaginaAtual";
            menuSalvarPaginaAtual.Size = new Size(180, 22);
            menuSalvarPaginaAtual.Text = "Salvar página atual";
            menuSalvarPaginaAtual.Click += menuSalvarPaginaAtual_Click;
            // 
            // menuSairNavegador
            // 
            menuSairNavegador.Name = "menuSairNavegador";
            menuSairNavegador.Size = new Size(180, 22);
            menuSairNavegador.Text = "Sair do navegador";
            menuSairNavegador.Click += menuSairNavegador_Click;
            // 
            // menuEditar
            // 
            menuEditar.DropDownItems.AddRange(new ToolStripItem[] { menuRecortar, menuCopiar, menuColar, menuSelecionarTudo });
            menuEditar.Name = "menuEditar";
            menuEditar.Size = new Size(49, 20);
            menuEditar.Text = "Editar";
            // 
            // menuRecortar
            // 
            menuRecortar.Name = "menuRecortar";
            menuRecortar.ShortcutKeys = Keys.Control | Keys.X;
            menuRecortar.Size = new Size(198, 22);
            menuRecortar.Text = "Recortar";
            menuRecortar.Click += menuRecortar_Click;
            // 
            // menuCopiar
            // 
            menuCopiar.Name = "menuCopiar";
            menuCopiar.ShortcutKeys = Keys.Control | Keys.C;
            menuCopiar.Size = new Size(198, 22);
            menuCopiar.Text = "Copiar";
            menuCopiar.Click += menuCopiar_Click;
            // 
            // menuColar
            // 
            menuColar.Name = "menuColar";
            menuColar.ShortcutKeys = Keys.Control | Keys.V;
            menuColar.Size = new Size(198, 22);
            menuColar.Text = "Colar";
            menuColar.Click += menuColar_Click;
            // 
            // menuSelecionarTudo
            // 
            menuSelecionarTudo.Name = "menuSelecionarTudo";
            menuSelecionarTudo.ShortcutKeys = Keys.Control | Keys.A;
            menuSelecionarTudo.Size = new Size(198, 22);
            menuSelecionarTudo.Text = "Selecionar tudo";
            menuSelecionarTudo.Click += menuSelecionarTudo_Click;
            // 
            // menuModoLightPrincipal
            // 
            menuModoLightPrincipal.Name = "menuModoLightPrincipal";
            menuModoLightPrincipal.Size = new Size(81, 20);
            menuModoLightPrincipal.Text = "Modo Light";
            menuModoLightPrincipal.Click += menuModoLight_Click;
            // 
            // menuFerramentas
            // 
            menuFerramentas.DropDownItems.AddRange(new ToolStripItem[] { menuModoLight, menuAbrirEditor, menuJogoOffline, menuRascunhos, menuCodigoFonte, menuFavoritos, menuDownloads });
            menuFerramentas.Name = "menuFerramentas";
            menuFerramentas.Size = new Size(84, 20);
            menuFerramentas.Text = "Ferramentas";
            // 
            // menuModoLight
            // 
            menuModoLight.Name = "menuModoLight";
            menuModoLight.Size = new Size(228, 22);
            menuModoLight.Text = "Modo light dos sites";
            menuModoLight.Click += menuModoLight_Click;
            // 
            // menuAbrirEditor
            // 
            menuAbrirEditor.Name = "menuAbrirEditor";
            menuAbrirEditor.Size = new Size(228, 22);
            menuAbrirEditor.Text = "Abrir editor";
            menuAbrirEditor.Click += menuAbrirEditor_Click;
            // 
            // menuJogoOffline
            // 
            menuJogoOffline.Name = "menuJogoOffline";
            menuJogoOffline.Size = new Size(228, 22);
            menuJogoOffline.Text = "Jogo offline";
            menuJogoOffline.Click += menuJogoOffline_Click;
            // 
            // menuRascunhos
            // 
            menuRascunhos.Name = "menuRascunhos";
            menuRascunhos.Size = new Size(228, 22);
            menuRascunhos.Text = "Rascunhos";
            menuRascunhos.Click += menuRascunhos_Click;
            // 
            // menuCodigoFonte
            // 
            menuCodigoFonte.Name = "menuCodigoFonte";
            menuCodigoFonte.Size = new Size(228, 22);
            menuCodigoFonte.Text = "Abrir código-fonte da página";
            menuCodigoFonte.Click += menuCodigoFonte_Click;
            // 
            // menuFavoritos
            // 
            menuFavoritos.Name = "menuFavoritos";
            menuFavoritos.Size = new Size(228, 22);
            menuFavoritos.Text = "Favoritos";
            menuFavoritos.Click += menuFavoritos_Click;
            // 
            // menuDownloads
            // 
            menuDownloads.Name = "menuDownloads";
            menuDownloads.Size = new Size(228, 22);
            menuDownloads.Text = "Downloads";
            menuDownloads.Click += menuDownloads_Click;
            // 
            // btnFavoritos
            // 
            btnFavoritos.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFavoritos.FlatStyle = FlatStyle.Flat;
            btnFavoritos.Location = new Point(600, 10);
            btnFavoritos.Name = "btnFavoritos";
            btnFavoritos.Size = new Size(96, 23);
            btnFavoritos.TabIndex = 7;
            btnFavoritos.Text = "Favoritos";
            toolTip1.SetToolTip(btnFavoritos, "Adicionar aos favoritos (Shift + F)");
            btnFavoritos.UseVisualStyleBackColor = true;
            btnFavoritos.Click += btnFavoritos_Click;
            // 
            // btnModoLight
            // 
            btnModoLight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnModoLight.FlatStyle = FlatStyle.Flat;
            btnModoLight.Location = new Point(796, 10);
            btnModoLight.Name = "btnModoLight";
            btnModoLight.Size = new Size(90, 23);
            btnModoLight.TabIndex = 9;
            btnModoLight.Text = "Modo light";
            toolTip1.SetToolTip(btnModoLight, "Economia de memória para sites pesados");
            btnModoLight.UseVisualStyleBackColor = true;
            btnModoLight.Click += btnModoLight_Click;
            // 
            // tabControl1
            // 
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.ItemSize = new Size(220, 34);
            tabControl1.Location = new Point(0, 71);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(18, 6);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(900, 379);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 1;
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            tabControl1.MouseDown += tabControl1_MouseDown;
            tabControl1.MouseMove += tabControl1_MouseMove;
            tabControl1.MouseUp += tabControl1_MouseUp;
            // 
            // panelTop
            // 
            panelTop.Controls.Add(txtUrl);
            panelTop.Controls.Add(btnModoLight);
            panelTop.Controls.Add(btnFavoritos);
            panelTop.Controls.Add(btnDownloads);
            panelTop.Controls.Add(btnAvancar);
            panelTop.Controls.Add(btnVoltar);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 24);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(900, 47);
            panelTop.TabIndex = 0;
            // 
            // txtUrl
            // 
            txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.Font = new Font("Segoe UI", 11F);
            txtUrl.Location = new Point(114, 10);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(480, 27);
            txtUrl.TabIndex = 5;
            txtUrl.KeyDown += TxtUrl_KeyDown;
            txtUrl.MouseUp += TxtUrl_MouseUp;
            // 
            // btnDownloads
            // 
            btnDownloads.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDownloads.FlatStyle = FlatStyle.Flat;
            btnDownloads.Location = new Point(702, 10);
            btnDownloads.Name = "btnDownloads";
            btnDownloads.Size = new Size(88, 23);
            btnDownloads.TabIndex = 8;
            btnDownloads.Text = "Downloads";
            btnDownloads.UseVisualStyleBackColor = true;
            btnDownloads.Click += btnDownloads_Click;
            // 
            // btnAvancar
            // 
            btnAvancar.FlatStyle = FlatStyle.Flat;
            btnAvancar.Location = new Point(63, 11);
            btnAvancar.Name = "btnAvancar";
            btnAvancar.Size = new Size(45, 23);
            btnAvancar.TabIndex = 1;
            btnAvancar.Text = ">";
            btnAvancar.UseVisualStyleBackColor = true;
            btnAvancar.Click += btnAvancar_Click;
            // 
            // btnVoltar
            // 
            btnVoltar.FlatStyle = FlatStyle.Flat;
            btnVoltar.Location = new Point(12, 11);
            btnVoltar.Name = "btnVoltar";
            btnVoltar.Size = new Size(45, 23);
            btnVoltar.TabIndex = 0;
            btnVoltar.Text = "<";
            btnVoltar.UseVisualStyleBackColor = true;
            btnVoltar.Click += btnVoltar_Click;
            // 
            // lnkVerMaisDownloads
            // 
            lnkVerMaisDownloads.ActiveLinkColor = Color.FromArgb(11, 92, 196);
            lnkVerMaisDownloads.AutoSize = true;
            lnkVerMaisDownloads.LinkColor = Color.FromArgb(11, 92, 196);
            lnkVerMaisDownloads.Location = new Point(17, 311);
            lnkVerMaisDownloads.Name = "lnkVerMaisDownloads";
            lnkVerMaisDownloads.Size = new Size(51, 15);
            lnkVerMaisDownloads.TabIndex = 2;
            lnkVerMaisDownloads.TabStop = true;
            lnkVerMaisDownloads.Text = "Ver mais";
            lnkVerMaisDownloads.LinkClicked += lnkVerMaisDownloads_LinkClicked;
            // 
            // flowDownloads
            // 
            flowDownloads.AutoScroll = true;
            flowDownloads.FlowDirection = FlowDirection.TopDown;
            flowDownloads.Location = new Point(17, 44);
            flowDownloads.Name = "flowDownloads";
            flowDownloads.Size = new Size(316, 253);
            flowDownloads.TabIndex = 1;
            flowDownloads.WrapContents = false;
            // 
            // lblDownloadsTitulo
            // 
            lblDownloadsTitulo.AutoSize = true;
            lblDownloadsTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDownloadsTitulo.Location = new Point(15, 14);
            lblDownloadsTitulo.Name = "lblDownloadsTitulo";
            lblDownloadsTitulo.Size = new Size(84, 20);
            lblDownloadsTitulo.TabIndex = 0;
            lblDownloadsTitulo.Text = "Downloads";
            // 
            // pnlDownloads
            // 
            pnlDownloads.BackColor = Color.White;
            pnlDownloads.BorderStyle = BorderStyle.FixedSingle;
            pnlDownloads.Controls.Add(lnkVerMaisDownloads);
            pnlDownloads.Controls.Add(flowDownloads);
            pnlDownloads.Controls.Add(lblDownloadsTitulo);
            pnlDownloads.Location = new Point(536, 53);
            pnlDownloads.Name = "pnlDownloads";
            pnlDownloads.Size = new Size(350, 340);
            pnlDownloads.TabIndex = 2;
            pnlDownloads.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 450);
            Controls.Add(pnlDownloads);
            Controls.Add(tabControl1);
            Controls.Add(panelTop);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Atlas";
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            pnlDownloads.ResumeLayout(false);
            pnlDownloads.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuArquivo;
        private ToolStripMenuItem menuAbrirPagina;
        private ToolStripMenuItem menuAbrirPaginaWeb;
        private ToolStripMenuItem menuSalvarPaginaAtual;
        private ToolStripMenuItem menuSairNavegador;
        private ToolStripMenuItem menuEditar;
        private ToolStripMenuItem menuRecortar;
        private ToolStripMenuItem menuCopiar;
        private ToolStripMenuItem menuColar;
        private ToolStripMenuItem menuSelecionarTudo;
        private ToolStripMenuItem menuModoLightPrincipal;
        private ToolStripMenuItem menuFerramentas;
        private ToolStripMenuItem menuModoLight;
        private ToolStripMenuItem menuAbrirEditor;
        private ToolStripMenuItem menuJogoOffline;
        private ToolStripMenuItem menuRascunhos;
        private ToolStripMenuItem menuCodigoFonte;
        private ToolStripMenuItem menuFavoritos;
        private ToolStripMenuItem menuDownloads;
        private ToolTip toolTip1;
        private Panel panelTop;
        private ChromeTabControl tabControl1;
        private Button btnAvancar;
        private Button btnVoltar;
        private TextBox txtUrl;
        private Button btnFavoritos;
        private Button btnDownloads;
        private Button btnModoLight;
        private Panel pnlDownloads;
        private Label lblDownloadsTitulo;
        private FlowLayoutPanel flowDownloads;
        private LinkLabel lnkVerMaisDownloads;

    }
}

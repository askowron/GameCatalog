namespace GameCatalog.App.Forms
{
    partial class PrefsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlPages = new System.Windows.Forms.Panel();
            this.pnlPageGeneral = new System.Windows.Forms.Panel();
            this.chkForceFullRefresh = new System.Windows.Forms.CheckBox();
            this.chkNaturalSort = new System.Windows.Forms.CheckBox();
            this.lblSectionGeneral = new System.Windows.Forms.Label();
            this.lblAutoNumberingNote = new System.Windows.Forms.Label();
            this.chkAutoNumbering = new System.Windows.Forms.CheckBox();
            this.lblSectionNumbering = new System.Windows.Forms.Label();
            this.pnlPageIcons = new System.Windows.Forms.Panel();
            this.chkMenuIcons = new System.Windows.Forms.CheckBox();
            this.tlpIcons = new System.Windows.Forms.TableLayoutPanel();
            this.lblTheme = new System.Windows.Forms.Label();
            this.cbxTheme = new System.Windows.Forms.ComboBox();
            this.lblColorScheme = new System.Windows.Forms.Label();
            this.cbxColorScheme = new System.Windows.Forms.ComboBox();
            this.lblSectionToolbar = new System.Windows.Forms.Label();
            this.pnlPageFiles = new System.Windows.Forms.Panel();
            this.chkBackupOnSave = new System.Windows.Forms.CheckBox();
            this.chkReopenLastFile = new System.Windows.Forms.CheckBox();
            this.lblSectionCatalog = new System.Windows.Forms.Label();
            this.pnlPageList = new System.Windows.Forms.Panel();
            this.chkShowGridLines = new System.Windows.Forms.CheckBox();
            this.chkFullRowSelect = new System.Windows.Forms.CheckBox();
            this.lblSectionListLook = new System.Windows.Forms.Label();
            this.pnlPageTitleFormat = new System.Windows.Forms.Panel();
            this.cbxTitleFormat = new System.Windows.Forms.ComboBox();
            this.lblSectionTitleFormat = new System.Windows.Forms.Label();
            this.pnlPageGrouping = new System.Windows.Forms.Panel();
            this.chkShowGroupCount = new System.Windows.Forms.CheckBox();
            this.pnlPageExtras = new System.Windows.Forms.Panel();
            this.chkShowExtraCategory = new System.Windows.Forms.CheckBox();
            this.pnlPageGame = new System.Windows.Forms.Panel();
            this.chkCoverVisible = new System.Windows.Forms.CheckBox();
            this.pnlPageLists = new System.Windows.Forms.Panel();
            this.lstListValues = new System.Windows.Forms.ListBox();
            this.pnlListEdit = new System.Windows.Forms.Panel();
            this.txtNewValue = new System.Windows.Forms.TextBox();
            this.btnListDelete = new System.Windows.Forms.Button();
            this.btnListAdd = new System.Windows.Forms.Button();
            this.cbxListKeys = new System.Windows.Forms.ComboBox();
            this.lblSectionLists = new System.Windows.Forms.Label();
            this.pnlPageImportMedia = new System.Windows.Forms.Panel();
            this.lblPatternsHint = new System.Windows.Forms.Label();
            this.txtMediaPatterns = new System.Windows.Forms.TextBox();
            this.lblSectionPatterns = new System.Windows.Forms.Label();
            this.pnlPageImportPictures = new System.Windows.Forms.Panel();
            this.tlpCover = new System.Windows.Forms.TableLayoutPanel();
            this.lblMaxCoverWidth = new System.Windows.Forms.Label();
            this.numMaxCoverWidth = new System.Windows.Forms.NumericUpDown();
            this.lblSectionCovers = new System.Windows.Forms.Label();
            this.pnlPageSearch = new System.Windows.Forms.Panel();
            this.lstSites = new System.Windows.Forms.ListBox();
            this.tlpSite = new System.Windows.Forms.TableLayoutPanel();
            this.lblSiteName = new System.Windows.Forms.Label();
            this.txtSiteName = new System.Windows.Forms.TextBox();
            this.lblSiteUrl = new System.Windows.Forms.Label();
            this.txtSiteUrl = new System.Windows.Forms.TextBox();
            this.pnlSiteButtons = new System.Windows.Forms.Panel();
            this.btnSiteDefaults = new System.Windows.Forms.Button();
            this.btnSiteDelete = new System.Windows.Forms.Button();
            this.btnSiteAdd = new System.Windows.Forms.Button();
            this.lblSectionSearch = new System.Windows.Forms.Label();
            this.pnlPageScripts = new System.Windows.Forms.Panel();
            this.tlpScripts = new System.Windows.Forms.TableLayoutPanel();
            this.lblScriptTimeout = new System.Windows.Forms.Label();
            this.numScriptTimeout = new System.Windows.Forms.NumericUpDown();
            this.lblSectionScripts = new System.Windows.Forms.Label();
            this.pnlPageExport = new System.Windows.Forms.Panel();
            this.cbxCsvDelimiter = new System.Windows.Forms.ComboBox();
            this.lblSectionCsv = new System.Windows.Forms.Label();
            this.cbxHtmlTemplate = new System.Windows.Forms.ComboBox();
            this.lblSectionHtml = new System.Windows.Forms.Label();
            this.pnlPageFolders = new System.Windows.Forms.Panel();
            this.txtScriptsFolder = new System.Windows.Forms.TextBox();
            this.lblScriptsFolder = new System.Windows.Forms.Label();
            this.txtDataFolder = new System.Windows.Forms.TextBox();
            this.lblDataFolder = new System.Windows.Forms.Label();
            this.lblSectionLocations = new System.Windows.Forms.Label();
            this.tvPages = new System.Windows.Forms.TreeView();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.localizer = new GameCatalog.UI.Components.Localizer(this.components);
            this.pnlPages.SuspendLayout();
            this.pnlPageGeneral.SuspendLayout();
            this.pnlPageIcons.SuspendLayout();
            this.tlpIcons.SuspendLayout();
            this.pnlPageFiles.SuspendLayout();
            this.pnlPageList.SuspendLayout();
            this.pnlPageTitleFormat.SuspendLayout();
            this.pnlPageGrouping.SuspendLayout();
            this.pnlPageExtras.SuspendLayout();
            this.pnlPageGame.SuspendLayout();
            this.pnlPageLists.SuspendLayout();
            this.pnlListEdit.SuspendLayout();
            this.pnlPageImportMedia.SuspendLayout();
            this.pnlPageImportPictures.SuspendLayout();
            this.tlpCover.SuspendLayout();
            this.pnlPageSearch.SuspendLayout();
            this.tlpSite.SuspendLayout();
            this.pnlSiteButtons.SuspendLayout();
            this.pnlPageScripts.SuspendLayout();
            this.tlpScripts.SuspendLayout();
            this.pnlPageExport.SuspendLayout();
            this.pnlPageFolders.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCoverWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScriptTimeout)).BeginInit();
            this.SuspendLayout();
            //
            // pnlPages
            //
            this.pnlPages.Controls.Add(this.pnlPageGeneral);
            this.pnlPages.Controls.Add(this.pnlPageIcons);
            this.pnlPages.Controls.Add(this.pnlPageFiles);
            this.pnlPages.Controls.Add(this.pnlPageList);
            this.pnlPages.Controls.Add(this.pnlPageTitleFormat);
            this.pnlPages.Controls.Add(this.pnlPageGrouping);
            this.pnlPages.Controls.Add(this.pnlPageExtras);
            this.pnlPages.Controls.Add(this.pnlPageGame);
            this.pnlPages.Controls.Add(this.pnlPageLists);
            this.pnlPages.Controls.Add(this.pnlPageImportMedia);
            this.pnlPages.Controls.Add(this.pnlPageImportPictures);
            this.pnlPages.Controls.Add(this.pnlPageSearch);
            this.pnlPages.Controls.Add(this.pnlPageScripts);
            this.pnlPages.Controls.Add(this.pnlPageExport);
            this.pnlPages.Controls.Add(this.pnlPageFolders);
            this.pnlPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPages.Name = "pnlPages";
            this.pnlPages.Padding = new System.Windows.Forms.Padding(10);
            //
            // pnlPageGeneral
            //
            this.pnlPageGeneral.Controls.Add(this.chkForceFullRefresh);
            this.pnlPageGeneral.Controls.Add(this.chkNaturalSort);
            this.pnlPageGeneral.Controls.Add(this.lblSectionGeneral);
            this.pnlPageGeneral.Controls.Add(this.lblAutoNumberingNote);
            this.pnlPageGeneral.Controls.Add(this.chkAutoNumbering);
            this.pnlPageGeneral.Controls.Add(this.lblSectionNumbering);
            this.pnlPageGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageGeneral.Name = "pnlPageGeneral";
            this.pnlPageGeneral.Visible = false;
            //
            // chkForceFullRefresh
            //
            this.chkForceFullRefresh.AutoSize = true;
            this.chkForceFullRefresh.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkForceFullRefresh.Name = "chkForceFullRefresh";
            this.chkForceFullRefresh.Text = "Wymuś odświeżanie całej listy gier po każdej zmianie wartości pola";
            this.chkForceFullRefresh.UseVisualStyleBackColor = true;
            //
            // chkNaturalSort
            //
            this.chkNaturalSort.AutoSize = true;
            this.chkNaturalSort.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkNaturalSort.Name = "chkNaturalSort";
            this.chkNaturalSort.Text = "Użyj naturalnego porównania do sortowania tytułów (liczby po wartości)";
            this.chkNaturalSort.UseVisualStyleBackColor = true;
            //
            // lblSectionGeneral
            //
            this.lblSectionGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionGeneral.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionGeneral.Name = "lblSectionGeneral";
            this.lblSectionGeneral.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionGeneral.Size = new System.Drawing.Size(434, 24);
            this.lblSectionGeneral.Text = "Opcje ogólne";
            //
            // lblAutoNumberingNote
            //
            this.lblAutoNumberingNote.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAutoNumberingNote.Name = "lblAutoNumberingNote";
            this.lblAutoNumberingNote.Size = new System.Drawing.Size(434, 30);
            this.lblAutoNumberingNote.Text = "Gdy wyłączone, dodawanie gry pyta o numer („Wybór numeru gry” jak w AMC).";
            //
            // chkAutoNumbering
            //
            this.chkAutoNumbering.AutoSize = true;
            this.chkAutoNumbering.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAutoNumbering.Name = "chkAutoNumbering";
            this.chkAutoNumbering.Text = "Automatyczne numerowanie gier";
            this.chkAutoNumbering.UseVisualStyleBackColor = true;
            //
            // lblSectionNumbering
            //
            this.lblSectionNumbering.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionNumbering.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionNumbering.Name = "lblSectionNumbering";
            this.lblSectionNumbering.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionNumbering.Size = new System.Drawing.Size(434, 24);
            this.lblSectionNumbering.Text = "Numerowanie";
            //
            // pnlPageIcons
            //
            this.pnlPageIcons.Controls.Add(this.chkMenuIcons);
            this.pnlPageIcons.Controls.Add(this.tlpIcons);
            this.pnlPageIcons.Controls.Add(this.lblSectionToolbar);
            this.pnlPageIcons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageIcons.Name = "pnlPageIcons";
            this.pnlPageIcons.Visible = false;
            //
            // chkMenuIcons
            //
            this.chkMenuIcons.AutoSize = true;
            this.chkMenuIcons.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkMenuIcons.Name = "chkMenuIcons";
            this.chkMenuIcons.Text = "Ikony w menu";
            this.chkMenuIcons.UseVisualStyleBackColor = true;
            //
            // tlpIcons
            //
            this.tlpIcons.AutoSize = true;
            this.tlpIcons.ColumnCount = 2;
            this.tlpIcons.Controls.Add(this.lblTheme, 0, 0);
            this.tlpIcons.Controls.Add(this.cbxTheme, 1, 0);
            this.tlpIcons.Controls.Add(this.lblColorScheme, 0, 1);
            this.tlpIcons.Controls.Add(this.cbxColorScheme, 1, 1);
            this.tlpIcons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpIcons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIcons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpIcons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpIcons.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpIcons.Name = "tlpIcons";
            this.tlpIcons.RowCount = 2;
            //
            // lblTheme
            //
            this.lblTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTheme.AutoSize = true;
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Text = "Zestaw ikon:";
            //
            // cbxTheme
            //
            this.cbxTheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbxTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTheme.Name = "cbxTheme";
            //
            // lblColorScheme
            //
            this.lblColorScheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblColorScheme.AutoSize = true;
            this.lblColorScheme.Name = "lblColorScheme";
            this.lblColorScheme.Text = "Schemat kolorowania:";
            //
            // cbxColorScheme
            //
            this.cbxColorScheme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbxColorScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColorScheme.Name = "cbxColorScheme";
            //
            // lblSectionToolbar
            //
            this.lblSectionToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionToolbar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionToolbar.Name = "lblSectionToolbar";
            this.lblSectionToolbar.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionToolbar.Size = new System.Drawing.Size(434, 24);
            this.lblSectionToolbar.Text = "Menu i Pasek narzędziowy";
            //
            // pnlPageFiles
            //
            this.pnlPageFiles.Controls.Add(this.chkBackupOnSave);
            this.pnlPageFiles.Controls.Add(this.chkReopenLastFile);
            this.pnlPageFiles.Controls.Add(this.lblSectionCatalog);
            this.pnlPageFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageFiles.Name = "pnlPageFiles";
            this.pnlPageFiles.Visible = false;
            //
            // chkBackupOnSave
            //
            this.chkBackupOnSave.AutoSize = true;
            this.chkBackupOnSave.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkBackupOnSave.Name = "chkBackupOnSave";
            this.chkBackupOnSave.Text = "Twórz kopię zapasową (.bak) przy zapisie";
            this.chkBackupOnSave.UseVisualStyleBackColor = true;
            //
            // chkReopenLastFile
            //
            this.chkReopenLastFile.AutoSize = true;
            this.chkReopenLastFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkReopenLastFile.Name = "chkReopenLastFile";
            this.chkReopenLastFile.Text = "Otwieraj ostatni katalog przy starcie";
            this.chkReopenLastFile.UseVisualStyleBackColor = true;
            //
            // lblSectionCatalog
            //
            this.lblSectionCatalog.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionCatalog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionCatalog.Name = "lblSectionCatalog";
            this.lblSectionCatalog.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionCatalog.Size = new System.Drawing.Size(434, 24);
            this.lblSectionCatalog.Text = "Katalog";
            //
            // pnlPageList
            //
            this.pnlPageList.Controls.Add(this.chkShowGridLines);
            this.pnlPageList.Controls.Add(this.chkFullRowSelect);
            this.pnlPageList.Controls.Add(this.lblSectionListLook);
            this.pnlPageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageList.Name = "pnlPageList";
            this.pnlPageList.Visible = false;
            //
            // chkShowGridLines
            //
            this.chkShowGridLines.AutoSize = true;
            this.chkShowGridLines.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkShowGridLines.Name = "chkShowGridLines";
            this.chkShowGridLines.Text = "Linie siatki";
            this.chkShowGridLines.UseVisualStyleBackColor = true;
            //
            // chkFullRowSelect
            //
            this.chkFullRowSelect.AutoSize = true;
            this.chkFullRowSelect.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkFullRowSelect.Name = "chkFullRowSelect";
            this.chkFullRowSelect.Text = "Zaznaczanie pełnych wierszy";
            this.chkFullRowSelect.UseVisualStyleBackColor = true;
            //
            // lblSectionListLook
            //
            this.lblSectionListLook.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionListLook.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionListLook.Name = "lblSectionListLook";
            this.lblSectionListLook.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionListLook.Size = new System.Drawing.Size(434, 24);
            this.lblSectionListLook.Text = "Wygląd listy";
            //
            // pnlPageTitleFormat
            //
            this.pnlPageTitleFormat.Controls.Add(this.cbxTitleFormat);
            this.pnlPageTitleFormat.Controls.Add(this.lblSectionTitleFormat);
            this.pnlPageTitleFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageTitleFormat.Name = "pnlPageTitleFormat";
            this.pnlPageTitleFormat.Visible = false;
            //
            // cbxTitleFormat
            //
            this.cbxTitleFormat.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxTitleFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTitleFormat.Name = "cbxTitleFormat";
            //
            // lblSectionTitleFormat
            //
            this.lblSectionTitleFormat.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionTitleFormat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionTitleFormat.Name = "lblSectionTitleFormat";
            this.lblSectionTitleFormat.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionTitleFormat.Size = new System.Drawing.Size(434, 24);
            this.lblSectionTitleFormat.Text = "Tytuł na liście gier";
            //
            // pnlPageGrouping
            //
            this.pnlPageGrouping.Controls.Add(this.chkShowGroupCount);
            this.pnlPageGrouping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageGrouping.Name = "pnlPageGrouping";
            this.pnlPageGrouping.Visible = false;
            //
            // chkShowGroupCount
            //
            this.chkShowGroupCount.AutoSize = true;
            this.chkShowGroupCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkShowGroupCount.Name = "chkShowGroupCount";
            this.chkShowGroupCount.Text = "Pokazuj liczebność grup na liście";
            this.chkShowGroupCount.UseVisualStyleBackColor = true;
            //
            // pnlPageExtras
            //
            this.pnlPageExtras.Controls.Add(this.chkShowExtraCategory);
            this.pnlPageExtras.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageExtras.Name = "pnlPageExtras";
            this.pnlPageExtras.Visible = false;
            //
            // chkShowExtraCategory
            //
            this.chkShowExtraCategory.AutoSize = true;
            this.chkShowExtraCategory.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkShowExtraCategory.Name = "chkShowExtraCategory";
            this.chkShowExtraCategory.Text = "Pokazuj kolumnę kategorii dodatków";
            this.chkShowExtraCategory.UseVisualStyleBackColor = true;
            //
            // pnlPageGame
            //
            this.pnlPageGame.Controls.Add(this.chkCoverVisible);
            this.pnlPageGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageGame.Name = "pnlPageGame";
            this.pnlPageGame.Visible = false;
            //
            // chkCoverVisible
            //
            this.chkCoverVisible.AutoSize = true;
            this.chkCoverVisible.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkCoverVisible.Name = "chkCoverVisible";
            this.chkCoverVisible.Text = "Pokazuj panel okładki";
            this.chkCoverVisible.UseVisualStyleBackColor = true;
            //
            // pnlPageLists
            //
            this.pnlPageLists.Controls.Add(this.lstListValues);
            this.pnlPageLists.Controls.Add(this.pnlListEdit);
            this.pnlPageLists.Controls.Add(this.cbxListKeys);
            this.pnlPageLists.Controls.Add(this.lblSectionLists);
            this.pnlPageLists.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageLists.Name = "pnlPageLists";
            this.pnlPageLists.Visible = false;
            //
            // lstListValues
            //
            this.lstListValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstListValues.IntegralHeight = false;
            this.lstListValues.Name = "lstListValues";
            //
            // pnlListEdit
            //
            this.pnlListEdit.Controls.Add(this.txtNewValue);
            this.pnlListEdit.Controls.Add(this.btnListDelete);
            this.pnlListEdit.Controls.Add(this.btnListAdd);
            this.pnlListEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlListEdit.Name = "pnlListEdit";
            this.pnlListEdit.Size = new System.Drawing.Size(434, 30);
            //
            // txtNewValue
            //
            this.txtNewValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNewValue.Name = "txtNewValue";
            //
            // btnListDelete
            //
            this.btnListDelete.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnListDelete.Name = "btnListDelete";
            this.btnListDelete.Size = new System.Drawing.Size(70, 23);
            this.btnListDelete.Text = "Usuń";
            this.btnListDelete.UseVisualStyleBackColor = true;
            //
            // btnListAdd
            //
            this.btnListAdd.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnListAdd.Name = "btnListAdd";
            this.btnListAdd.Size = new System.Drawing.Size(70, 23);
            this.btnListAdd.Text = "Dodaj";
            this.btnListAdd.UseVisualStyleBackColor = true;
            //
            // cbxListKeys
            //
            this.cbxListKeys.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxListKeys.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxListKeys.Name = "cbxListKeys";
            //
            // lblSectionLists
            //
            this.lblSectionLists.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionLists.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionLists.Name = "lblSectionLists";
            this.lblSectionLists.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionLists.Size = new System.Drawing.Size(434, 24);
            this.lblSectionLists.Text = "Wartości list rozwijanych";
            //
            // pnlPageImportMedia
            //
            this.pnlPageImportMedia.Controls.Add(this.lblPatternsHint);
            this.pnlPageImportMedia.Controls.Add(this.txtMediaPatterns);
            this.pnlPageImportMedia.Controls.Add(this.lblSectionPatterns);
            this.pnlPageImportMedia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageImportMedia.Name = "pnlPageImportMedia";
            this.pnlPageImportMedia.Visible = false;
            //
            // lblPatternsHint
            //
            this.lblPatternsHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPatternsHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblPatternsHint.Name = "lblPatternsHint";
            this.lblPatternsHint.Size = new System.Drawing.Size(434, 22);
            this.lblPatternsHint.Text = "Rozdzielone średnikiem, np. *.exe;*.lnk";
            //
            // txtMediaPatterns
            //
            this.txtMediaPatterns.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtMediaPatterns.Name = "txtMediaPatterns";
            //
            // lblSectionPatterns
            //
            this.lblSectionPatterns.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionPatterns.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionPatterns.Name = "lblSectionPatterns";
            this.lblSectionPatterns.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionPatterns.Size = new System.Drawing.Size(434, 24);
            this.lblSectionPatterns.Text = "Wzorce plików wykonywalnych";
            //
            // pnlPageImportPictures
            //
            this.pnlPageImportPictures.Controls.Add(this.tlpCover);
            this.pnlPageImportPictures.Controls.Add(this.lblSectionCovers);
            this.pnlPageImportPictures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageImportPictures.Name = "pnlPageImportPictures";
            this.pnlPageImportPictures.Visible = false;
            //
            // tlpCover
            //
            this.tlpCover.AutoSize = true;
            this.tlpCover.ColumnCount = 2;
            this.tlpCover.Controls.Add(this.lblMaxCoverWidth, 0, 0);
            this.tlpCover.Controls.Add(this.numMaxCoverWidth, 1, 0);
            this.tlpCover.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpCover.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCover.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpCover.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpCover.Name = "tlpCover";
            this.tlpCover.RowCount = 1;
            //
            // lblMaxCoverWidth
            //
            this.lblMaxCoverWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMaxCoverWidth.AutoSize = true;
            this.lblMaxCoverWidth.Name = "lblMaxCoverWidth";
            this.lblMaxCoverWidth.Text = "Maks. szerokość okładki (px, 0 = bez zmian):";
            //
            // numMaxCoverWidth
            //
            this.numMaxCoverWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numMaxCoverWidth.Maximum = new decimal(new int[] { 4000, 0, 0, 0 });
            this.numMaxCoverWidth.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numMaxCoverWidth.Name = "numMaxCoverWidth";
            this.numMaxCoverWidth.Value = new decimal(new int[] { 0, 0, 0, 0 });
            //
            // lblSectionCovers
            //
            this.lblSectionCovers.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionCovers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionCovers.Name = "lblSectionCovers";
            this.lblSectionCovers.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionCovers.Size = new System.Drawing.Size(434, 24);
            this.lblSectionCovers.Text = "Wczytywanie okładek";
            //
            // pnlPageSearch
            //
            this.pnlPageSearch.Controls.Add(this.lstSites);
            this.pnlPageSearch.Controls.Add(this.tlpSite);
            this.pnlPageSearch.Controls.Add(this.pnlSiteButtons);
            this.pnlPageSearch.Controls.Add(this.lblSectionSearch);
            this.pnlPageSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageSearch.Name = "pnlPageSearch";
            this.pnlPageSearch.Visible = false;
            //
            // lstSites
            //
            this.lstSites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSites.IntegralHeight = false;
            this.lstSites.Name = "lstSites";
            //
            // tlpSite
            //
            this.tlpSite.ColumnCount = 2;
            this.tlpSite.Controls.Add(this.lblSiteName, 0, 0);
            this.tlpSite.Controls.Add(this.txtSiteName, 1, 0);
            this.tlpSite.Controls.Add(this.lblSiteUrl, 0, 1);
            this.tlpSite.Controls.Add(this.txtSiteUrl, 1, 1);
            this.tlpSite.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSite.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSite.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSite.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpSite.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpSite.Name = "tlpSite";
            this.tlpSite.RowCount = 2;
            this.tlpSite.Size = new System.Drawing.Size(434, 84);
            //
            // lblSiteName
            //
            this.lblSiteName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSiteName.AutoSize = true;
            this.lblSiteName.Name = "lblSiteName";
            this.lblSiteName.Text = "Nazwa:";
            //
            // txtSiteName
            //
            this.txtSiteName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSiteName.Name = "txtSiteName";
            //
            // lblSiteUrl
            //
            this.lblSiteUrl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSiteUrl.AutoSize = true;
            this.lblSiteUrl.Name = "lblSiteUrl";
            this.lblSiteUrl.Text = "Adres URL:";
            //
            // txtSiteUrl
            //
            this.txtSiteUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSiteUrl.Name = "txtSiteUrl";
            //
            // pnlSiteButtons
            //
            this.pnlSiteButtons.Controls.Add(this.btnSiteDefaults);
            this.pnlSiteButtons.Controls.Add(this.btnSiteDelete);
            this.pnlSiteButtons.Controls.Add(this.btnSiteAdd);
            this.pnlSiteButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSiteButtons.Name = "pnlSiteButtons";
            this.pnlSiteButtons.Size = new System.Drawing.Size(434, 30);
            //
            // btnSiteDefaults
            //
            this.btnSiteDefaults.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSiteDefaults.Name = "btnSiteDefaults";
            this.btnSiteDefaults.Size = new System.Drawing.Size(80, 23);
            this.btnSiteDefaults.Text = "Domyślne";
            this.btnSiteDefaults.UseVisualStyleBackColor = true;
            //
            // btnSiteDelete
            //
            this.btnSiteDelete.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSiteDelete.Name = "btnSiteDelete";
            this.btnSiteDelete.Size = new System.Drawing.Size(70, 23);
            this.btnSiteDelete.Text = "Usuń";
            this.btnSiteDelete.UseVisualStyleBackColor = true;
            //
            // btnSiteAdd
            //
            this.btnSiteAdd.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSiteAdd.Name = "btnSiteAdd";
            this.btnSiteAdd.Size = new System.Drawing.Size(70, 23);
            this.btnSiteAdd.Text = "Dodaj";
            this.btnSiteAdd.UseVisualStyleBackColor = true;
            //
            // lblSectionSearch
            //
            this.lblSectionSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionSearch.Name = "lblSectionSearch";
            this.lblSectionSearch.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionSearch.Size = new System.Drawing.Size(434, 24);
            this.lblSectionSearch.Text = "Wyszukiwarki (adres z {q} jako zapytaniem)";
            //
            // pnlPageScripts
            //
            this.pnlPageScripts.Controls.Add(this.tlpScripts);
            this.pnlPageScripts.Controls.Add(this.lblSectionScripts);
            this.pnlPageScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageScripts.Name = "pnlPageScripts";
            this.pnlPageScripts.Visible = false;
            //
            // tlpScripts
            //
            this.tlpScripts.AutoSize = true;
            this.tlpScripts.ColumnCount = 2;
            this.tlpScripts.Controls.Add(this.lblScriptTimeout, 0, 0);
            this.tlpScripts.Controls.Add(this.numScriptTimeout, 1, 0);
            this.tlpScripts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpScripts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScripts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpScripts.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpScripts.Name = "tlpScripts";
            this.tlpScripts.RowCount = 1;
            //
            // lblScriptTimeout
            //
            this.lblScriptTimeout.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblScriptTimeout.AutoSize = true;
            this.lblScriptTimeout.Name = "lblScriptTimeout";
            this.lblScriptTimeout.Text = "Limit czasu skryptu (s):";
            //
            // numScriptTimeout
            //
            this.numScriptTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numScriptTimeout.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            this.numScriptTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numScriptTimeout.Name = "numScriptTimeout";
            this.numScriptTimeout.Value = new decimal(new int[] { 5, 0, 0, 0 });
            //
            // lblSectionScripts
            //
            this.lblSectionScripts.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionScripts.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionScripts.Name = "lblSectionScripts";
            this.lblSectionScripts.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionScripts.Size = new System.Drawing.Size(434, 24);
            this.lblSectionScripts.Text = "Wykonywanie";
            //
            // pnlPageExport
            //
            this.pnlPageExport.Controls.Add(this.cbxCsvDelimiter);
            this.pnlPageExport.Controls.Add(this.lblSectionCsv);
            this.pnlPageExport.Controls.Add(this.cbxHtmlTemplate);
            this.pnlPageExport.Controls.Add(this.lblSectionHtml);
            this.pnlPageExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageExport.Name = "pnlPageExport";
            this.pnlPageExport.Visible = false;
            //
            // cbxCsvDelimiter
            //
            this.cbxCsvDelimiter.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxCsvDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCsvDelimiter.Name = "cbxCsvDelimiter";
            //
            // lblSectionCsv
            //
            this.lblSectionCsv.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionCsv.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionCsv.Name = "lblSectionCsv";
            this.lblSectionCsv.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionCsv.Size = new System.Drawing.Size(434, 24);
            this.lblSectionCsv.Text = "Plik CSV";
            //
            // cbxHtmlTemplate
            //
            this.cbxHtmlTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxHtmlTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxHtmlTemplate.Name = "cbxHtmlTemplate";
            //
            // lblSectionHtml
            //
            this.lblSectionHtml.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionHtml.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionHtml.Name = "lblSectionHtml";
            this.lblSectionHtml.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionHtml.Size = new System.Drawing.Size(434, 24);
            this.lblSectionHtml.Text = "Szablon HTML";
            //
            // pnlPageFolders
            //
            this.pnlPageFolders.Controls.Add(this.txtScriptsFolder);
            this.pnlPageFolders.Controls.Add(this.lblScriptsFolder);
            this.pnlPageFolders.Controls.Add(this.txtDataFolder);
            this.pnlPageFolders.Controls.Add(this.lblDataFolder);
            this.pnlPageFolders.Controls.Add(this.lblSectionLocations);
            this.pnlPageFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPageFolders.Name = "pnlPageFolders";
            this.pnlPageFolders.Visible = false;
            //
            // txtScriptsFolder
            //
            this.txtScriptsFolder.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtScriptsFolder.Name = "txtScriptsFolder";
            this.txtScriptsFolder.ReadOnly = true;
            //
            // lblScriptsFolder
            //
            this.lblScriptsFolder.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblScriptsFolder.Name = "lblScriptsFolder";
            this.lblScriptsFolder.Size = new System.Drawing.Size(434, 20);
            this.lblScriptsFolder.Text = "Katalog skryptów:";
            //
            // txtDataFolder
            //
            this.txtDataFolder.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtDataFolder.Name = "txtDataFolder";
            this.txtDataFolder.ReadOnly = true;
            //
            // lblDataFolder
            //
            this.lblDataFolder.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDataFolder.Name = "lblDataFolder";
            this.lblDataFolder.Size = new System.Drawing.Size(434, 20);
            this.lblDataFolder.Text = "Katalog danych:";
            //
            // lblSectionLocations
            //
            this.lblSectionLocations.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionLocations.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSectionLocations.Name = "lblSectionLocations";
            this.lblSectionLocations.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblSectionLocations.Size = new System.Drawing.Size(434, 24);
            this.lblSectionLocations.Text = "Lokalizacje";
            //
            // tvPages
            //
            this.tvPages.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvPages.HideSelection = false;
            this.tvPages.Name = "tvPages";
            this.tvPages.Size = new System.Drawing.Size(190, 387);
            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnHelp);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(8);
            this.pnlButtons.Size = new System.Drawing.Size(434, 44);
            //
            // btnHelp
            //
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(80, 23);
            this.btnHelp.Text = "Pomoc";
            this.btnHelp.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 23);
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 23);
            this.btnSave.Text = "Zapisz";
            this.btnSave.UseVisualStyleBackColor = true;
            //
            // localizer
            //
            this.localizer.ContainerControl = this;
            //
            // PrefsForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(644, 431);
            this.Controls.Add(this.pnlPages);
            this.Controls.Add(this.tvPages);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrefsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preferencje";
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCoverWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScriptTimeout)).EndInit();
            this.pnlPages.ResumeLayout(false);
            this.pnlPages.PerformLayout();
            this.pnlPageGeneral.ResumeLayout(false);
            this.pnlPageGeneral.PerformLayout();
            this.pnlPageIcons.ResumeLayout(false);
            this.pnlPageIcons.PerformLayout();
            this.tlpIcons.ResumeLayout(false);
            this.tlpIcons.PerformLayout();
            this.pnlPageFiles.ResumeLayout(false);
            this.pnlPageFiles.PerformLayout();
            this.pnlPageList.ResumeLayout(false);
            this.pnlPageList.PerformLayout();
            this.pnlPageTitleFormat.ResumeLayout(false);
            this.pnlPageTitleFormat.PerformLayout();
            this.pnlPageGrouping.ResumeLayout(false);
            this.pnlPageGrouping.PerformLayout();
            this.pnlPageExtras.ResumeLayout(false);
            this.pnlPageExtras.PerformLayout();
            this.pnlPageGame.ResumeLayout(false);
            this.pnlPageGame.PerformLayout();
            this.pnlPageLists.ResumeLayout(false);
            this.pnlPageLists.PerformLayout();
            this.pnlListEdit.ResumeLayout(false);
            this.pnlListEdit.PerformLayout();
            this.pnlPageImportMedia.ResumeLayout(false);
            this.pnlPageImportMedia.PerformLayout();
            this.pnlPageImportPictures.ResumeLayout(false);
            this.pnlPageImportPictures.PerformLayout();
            this.tlpCover.ResumeLayout(false);
            this.tlpCover.PerformLayout();
            this.pnlPageSearch.ResumeLayout(false);
            this.pnlPageSearch.PerformLayout();
            this.tlpSite.ResumeLayout(false);
            this.tlpSite.PerformLayout();
            this.pnlSiteButtons.ResumeLayout(false);
            this.pnlSiteButtons.PerformLayout();
            this.pnlPageScripts.ResumeLayout(false);
            this.pnlPageScripts.PerformLayout();
            this.tlpScripts.ResumeLayout(false);
            this.tlpScripts.PerformLayout();
            this.pnlPageExport.ResumeLayout(false);
            this.pnlPageExport.PerformLayout();
            this.pnlPageFolders.ResumeLayout(false);
            this.pnlPageFolders.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlPages;
        private System.Windows.Forms.Panel pnlPageGeneral;
        private System.Windows.Forms.CheckBox chkForceFullRefresh;
        private System.Windows.Forms.CheckBox chkNaturalSort;
        private System.Windows.Forms.Label lblSectionGeneral;
        private System.Windows.Forms.Label lblAutoNumberingNote;
        private System.Windows.Forms.CheckBox chkAutoNumbering;
        private System.Windows.Forms.Label lblSectionNumbering;
        private System.Windows.Forms.Panel pnlPageIcons;
        private System.Windows.Forms.CheckBox chkMenuIcons;
        private System.Windows.Forms.TableLayoutPanel tlpIcons;
        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.ComboBox cbxTheme;
        private System.Windows.Forms.Label lblColorScheme;
        private System.Windows.Forms.ComboBox cbxColorScheme;
        private System.Windows.Forms.Label lblSectionToolbar;
        private System.Windows.Forms.Panel pnlPageFiles;
        private System.Windows.Forms.CheckBox chkBackupOnSave;
        private System.Windows.Forms.CheckBox chkReopenLastFile;
        private System.Windows.Forms.Label lblSectionCatalog;
        private System.Windows.Forms.Panel pnlPageList;
        private System.Windows.Forms.CheckBox chkShowGridLines;
        private System.Windows.Forms.CheckBox chkFullRowSelect;
        private System.Windows.Forms.Label lblSectionListLook;
        private System.Windows.Forms.Panel pnlPageTitleFormat;
        private System.Windows.Forms.ComboBox cbxTitleFormat;
        private System.Windows.Forms.Label lblSectionTitleFormat;
        private System.Windows.Forms.Panel pnlPageGrouping;
        private System.Windows.Forms.CheckBox chkShowGroupCount;
        private System.Windows.Forms.Panel pnlPageExtras;
        private System.Windows.Forms.CheckBox chkShowExtraCategory;
        private System.Windows.Forms.Panel pnlPageGame;
        private System.Windows.Forms.CheckBox chkCoverVisible;
        private System.Windows.Forms.Panel pnlPageLists;
        private System.Windows.Forms.ListBox lstListValues;
        private System.Windows.Forms.Panel pnlListEdit;
        private System.Windows.Forms.TextBox txtNewValue;
        private System.Windows.Forms.Button btnListDelete;
        private System.Windows.Forms.Button btnListAdd;
        private System.Windows.Forms.ComboBox cbxListKeys;
        private System.Windows.Forms.Label lblSectionLists;
        private System.Windows.Forms.Panel pnlPageImportMedia;
        private System.Windows.Forms.Label lblPatternsHint;
        private System.Windows.Forms.TextBox txtMediaPatterns;
        private System.Windows.Forms.Label lblSectionPatterns;
        private System.Windows.Forms.Panel pnlPageImportPictures;
        private System.Windows.Forms.TableLayoutPanel tlpCover;
        private System.Windows.Forms.Label lblMaxCoverWidth;
        private System.Windows.Forms.NumericUpDown numMaxCoverWidth;
        private System.Windows.Forms.Label lblSectionCovers;
        private System.Windows.Forms.Panel pnlPageSearch;
        private System.Windows.Forms.ListBox lstSites;
        private System.Windows.Forms.TableLayoutPanel tlpSite;
        private System.Windows.Forms.Label lblSiteName;
        private System.Windows.Forms.TextBox txtSiteName;
        private System.Windows.Forms.Label lblSiteUrl;
        private System.Windows.Forms.TextBox txtSiteUrl;
        private System.Windows.Forms.Panel pnlSiteButtons;
        private System.Windows.Forms.Button btnSiteDefaults;
        private System.Windows.Forms.Button btnSiteDelete;
        private System.Windows.Forms.Button btnSiteAdd;
        private System.Windows.Forms.Label lblSectionSearch;
        private System.Windows.Forms.Panel pnlPageScripts;
        private System.Windows.Forms.TableLayoutPanel tlpScripts;
        private System.Windows.Forms.Label lblScriptTimeout;
        private System.Windows.Forms.NumericUpDown numScriptTimeout;
        private System.Windows.Forms.Label lblSectionScripts;
        private System.Windows.Forms.Panel pnlPageExport;
        private System.Windows.Forms.ComboBox cbxCsvDelimiter;
        private System.Windows.Forms.Label lblSectionCsv;
        private System.Windows.Forms.ComboBox cbxHtmlTemplate;
        private System.Windows.Forms.Label lblSectionHtml;
        private System.Windows.Forms.Panel pnlPageFolders;
        private System.Windows.Forms.TextBox txtScriptsFolder;
        private System.Windows.Forms.Label lblScriptsFolder;
        private System.Windows.Forms.TextBox txtDataFolder;
        private System.Windows.Forms.Label lblDataFolder;
        private System.Windows.Forms.Label lblSectionLocations;
        private System.Windows.Forms.TreeView tvPages;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private GameCatalog.UI.Components.Localizer localizer;
    }
}

namespace SAM.Picker
{
    partial class GamePicker
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
            this._CallbackTimer = new System.Windows.Forms.Timer(this.components);
            this._HeaderPanel = new System.Windows.Forms.Panel();
            this._LibraryLabel = new System.Windows.Forms.Label();
            this._LibraryCountLabel = new System.Windows.Forms.Label();
            this._AddGameTextBox = new SAM.UI.ThemedField();
            this._AddGameButton = new SAM.UI.ThemedButton();
            this._RefreshGamesButton = new SAM.UI.ThemedButton();
            this._SearchGameTextBox = new SAM.UI.ThemedField();
            this._FilterGamesChip = new SAM.UI.ThemedChip();
            this._FilterDemosChip = new SAM.UI.ThemedChip();
            this._FilterModsChip = new SAM.UI.ThemedChip();
            this._FilterJunkChip = new SAM.UI.ThemedChip();
            this._GameGrid = new SAM.Picker.GameGrid();
            this._FooterPanel = new System.Windows.Forms.Panel();
            this._PickerStatusLabel = new System.Windows.Forms.Label();
            this._DownloadStatusLabel = new System.Windows.Forms.Label();
            this._LogoWorker = new System.ComponentModel.BackgroundWorker();
            this._ListWorker = new System.ComponentModel.BackgroundWorker();
            this._HeaderPanel.SuspendLayout();
            this._FooterPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _CallbackTimer
            //
            this._CallbackTimer.Enabled = true;
            this._CallbackTimer.Tick += new System.EventHandler(this.OnTimer);
            //
            // _HeaderPanel
            //
            this._HeaderPanel.BackColor = SAM.UI.Theme.Surface;
            this._HeaderPanel.Controls.Add(this._LibraryLabel);
            this._HeaderPanel.Controls.Add(this._LibraryCountLabel);
            this._HeaderPanel.Controls.Add(this._SearchGameTextBox);
            this._HeaderPanel.Controls.Add(this._FilterGamesChip);
            this._HeaderPanel.Controls.Add(this._FilterDemosChip);
            this._HeaderPanel.Controls.Add(this._FilterModsChip);
            this._HeaderPanel.Controls.Add(this._FilterJunkChip);
            this._HeaderPanel.Controls.Add(this._AddGameTextBox);
            this._HeaderPanel.Controls.Add(this._AddGameButton);
            this._HeaderPanel.Controls.Add(this._RefreshGamesButton);
            this._HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._HeaderPanel.Name = "_HeaderPanel";
            this._HeaderPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnHeaderLayout);
            //
            // _LibraryLabel
            //
            this._LibraryLabel.AutoSize = true;
            this._LibraryLabel.Font = SAM.UI.Theme.Title;
            this._LibraryLabel.ForeColor = SAM.UI.Theme.Text;
            this._LibraryLabel.Name = "_LibraryLabel";
            this._LibraryLabel.Text = "Your library";
            //
            // _LibraryCountLabel
            //
            this._LibraryCountLabel.AutoSize = true;
            this._LibraryCountLabel.Font = SAM.UI.Theme.Body;
            this._LibraryCountLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._LibraryCountLabel.Name = "_LibraryCountLabel";
            //
            // _AddGameTextBox
            //
            this._AddGameTextBox.Cue = "App ID";
            this._AddGameTextBox.Name = "_AddGameTextBox";
            this._AddGameTextBox.NumbersOnly = true;
            this._AddGameTextBox.Submitted += new System.EventHandler(this.OnAddGame);
            //
            // _AddGameButton
            //
            this._AddGameButton.Glyph = SAM.UI.Glyphs.Add;
            this._AddGameButton.Name = "_AddGameButton";
            this._AddGameButton.Text = "Add game";
            this._AddGameButton.Click += new System.EventHandler(this.OnAddGame);
            //
            // _RefreshGamesButton
            //
            this._RefreshGamesButton.AccessibleName = "Refresh games";
            this._RefreshGamesButton.Glyph = SAM.UI.Glyphs.Refresh;
            this._RefreshGamesButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._RefreshGamesButton.Name = "_RefreshGamesButton";
            this._RefreshGamesButton.Click += new System.EventHandler(this.OnRefresh);
            //
            // _SearchGameTextBox
            //
            this._SearchGameTextBox.Cue = "Search your games";
            this._SearchGameTextBox.Glyph = SAM.UI.Glyphs.Search;
            this._SearchGameTextBox.Name = "_SearchGameTextBox";
            this._SearchGameTextBox.TextChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _FilterGamesChip
            //
            this._FilterGamesChip.Checked = true;
            this._FilterGamesChip.Name = "_FilterGamesChip";
            this._FilterGamesChip.Text = "Games";
            this._FilterGamesChip.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _FilterDemosChip
            //
            this._FilterDemosChip.Name = "_FilterDemosChip";
            this._FilterDemosChip.Text = "Demos";
            this._FilterDemosChip.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _FilterModsChip
            //
            this._FilterModsChip.Name = "_FilterModsChip";
            this._FilterModsChip.Text = "Mods";
            this._FilterModsChip.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _FilterJunkChip
            //
            this._FilterJunkChip.Name = "_FilterJunkChip";
            this._FilterJunkChip.Text = "Junk";
            this._FilterJunkChip.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _GameGrid
            //
            this._GameGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._GameGrid.Name = "_GameGrid";
            this._GameGrid.TabIndex = 0;
            this._GameGrid.ItemActivate += new System.EventHandler(this.OnActivateGame);
            this._GameGrid.LogoNeeded += new System.Action<SAM.Picker.GameInfo>(this.OnGameLogoNeeded);
            //
            // _FooterPanel
            //
            this._FooterPanel.BackColor = SAM.UI.Theme.Surface;
            this._FooterPanel.Controls.Add(this._PickerStatusLabel);
            this._FooterPanel.Controls.Add(this._DownloadStatusLabel);
            this._FooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._FooterPanel.Name = "_FooterPanel";
            this._FooterPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnFooterLayout);
            //
            // _PickerStatusLabel
            //
            this._PickerStatusLabel.AutoEllipsis = true;
            this._PickerStatusLabel.Font = SAM.UI.Theme.Small;
            this._PickerStatusLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._PickerStatusLabel.Name = "_PickerStatusLabel";
            this._PickerStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _DownloadStatusLabel
            //
            this._DownloadStatusLabel.Font = SAM.UI.Theme.Small;
            this._DownloadStatusLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._DownloadStatusLabel.Name = "_DownloadStatusLabel";
            this._DownloadStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._DownloadStatusLabel.Visible = false;
            //
            // _LogoWorker
            //
            this._LogoWorker.WorkerSupportsCancellation = true;
            this._LogoWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadLogo);
            this._LogoWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadLogo);
            //
            // _ListWorker
            //
            this._ListWorker.WorkerReportsProgress = true;
            this._ListWorker.WorkerSupportsCancellation = true;
            this._ListWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadList);
            this._ListWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.OnDownloadListProgress);
            this._ListWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadList);
            //
            // GamePicker
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._GameGrid);
            this.Controls.Add(this._FooterPanel);
            this.Controls.Add(this._HeaderPanel);
            this.Name = "GamePicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SAM Modern";
            this._HeaderPanel.ResumeLayout(false);
            this._HeaderPanel.PerformLayout();
            this._FooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer _CallbackTimer;
        private System.Windows.Forms.Panel _HeaderPanel;
        private System.Windows.Forms.Label _LibraryLabel;
        private System.Windows.Forms.Label _LibraryCountLabel;
        private SAM.UI.ThemedField _AddGameTextBox;
        private SAM.UI.ThemedButton _AddGameButton;
        private SAM.UI.ThemedButton _RefreshGamesButton;
        private SAM.UI.ThemedField _SearchGameTextBox;
        private SAM.UI.ThemedChip _FilterGamesChip;
        private SAM.UI.ThemedChip _FilterDemosChip;
        private SAM.UI.ThemedChip _FilterModsChip;
        private SAM.UI.ThemedChip _FilterJunkChip;
        private GameGrid _GameGrid;
        private System.Windows.Forms.Panel _FooterPanel;
        private System.Windows.Forms.Label _PickerStatusLabel;
        private System.Windows.Forms.Label _DownloadStatusLabel;
        private System.ComponentModel.BackgroundWorker _LogoWorker;
        private System.ComponentModel.BackgroundWorker _ListWorker;
    }
}

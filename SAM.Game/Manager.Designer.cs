namespace SAM.Game
{
    partial class Manager
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
            this._GameArt = new SAM.UI.RoundedPicture();
            this._GameNameLabel = new System.Windows.Forms.Label();
            this._ProgressLabel = new System.Windows.Forms.Label();
            this._ProgressBar = new SAM.UI.ThemedProgress();
            this._PendingLabel = new System.Windows.Forms.Label();
            this._StoreButton = new SAM.UI.ThemedButton();
            this._ReloadButton = new SAM.UI.ThemedButton();
            this._ResetButton = new SAM.UI.ThemedButton();
            this._AchievementsTabChip = new SAM.UI.ThemedChip();
            this._StatisticsTabChip = new SAM.UI.ThemedChip();
            this._AchievementsPanel = new System.Windows.Forms.Panel();
            this._AchievementList = new SAM.Game.AchievementList();
            this._ProtectedNotice = new SAM.UI.ThemedNotice();
            this._AchievementToolbar = new System.Windows.Forms.Panel();
            this._MatchingStringTextBox = new SAM.UI.ThemedField();
            this._ShowLockedChip = new SAM.UI.ThemedChip();
            this._ShowUnlockedChip = new SAM.UI.ThemedChip();
            this._LockAllButton = new SAM.UI.ThemedButton();
            this._InvertAllButton = new SAM.UI.ThemedButton();
            this._UnlockAllButton = new SAM.UI.ThemedButton();
            this._StatisticsPanel = new System.Windows.Forms.Panel();
            this._EnableStatsEditingCheckBox = new SAM.UI.ThemedChip();
            this._StatisticsDataGridView = new System.Windows.Forms.DataGridView();
            this._FooterPanel = new System.Windows.Forms.Panel();
            this._GameStatusLabel = new System.Windows.Forms.Label();
            this._DownloadStatusLabel = new System.Windows.Forms.Label();
            this._HeaderPanel.SuspendLayout();
            this._AchievementsPanel.SuspendLayout();
            this._AchievementToolbar.SuspendLayout();
            this._StatisticsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._StatisticsDataGridView)).BeginInit();
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
            this._HeaderPanel.Controls.Add(this._GameArt);
            this._HeaderPanel.Controls.Add(this._GameNameLabel);
            this._HeaderPanel.Controls.Add(this._ProgressLabel);
            this._HeaderPanel.Controls.Add(this._ProgressBar);
            this._HeaderPanel.Controls.Add(this._PendingLabel);
            this._HeaderPanel.Controls.Add(this._StoreButton);
            this._HeaderPanel.Controls.Add(this._ReloadButton);
            this._HeaderPanel.Controls.Add(this._ResetButton);
            this._HeaderPanel.Controls.Add(this._AchievementsTabChip);
            this._HeaderPanel.Controls.Add(this._StatisticsTabChip);
            this._HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._HeaderPanel.Name = "_HeaderPanel";
            this._HeaderPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnHeaderLayout);
            //
            // _GameArt
            //
            this._GameArt.Name = "_GameArt";
            //
            // _GameNameLabel
            //
            this._GameNameLabel.AutoEllipsis = true;
            this._GameNameLabel.Font = SAM.UI.Theme.Title;
            this._GameNameLabel.ForeColor = SAM.UI.Theme.Text;
            this._GameNameLabel.Name = "_GameNameLabel";
            this._GameNameLabel.UseMnemonic = false;
            //
            // _ProgressLabel
            //
            this._ProgressLabel.AutoSize = true;
            this._ProgressLabel.Font = SAM.UI.Theme.Body;
            this._ProgressLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._ProgressLabel.Name = "_ProgressLabel";
            //
            // _ProgressBar
            //
            this._ProgressBar.Name = "_ProgressBar";
            //
            // _PendingLabel
            //
            this._PendingLabel.AutoSize = true;
            this._PendingLabel.Font = SAM.UI.Theme.BodyStrong;
            this._PendingLabel.ForeColor = SAM.UI.Theme.AccentHover;
            this._PendingLabel.Name = "_PendingLabel";
            //
            // _StoreButton
            //
            this._StoreButton.Enabled = false;
            this._StoreButton.Glyph = SAM.UI.Glyphs.Check;
            this._StoreButton.Name = "_StoreButton";
            this._StoreButton.Text = "Save changes";
            this._StoreButton.Click += new System.EventHandler(this.OnStore);
            //
            // _ReloadButton
            //
            this._ReloadButton.Enabled = false;
            this._ReloadButton.Glyph = SAM.UI.Glyphs.Refresh;
            this._ReloadButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._ReloadButton.Name = "_ReloadButton";
            this._ReloadButton.Text = "Refresh";
            this._ReloadButton.Click += new System.EventHandler(this.OnRefresh);
            //
            // _ResetButton
            //
            this._ResetButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._ResetButton.Name = "_ResetButton";
            this._ResetButton.Text = "Reset";
            this._ResetButton.Click += new System.EventHandler(this.OnResetAllStats);
            //
            // _AchievementsTabChip
            //
            this._AchievementsTabChip.AutoCheck = false;
            this._AchievementsTabChip.Checked = true;
            this._AchievementsTabChip.Name = "_AchievementsTabChip";
            this._AchievementsTabChip.Text = "Achievements";
            this._AchievementsTabChip.Click += new System.EventHandler(this.OnShowAchievements);
            //
            // _StatisticsTabChip
            //
            this._StatisticsTabChip.AutoCheck = false;
            this._StatisticsTabChip.Name = "_StatisticsTabChip";
            this._StatisticsTabChip.Text = "Statistics";
            this._StatisticsTabChip.Click += new System.EventHandler(this.OnShowStatistics);
            //
            // _AchievementsPanel
            //
            this._AchievementsPanel.Controls.Add(this._AchievementList);
            this._AchievementsPanel.Controls.Add(this._ProtectedNotice);
            this._AchievementsPanel.Controls.Add(this._AchievementToolbar);
            this._AchievementsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._AchievementsPanel.Name = "_AchievementsPanel";
            //
            // _AchievementList
            //
            this._AchievementList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._AchievementList.Name = "_AchievementList";
            this._AchievementList.ItemToggled += new System.Action<SAM.Game.Stats.AchievementInfo>(this.OnAchievementToggled);
            this._AchievementList.ProtectedToggled += new System.Action<SAM.Game.Stats.AchievementInfo>(this.OnProtectedToggled);
            //
            // _ProtectedNotice
            //
            this._ProtectedNotice.Dock = System.Windows.Forms.DockStyle.Top;
            this._ProtectedNotice.Name = "_ProtectedNotice";
            this._ProtectedNotice.Visible = false;
            //
            // _AchievementToolbar
            //
            this._AchievementToolbar.Controls.Add(this._MatchingStringTextBox);
            this._AchievementToolbar.Controls.Add(this._ShowLockedChip);
            this._AchievementToolbar.Controls.Add(this._ShowUnlockedChip);
            this._AchievementToolbar.Controls.Add(this._LockAllButton);
            this._AchievementToolbar.Controls.Add(this._InvertAllButton);
            this._AchievementToolbar.Controls.Add(this._UnlockAllButton);
            this._AchievementToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this._AchievementToolbar.Name = "_AchievementToolbar";
            this._AchievementToolbar.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnToolbarLayout);
            //
            // _MatchingStringTextBox
            //
            this._MatchingStringTextBox.Cue = "Search achievements";
            this._MatchingStringTextBox.Glyph = SAM.UI.Glyphs.Search;
            this._MatchingStringTextBox.Name = "_MatchingStringTextBox";
            this._MatchingStringTextBox.TextChanged += new System.EventHandler(this.OnFilterUpdate);
            //
            // _ShowLockedChip
            //
            this._ShowLockedChip.Name = "_ShowLockedChip";
            this._ShowLockedChip.Text = "Locked";
            this._ShowLockedChip.Click += new System.EventHandler(this.OnShowLockedOnly);
            //
            // _ShowUnlockedChip
            //
            this._ShowUnlockedChip.Name = "_ShowUnlockedChip";
            this._ShowUnlockedChip.Text = "Unlocked";
            this._ShowUnlockedChip.Click += new System.EventHandler(this.OnShowUnlockedOnly);
            //
            // _LockAllButton
            //
            this._LockAllButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._LockAllButton.Name = "_LockAllButton";
            this._LockAllButton.Text = "Lock all";
            this._LockAllButton.Click += new System.EventHandler(this.OnLockAll);
            //
            // _InvertAllButton
            //
            this._InvertAllButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._InvertAllButton.Name = "_InvertAllButton";
            this._InvertAllButton.Text = "Invert";
            this._InvertAllButton.Click += new System.EventHandler(this.OnInvertAll);
            //
            // _UnlockAllButton
            //
            this._UnlockAllButton.Kind = SAM.UI.ThemedButtonKind.Ghost;
            this._UnlockAllButton.Name = "_UnlockAllButton";
            this._UnlockAllButton.Text = "Unlock all";
            this._UnlockAllButton.Click += new System.EventHandler(this.OnUnlockAll);
            //
            // _StatisticsPanel
            //
            this._StatisticsPanel.Controls.Add(this._StatisticsDataGridView);
            this._StatisticsPanel.Controls.Add(this._EnableStatsEditingCheckBox);
            this._StatisticsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._StatisticsPanel.Name = "_StatisticsPanel";
            this._StatisticsPanel.Visible = false;
            this._StatisticsPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnStatisticsLayout);
            //
            // _EnableStatsEditingCheckBox
            //
            this._EnableStatsEditingCheckBox.Name = "_EnableStatsEditingCheckBox";
            this._EnableStatsEditingCheckBox.Text = "I understand by modifying the values of stats, I may screw things up and can\'t blame anyone but myself.";
            this._EnableStatsEditingCheckBox.CheckedChanged += new System.EventHandler(this.OnStatAgreementChecked);
            //
            // _StatisticsDataGridView
            //
            this._StatisticsDataGridView.AllowUserToAddRows = false;
            this._StatisticsDataGridView.AllowUserToDeleteRows = false;
            this._StatisticsDataGridView.AllowUserToResizeRows = false;
            this._StatisticsDataGridView.BackgroundColor = SAM.UI.Theme.Background;
            this._StatisticsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._StatisticsDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this._StatisticsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this._StatisticsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._StatisticsDataGridView.EnableHeadersVisualStyles = false;
            this._StatisticsDataGridView.GridColor = SAM.UI.Theme.SurfaceRaised;
            this._StatisticsDataGridView.Name = "_StatisticsDataGridView";
            this._StatisticsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this._StatisticsDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnStatCellEndEdit);
            this._StatisticsDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.OnStatDataError);
            //
            // _FooterPanel
            //
            this._FooterPanel.BackColor = SAM.UI.Theme.Surface;
            this._FooterPanel.Controls.Add(this._GameStatusLabel);
            this._FooterPanel.Controls.Add(this._DownloadStatusLabel);
            this._FooterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._FooterPanel.Name = "_FooterPanel";
            this._FooterPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnFooterLayout);
            //
            // _GameStatusLabel
            //
            this._GameStatusLabel.AutoEllipsis = true;
            this._GameStatusLabel.Font = SAM.UI.Theme.Small;
            this._GameStatusLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._GameStatusLabel.Name = "_GameStatusLabel";
            this._GameStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._GameStatusLabel.UseMnemonic = false;
            //
            // _DownloadStatusLabel
            //
            this._DownloadStatusLabel.Font = SAM.UI.Theme.Small;
            this._DownloadStatusLabel.ForeColor = SAM.UI.Theme.TextMuted;
            this._DownloadStatusLabel.Name = "_DownloadStatusLabel";
            this._DownloadStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._DownloadStatusLabel.Visible = false;
            //
            // Manager
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._AchievementsPanel);
            this.Controls.Add(this._StatisticsPanel);
            this.Controls.Add(this._FooterPanel);
            this.Controls.Add(this._HeaderPanel);
            this.Name = "Manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SAM Modern";
            this._HeaderPanel.ResumeLayout(false);
            this._HeaderPanel.PerformLayout();
            this._AchievementsPanel.ResumeLayout(false);
            this._AchievementToolbar.ResumeLayout(false);
            this._StatisticsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._StatisticsDataGridView)).EndInit();
            this._FooterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer _CallbackTimer;
        private System.Windows.Forms.Panel _HeaderPanel;
        private SAM.UI.RoundedPicture _GameArt;
        private System.Windows.Forms.Label _GameNameLabel;
        private System.Windows.Forms.Label _ProgressLabel;
        private SAM.UI.ThemedProgress _ProgressBar;
        private System.Windows.Forms.Label _PendingLabel;
        private SAM.UI.ThemedButton _StoreButton;
        private SAM.UI.ThemedButton _ReloadButton;
        private SAM.UI.ThemedButton _ResetButton;
        private SAM.UI.ThemedChip _AchievementsTabChip;
        private SAM.UI.ThemedChip _StatisticsTabChip;
        private System.Windows.Forms.Panel _AchievementsPanel;
        private AchievementList _AchievementList;
        private SAM.UI.ThemedNotice _ProtectedNotice;
        private System.Windows.Forms.Panel _AchievementToolbar;
        private SAM.UI.ThemedField _MatchingStringTextBox;
        private SAM.UI.ThemedChip _ShowLockedChip;
        private SAM.UI.ThemedChip _ShowUnlockedChip;
        private SAM.UI.ThemedButton _LockAllButton;
        private SAM.UI.ThemedButton _InvertAllButton;
        private SAM.UI.ThemedButton _UnlockAllButton;
        private System.Windows.Forms.Panel _StatisticsPanel;
        private SAM.UI.ThemedChip _EnableStatsEditingCheckBox;
        private System.Windows.Forms.DataGridView _StatisticsDataGridView;
        private System.Windows.Forms.Panel _FooterPanel;
        private System.Windows.Forms.Label _GameStatusLabel;
        private System.Windows.Forms.Label _DownloadStatusLabel;
    }
}

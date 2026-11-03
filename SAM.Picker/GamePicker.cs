/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 * Modified in 2026 by Naman for SAM Modern
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using SAM.UI;
using static SAM.Picker.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Picker
{
    internal partial class GamePicker : Form
    {
        private readonly API.Client _SteamClient;

        private readonly Dictionary<uint, GameInfo> _Games;
        private readonly List<GameInfo> _FilteredGames;

        private readonly object _LogoLock;
        private readonly HashSet<string> _LogosAttempting;
        private readonly HashSet<string> _LogosAttempted;
        private readonly ConcurrentQueue<GameInfo> _LogoQueue;
        private readonly Dictionary<string, Bitmap> _Logos;

        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;

        private string _GameListNotice;
        private bool _CloseAfterList;

        public GamePicker(API.Client client)
        {
            this._Games = new();
            this._FilteredGames = new();
            this._LogoLock = new();
            this._LogosAttempting = new();
            this._LogosAttempted = new();
            this._LogoQueue = new();
            this._Logos = new();
            this._GameListNotice = "";

            this.InitializeComponent();

            Theme.Apply(this);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this._HeaderPanel.Height = this.Scaled(112);
            this._FooterPanel.Height = this.Scaled(30);

            var area = Screen.PrimaryScreen.WorkingArea;
            this.MinimumSize = new(this.Scaled(780), this.Scaled(520));
            this.ClientSize = new(
                Math.Min(this.Scaled(1120), area.Width - this.Scaled(80)),
                Math.Min(this.Scaled(760), area.Height - this.Scaled(80)));

            this._SteamClient = client;

            this._AppDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
            this._AppDataChangedCallback.OnRun += this.OnAppDataChanged;

            this.AddGames();
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == false)
            {
                return;
            }

            if (this._Games.TryGetValue(param.Id, out var game) == false)
            {
                return;
            }

            game.Name = this._SteamClient.SteamApps001.GetAppData(game.Id, "name");

            this.AddGameToLogoQueue(game);
            this.DownloadNextLogo();
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            this._ListWorker.ReportProgress(0, "Downloading game list...");

            var pairs = this.GetGameList();

            this._ListWorker.ReportProgress(0, "Checking game ownership...");
            foreach (var kv in pairs)
            {
                if (this._ListWorker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }

                this.AddGame(kv.Key, kv.Value);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // The list worker calls Steam through the client, which is released once this window
            // is gone, so the window has to outlive it.
            if (this._ListWorker.IsBusy == true)
            {
                e.Cancel = true;
                this._CloseAfterList = true;
                this._ListWorker.CancelAsync();
                this.Hide();
            }
        }

        private static string GameListPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SAM",
            "games.xml");

        private List<KeyValuePair<uint, string>> GetGameList()
        {
            this._GameListNotice = "";

            byte[] bytes;
            List<KeyValuePair<uint, string>> pairs;
            try
            {
                using (WebClient downloader = new())
                {
                    bytes = downloader.DownloadData(new Uri("https://gib.me/sam/games.xml"));
                }

                pairs = ParseGameList(bytes);
            }
            catch (Exception e) when (e is WebException or XmlException or FormatException)
            {
                var savedPairs = LoadSavedGameList();
                if (savedPairs == null)
                {
                    throw;
                }

                var savedTime = File.GetLastWriteTime(GameListPath);
                this._GameListNotice = $" The server could not be reached, using the list saved on {savedTime:d}.";
                return savedPairs;
            }

            SaveGameList(bytes);
            return pairs;
        }

        private static List<KeyValuePair<uint, string>> ParseGameList(byte[] bytes)
        {
            List<KeyValuePair<uint, string>> pairs = new();
            using (MemoryStream stream = new(bytes, false))
            {
                XPathDocument document = new(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext() == true)
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (string.IsNullOrEmpty(type) == true)
                    {
                        type = "normal";
                    }
                    pairs.Add(new((uint)nodes.Current.ValueAsLong, type));
                }
            }

            if (pairs.Count == 0)
            {
                throw new XmlException("game list is empty");
            }

            return pairs;
        }

        private static List<KeyValuePair<uint, string>> LoadSavedGameList()
        {
            try
            {
                return ParseGameList(File.ReadAllBytes(GameListPath));
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or XmlException or FormatException)
            {
                return null;
            }
        }

        private static void SaveGameList(byte[] bytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GameListPath));
                File.WriteAllBytes(GameListPath, bytes);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                // Without a saved copy the picker only loses its fallback.
            }
        }

        private void OnDownloadListProgress(object sender, ProgressChangedEventArgs e)
        {
            this._PickerStatusLabel.Text = (string)e.UserState;
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this._CloseAfterList == true)
            {
                this.Close();
                return;
            }

            if (e.Error != null || e.Cancelled == true)
            {
                this.AddDefaultGames();
                ThemedDialog.Show(
                    "The game list could not be downloaded, and there is no saved copy to use instead.\n\n" +
                    "You can still add a game by typing its App ID next to Add Game.\n\n" +
                    "(" + (e.Error?.Message ?? "download cancelled") + ")",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            this._GameGrid.IsLoading = false;
            this.RefreshGames();
            this._GameGrid.Select();
            this._RefreshGamesButton.Enabled = true;
            this._AddGameButton.Enabled = true;
            this.DownloadNextLogo();
        }

        private void RefreshGames()
        {
            var nameSearch = this._SearchGameTextBox.Text.Length > 0
                ? this._SearchGameTextBox.Text
                : null;

            var wantNormals = this._FilterGamesChip.Checked == true;
            var wantDemos = this._FilterDemosChip.Checked == true;
            var wantMods = this._FilterModsChip.Checked == true;
            var wantJunk = this._FilterJunkChip.Checked == true;

            this._FilteredGames.Clear();
            foreach (var info in this._Games.Values.OrderBy(gi => gi.Name))
            {
                if (nameSearch != null &&
                    info.Name.IndexOf(nameSearch, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                bool wanted = info.Type switch
                {
                    "normal" => wantNormals,
                    "demo" => wantDemos,
                    "mod" => wantMods,
                    "junk" => wantJunk,
                    _ => true,
                };
                if (wanted == false)
                {
                    continue;
                }

                this._FilteredGames.Add(info);
            }

            this._GameGrid.EmptyTitle = nameSearch != null ? $"No games match \"{nameSearch}\"" : "No games to show";
            this._GameGrid.EmptyHint = "Check the type filters, or add a game by its App ID.";
            this._GameGrid.SetItems(this._FilteredGames);

            this._LibraryCountLabel.Text = Counted(this._Games.Count, "game");
            this._PickerStatusLabel.Text =
                $"Displaying {Counted(this._FilteredGames.Count, "game")}. Total {Counted(this._Games.Count, "game")}.{this._GameListNotice}";
        }

        private static string Counted(int count, string noun)
        {
            return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
        }

        private void DoDownloadLogo(object sender, DoWorkEventArgs e)
        {
            var info = (GameInfo)e.Argument;

            this._LogosAttempted.Add(info.ImageUrl);

            using (WebClient downloader = new())
            {
                try
                {
                    var data = downloader.DownloadData(new Uri(info.ImageUrl));
                    using (MemoryStream stream = new(data, false))
                    using (Bitmap decoded = new(stream))
                    {
                        e.Result = new LogoInfo(info.Id, new(decoded));
                    }
                }
                catch (Exception)
                {
                    e.Result = new LogoInfo(info.Id, null);
                }
            }
        }

        private void OnDownloadLogo(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled == true)
            {
                return;
            }

            if (e.Result is LogoInfo logoInfo &&
                logoInfo.Bitmap != null &&
                this._Games.TryGetValue(logoInfo.Id, out var gameInfo) == true)
            {
                this._Logos[gameInfo.ImageUrl] = logoInfo.Bitmap;
                gameInfo.Logo = logoInfo.Bitmap;
                this._GameGrid.Invalidate();
            }

            this.DownloadNextLogo();
        }

        private void DownloadNextLogo()
        {
            lock (this._LogoLock)
            {

                if (this._LogoWorker.IsBusy == true)
                {
                    return;
                }

                GameInfo info;
                while (true)
                {
                    if (this._LogoQueue.TryDequeue(out info) == false)
                    {
                        this._DownloadStatusLabel.Visible = false;
                        return;
                    }

                    if (this._GameGrid.IsItemVisible(info) == false)
                    {
                        this._LogosAttempting.Remove(info.ImageUrl);
                        info.LogoQueued = false;
                        continue;
                    }

                    break;
                }

                this._DownloadStatusLabel.Text = $"Downloading {Counted(1 + this._LogoQueue.Count, "game icon")}...";
                this._DownloadStatusLabel.Visible = true;

                this._LogoWorker.RunWorkerAsync(info);
            }
        }

        private void AddGameToLogoQueue(GameInfo info)
        {
            if (info.Logo != null)
            {
                return;
            }

            var imageUrl = this._SteamClient.GetGameImageUrl(info.Id);
            if (string.IsNullOrEmpty(imageUrl) == true)
            {
                return;
            }

            info.ImageUrl = imageUrl;

            if (this._Logos.TryGetValue(imageUrl, out var logo) == true)
            {
                info.Logo = logo;
            }
            else if (
                this._LogosAttempting.Contains(imageUrl) == false &&
                this._LogosAttempted.Contains(imageUrl) == false)
            {
                this._LogosAttempting.Add(imageUrl);
                this._LogoQueue.Enqueue(info);
            }
        }

        private bool OwnsGame(uint id)
        {
            return this._SteamClient.SteamApps008.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
        {
            if (this._Games.ContainsKey(id) == true)
            {
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                return;
            }

            GameInfo info = new(id, type);
            info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");
            this._Games.Add(id, info);
        }

        private void AddGames()
        {
            this._Games.Clear();
            this._FilteredGames.Clear();
            this._GameGrid.SetItems(this._FilteredGames);
            this._GameGrid.IsLoading = true;
            this._RefreshGamesButton.Enabled = false;
            this._AddGameButton.Enabled = false;
            this._ListWorker.RunWorkerAsync();
        }

        private void AddDefaultGames()
        {
            this.AddGame(480, "normal"); // Spacewar
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._CallbackTimer.Enabled = false;
            this._SteamClient.RunCallbacks(false);
            this._CallbackTimer.Enabled = true;
        }

        private void OnActivateGame(object sender, EventArgs e)
        {
            var index = this._GameGrid.SelectedIndex;
            if (index < 0 || index >= this._FilteredGames.Count)
            {
                return;
            }

            var info = this._FilteredGames[index];
            if (info == null)
            {
                return;
            }

            try
            {
                Process.Start(
                    Path.Combine(Application.StartupPath, "SAM.Game.exe"),
                    info.Id.ToString(CultureInfo.InvariantCulture));
            }
            catch (Win32Exception)
            {
                ThemedDialog.Show(
                    this,
                    "Failed to start SAM.Game.exe. Please check that it is in the same folder as\n" +
                    "SAM.Picker.exe, and that your antivirus hasn't removed it.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this._AddGameTextBox.Text = "";
            this.AddGames();
        }

        private void OnAddGame(object sender, EventArgs e)
        {
            if (this._ListWorker.IsBusy == true)
            {
                return;
            }

            uint id;

            if (uint.TryParse(this._AddGameTextBox.Text, out id) == false)
            {
                ThemedDialog.Show(
                    this,
                    "Please enter a valid game ID.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                ThemedDialog.Show(this, "You don't own that game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this._AddGameTextBox.Text = "";
            this._SearchGameTextBox.Text = "";
            this.AddGame(id, "normal");

            // The list may already hold this game under a type that is filtered out.
            this._Games[id].Type = "normal";

            this._FilterGamesChip.Checked = true;
            this.RefreshGames();

            var index = this._FilteredGames.FindIndex(gi => gi.Id == id);
            if (index >= 0)
            {
                this._GameGrid.SelectedIndex = index;
                this._GameGrid.EnsureVisible(index);
                this._GameGrid.Select();
            }

            this.DownloadNextLogo();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            // The list is still being filled on another thread, and finishing applies the filters.
            if (this._ListWorker.IsBusy == true)
            {
                return;
            }

            this.RefreshGames();
        }

        private void OnGameLogoNeeded(GameInfo info)
        {
            if (info.LogoQueued == true)
            {
                return;
            }

            info.LogoQueued = true;
            this.AddGameToLogoQueue(info);
            this.DownloadNextLogo();
        }

        private int Scaled(int value)
        {
            return Theme.Scale(value);
        }

        private void OnHeaderLayout(object sender, LayoutEventArgs e)
        {
            int margin = this.Scaled(20);
            int gap = this.Scaled(8);
            int width = this._HeaderPanel.ClientSize.Width;
            int rowHeight = this.Scaled(34);
            int firstRow = this.Scaled(16);
            int secondRow = firstRow + rowHeight + this.Scaled(12);

            this._LibraryLabel.Location = new(margin - this.Scaled(5), firstRow + ((rowHeight - this._LibraryLabel.Height) / 2));
            this._LibraryCountLabel.Location = new(
                this._LibraryLabel.Right + gap,
                this._LibraryLabel.Bottom - this._LibraryCountLabel.Height - this.Scaled(3));

            int right = width - margin;
            this._RefreshGamesButton.SetBounds(right - rowHeight, firstRow, rowHeight, rowHeight);
            right = this._RefreshGamesButton.Left - gap;

            int addWidth = this._AddGameButton.FitWidth();
            this._AddGameButton.SetBounds(right - addWidth, firstRow, addWidth, rowHeight);
            right = this._AddGameButton.Left - gap;

            int idWidth = this.Scaled(120);
            this._AddGameTextBox.SetBounds(right - idWidth, firstRow, idWidth, rowHeight);

            ThemedChip[] chips = { this._FilterGamesChip, this._FilterDemosChip, this._FilterModsChip, this._FilterJunkChip };
            int chipsWidth = chips.Sum(chip => chip.FitWidth() + gap);
            int searchWidth = Math.Max(this.Scaled(160), Math.Min(this.Scaled(380), width - (margin * 2) - chipsWidth - gap));
            this._SearchGameTextBox.SetBounds(margin, secondRow, searchWidth, rowHeight);

            int left = this._SearchGameTextBox.Right + this.Scaled(12);
            foreach (var chip in chips)
            {
                int chipWidth = chip.FitWidth();
                chip.SetBounds(left, secondRow, chipWidth, rowHeight);
                left += chipWidth + gap;
            }
        }

        private void OnFooterLayout(object sender, LayoutEventArgs e)
        {
            int margin = this.Scaled(20);
            var size = this._FooterPanel.ClientSize;
            int downloadWidth = this.Scaled(220);
            this._DownloadStatusLabel.SetBounds(size.Width - margin - downloadWidth, 0, downloadWidth, size.Height);
            this._PickerStatusLabel.SetBounds(margin, 0, this._DownloadStatusLabel.Left - margin - this.Scaled(8), size.Height);
        }
    }
}

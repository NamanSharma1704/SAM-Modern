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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using SAM.UI;
using static SAM.Game.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Game
{
    internal partial class Manager : Form
    {
        private readonly long _GameId;
        private readonly API.Client _SteamClient;

        private const int IconDownloadSlots = 4;

        private readonly List<WebClient> _IconDownloaders = new();
        private readonly WebClient _ArtDownloader = new();

        private readonly List<Stats.AchievementInfo> _Achievements = new();
        private readonly Dictionary<string, Bitmap> _Icons = new();
        private readonly List<Stats.AchievementInfo> _IconQueue = new();
        private readonly List<Stats.StatDefinition> _StatDefinitions = new();

        private readonly List<Stats.AchievementDefinition> _AchievementDefinitions = new();

        private readonly BindingList<Stats.StatInfo> _Statistics = new();

        private readonly API.Callbacks.UserStatsReceived _UserStatsReceivedCallback;

        private readonly API.Callbacks.UserStatsStored _UserStatsStoredCallback;

        private const int StoreConfirmationSeconds = 15;

        private bool _HasArt;
        private DateTime? _StoreDeadline;
        private string _StoreSummary;
        private bool _StoreHadFailures;

        public Manager(long gameId, API.Client client)
        {
            this.InitializeComponent();

            Theme.Apply(this);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this._HeaderPanel.Height = Theme.Scale(146);
            this._AchievementToolbar.Height = Theme.Scale(62);
            this._FooterPanel.Height = Theme.Scale(30);

            var area = Screen.PrimaryScreen.WorkingArea;
            this.MinimumSize = new(Theme.Scale(760), Theme.Scale(560));
            this.ClientSize = new(
                Math.Min(Theme.Scale(1000), area.Width - Theme.Scale(80)),
                Math.Min(Theme.Scale(780), area.Height - Theme.Scale(80)));

            this._AchievementList.EmptyTitle = "No achievements to show";
            this._AchievementList.EmptyHint = "Steam didn't return any achievements for this game.";

            this.SetupStatisticsGrid();

            this._GameId = gameId;
            this._SteamClient = client;

            for (int i = 0; i < IconDownloadSlots; i++)
            {
                WebClient downloader = new();
                downloader.DownloadDataCompleted += this.OnIconDownload;
                this._IconDownloaders.Add(downloader);
            }

            string name = this._SteamClient.SteamApps001.GetAppData((uint)this._GameId, "name");
            name ??= this._GameId.ToString(CultureInfo.InvariantCulture);
            base.Text += " | " + name;
            this._GameNameLabel.Text = name;

            var artUrl = client.GetGameImageUrl((uint)gameId);
            this.SetHasArt(string.IsNullOrEmpty(artUrl) == false);
            if (this._HasArt == true)
            {
                this._ArtDownloader.DownloadDataCompleted += this.OnArtDownload;
                this._ArtDownloader.DownloadDataAsync(new Uri(artUrl));
            }

            this._UserStatsReceivedCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            this._UserStatsReceivedCallback.OnRun += this.OnUserStatsReceived;

            this._UserStatsStoredCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsStored>();
            this._UserStatsStoredCallback.OnRun += this.OnUserStatsStored;

            this.RefreshStats();
        }

        private void SetupStatisticsGrid()
        {
            var grid = this._StatisticsDataGridView;
            grid.AutoGenerateColumns = false;

            grid.Columns.Add("name", "Name");
            grid.Columns[0].ReadOnly = true;
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns[0].DataPropertyName = "DisplayName";

            grid.Columns.Add("value", "Value");
            grid.Columns[1].ReadOnly = this._EnableStatsEditingCheckBox.Checked == false;
            grid.Columns[1].Width = Theme.Scale(160);
            grid.Columns[1].DataPropertyName = "Value";

            grid.Columns.Add("extra", "Extra");
            grid.Columns[2].ReadOnly = true;
            grid.Columns[2].Width = Theme.Scale(220);
            grid.Columns[2].DataPropertyName = "Extra";

            grid.RowTemplate.Height = Theme.Scale(36);
            grid.ColumnHeadersHeight = Theme.Scale(36);
            grid.RowHeadersWidth = Theme.Scale(30);

            foreach (var style in new[] { grid.ColumnHeadersDefaultCellStyle, grid.RowHeadersDefaultCellStyle })
            {
                style.BackColor = Theme.Background;
                style.ForeColor = Theme.TextMuted;
                style.SelectionBackColor = Theme.Background;
                style.SelectionForeColor = Theme.TextMuted;
                style.Font = Theme.BodyStrong;
            }

            grid.DefaultCellStyle.BackColor = Theme.Background;
            grid.DefaultCellStyle.ForeColor = Theme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Theme.SurfaceRaised;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
            grid.DefaultCellStyle.Font = Theme.Body;

            grid.DataSource = new BindingSource()
            {
                DataSource = this._Statistics,
            };

            grid.HandleCreated += (sender, e) => Theme.ApplyDarkScrollbars(grid);
        }

        private void SetHasArt(bool hasArt)
        {
            this._HasArt = hasArt;
            this._GameArt.Visible = hasArt;
            this._HeaderPanel.PerformLayout();
        }

        private void OnArtDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled == true)
            {
                this.SetHasArt(false);
                return;
            }

            try
            {
                using (MemoryStream stream = new(e.Result, false))
                using (Bitmap decoded = new(stream))
                {
                    this._GameArt.Image = new(decoded);
                }
                this.SetHasArt(true);
            }
            catch (Exception)
            {
                this.SetHasArt(false);
            }
        }

        private static string GetIconName(Stats.AchievementInfo info)
        {
            return info.IsAchieved == true ? info.IconNormal : info.IconLocked;
        }

        private void OnIconDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                var info = (Stats.AchievementInfo)e.UserState;

                try
                {
                    using (MemoryStream stream = new(e.Result, false))
                    using (Bitmap decoded = new(stream))
                    {
                        Bitmap icon = new(decoded);
                        this._Icons[GetIconName(info)] = icon;
                        info.Icon = icon;
                    }
                }
                catch (Exception)
                {
                    info.Icon = null;
                }

                this._AchievementList.Invalidate();
            }

            this.DownloadNextIcon();
        }

        private void DownloadNextIcon()
        {
            foreach (var downloader in this._IconDownloaders)
            {
                if (downloader.IsBusy == false && this.TakeNextIcon() is Stats.AchievementInfo info)
                {
                    downloader.DownloadDataAsync(
                        new Uri(_($"https://shared.steamstatic.com/community_assets/images/apps/{this._GameId}/{GetIconName(info)}")),
                        info);
                }
            }

            int remaining = this._IconQueue.Count + this._IconDownloaders.Count(downloader => downloader.IsBusy == true);
            this._DownloadStatusLabel.Text = $"Downloading {Counted(remaining, "icon")}...";
            this._DownloadStatusLabel.Visible = remaining > 0;
        }

        private Stats.AchievementInfo TakeNextIcon()
        {
            var visible = this._AchievementList.GetVisibleItems();
            while (this._IconQueue.Count > 0)
            {
                var info = this._IconQueue.FirstOrDefault(visible.Contains) ?? this._IconQueue[0];
                this._IconQueue.Remove(info);

                if (this._Icons.TryGetValue(GetIconName(info), out var icon) == false)
                {
                    return info;
                }

                info.Icon = icon;
                this._AchievementList.Invalidate();
            }

            return null;
        }

        private static string TranslateError(int id) => id switch
        {
            2 => "generic error -- this usually means you don't own the game",
            3 => "no connection to Steam, please check that Steam is online",
            6 => "this account is logged in somewhere else",
            8 => "one or more values broke the game's limits or were out of date",
            15 => "access denied",
            16 => "Steam timed out, please try again",
            20 => "Steam is unavailable right now, please try again later",
            21 => "not logged on to Steam",
            84 => "Steam is limiting requests, please try again later",
            _ => _($"{id}"),
        };

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }

        private bool LoadUserGameStatsSchema()
        {
            string path;
            try
            {
                string fileName = _($"UserGameStatsSchema_{this._GameId}.bin");
                path = API.Steam.GetInstallPath();
                path = Path.Combine(path, "appcache", "stats", fileName);
                if (File.Exists(path) == false)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);
            if (kv == null)
            {
                return false;
            }

            var currentLanguage = this._SteamClient.SteamApps008.GetCurrentGameLanguage();

            this._AchievementDefinitions.Clear();
            this._StatDefinitions.Clear();

            var stats = kv[this._GameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false || stats.Children == null)
            {
                return false;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                APITypes.UserStatType type;

                // schema in the new format?
                var typeNode = stat["type"];
                if (typeNode.Valid == true && typeNode.Type == KeyValueType.String)
                {
                    if (Enum.TryParse((string)typeNode.Value, true, out type) == false)
                    {
                        type = APITypes.UserStatType.Invalid;
                    }
                }
                else
                {
                    type = APITypes.UserStatType.Invalid;
                }

                // schema in the old format?
                if (type == APITypes.UserStatType.Invalid)
                {
                    var typeIntNode = stat["type_int"];
                    var rawType = typeIntNode.Valid == true
                        ? typeIntNode.AsInteger(0)
                        : typeNode.AsInteger(0);
                    type = (APITypes.UserStatType)rawType;
                }

                switch (type)
                {
                    case APITypes.UserStatType.Invalid:
                    {
                        break;
                    }

                    case APITypes.UserStatType.Integer:
                    {
                        var id = stat["name"].AsString("");
                        string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        this._StatDefinitions.Add(new Stats.IntegerStatDefinition()
                        {
                            Id = stat["name"].AsString(""),
                            DisplayName = name,
                            MinValue = stat["min"].AsInteger(int.MinValue),
                            MaxValue = stat["max"].AsInteger(int.MaxValue),
                            MaxChange = stat["maxchange"].AsInteger(0),
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            SetByTrustedGameServer = stat["bSetByTrustedGS"].AsBoolean(false),
                            DefaultValue = stat["default"].AsInteger(0),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case APITypes.UserStatType.Float:
                    case APITypes.UserStatType.AverageRate:
                    {
                        var id = stat["name"].AsString("");
                        string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        this._StatDefinitions.Add(new Stats.FloatStatDefinition()
                        {
                            Id = stat["name"].AsString(""),
                            DisplayName = name,
                            MinValue = stat["min"].AsFloat(float.MinValue),
                            MaxValue = stat["max"].AsFloat(float.MaxValue),
                            MaxChange = stat["maxchange"].AsFloat(0.0f),
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            DefaultValue = stat["default"].AsFloat(0.0f),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case APITypes.UserStatType.Achievements:
                    case APITypes.UserStatType.GroupAchievements:
                    {
                        if (stat.Children != null)
                        {
                            foreach (var bits in stat.Children.Where(
                                b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                            {
                                if (bits.Valid == false || bits.Children == null)
                                {
                                    continue;
                                }

                                foreach (var bit in bits.Children)
                                {
                                    string id = bit["name"].AsString("");
                                    string name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                    string desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                    this._AchievementDefinitions.Add(new()
                                    {
                                        Id = id,
                                        Name = name,
                                        Description = desc,
                                        IconNormal = bit["display"]["icon"].AsString(""),
                                        IconLocked = bit["display"]["icon_gray"].AsString(""),
                                        IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                        Permission = bit["permission"].AsInteger(0),
                                    });
                                }
                            }
                        }

                        break;
                    }

                    default:
                    {
                        throw new InvalidOperationException("invalid stat type");
                    }
                }
            }

            return true;
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceived param)
        {
            if (param.Result != 1)
            {
                this._GameStatusLabel.Text = $"Error while retrieving stats: {TranslateError(param.Result)}";
                this.EnableInput();
                return;
            }

            if (this.LoadUserGameStatsSchema() == false)
            {
                this._GameStatusLabel.Text = "Failed to load schema.";
                this.EnableInput();
                return;
            }

            try
            {
                this.GetAchievements();
            }
            catch (Exception e)
            {
                this._GameStatusLabel.Text = "Error when handling achievements retrieval.";
                this.EnableInput();
                ThemedDialog.Show(
                    "Error when handling achievements retrieval:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.GetStatistics();
            }
            catch (Exception e)
            {
                this._GameStatusLabel.Text = "Error when handling stats retrieval.";
                this.EnableInput();
                ThemedDialog.Show(
                    "Error when handling stats retrieval:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            this._GameStatusLabel.Text = $"Retrieved {Counted(this._Achievements.Count, "achievement")} and {Counted(this._Statistics.Count, "statistic")}.";
            this.UpdateProgress();
            this.UpdateProtectedNotice();
            this.UpdatePending();
            this.EnableInput();
        }

        private void RefreshStats()
        {
            this._Achievements.Clear();
            this._AchievementList.SetItems(new());
            this._AchievementList.IsLoading = true;
            this.UpdateProtectedNotice();
            this._StatisticsDataGridView.Rows.Clear();

            var steamId = this._SteamClient.SteamUser.GetSteamId();

            // This still triggers the UserStatsReceived callback, in addition to the callresult.
            // No need to implement callresults for the time being.
            var callHandle = this._SteamClient.SteamUserStats.RequestUserStats(steamId);
            if (callHandle == API.CallHandle.Invalid)
            {
                ThemedDialog.Show(this, "Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this._GameStatusLabel.Text = "Retrieving stat information...";
            this.DisableInput();
        }

        private void GetAchievements()
        {
            this._Achievements.Clear();
            this._IconQueue.Clear();

            foreach (var def in this._AchievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.Id) == true)
                {
                    continue;
                }

                if (this._SteamClient.SteamUserStats.GetAchievementAndUnlockTime(
                    def.Id,
                    out bool isAchieved,
                    out var unlockTime) == false)
                {
                    continue;
                }

                var hasName = def.Name.StartsWith("#", StringComparison.InvariantCulture) == false;

                Stats.AchievementInfo info = new()
                {
                    Id = def.Id,
                    IsAchieved = isAchieved,
                    IsChecked = isAchieved,
                    UnlockTime = isAchieved == true && unlockTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime
                        : null,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    Permission = def.Permission,
                    Name = hasName == true ? def.Name : def.Id,
                    Description = hasName == true ? def.Description : "",
                };

                this._Achievements.Add(info);
                this.AddAchievementToIconQueue(info);
            }

            this.ApplyAchievementFilter();
            this.DownloadNextIcon();
        }

        private void ApplyAchievementFilter()
        {
            var textSearch = this._MatchingStringTextBox.Text.Length > 0
                ? this._MatchingStringTextBox.Text
                : null;

            bool wantLocked = this._ShowLockedChip.Checked == true;
            bool wantUnlocked = this._ShowUnlockedChip.Checked == true;

            var visible = this._Achievements.Where(info =>
            {
                bool wanted = (wantLocked == false && wantUnlocked == false) ||
                              (info.IsAchieved == true ? wantUnlocked : wantLocked);
                if (wanted == false)
                {
                    return false;
                }

                return textSearch == null ||
                       info.Name.IndexOf(textSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       info.Description.IndexOf(textSearch, StringComparison.OrdinalIgnoreCase) >= 0;
            }).ToList();

            if (this._Achievements.Count > 0)
            {
                this._AchievementList.EmptyTitle = "No achievements match";
                this._AchievementList.EmptyHint = "Try a different search, or clear the Locked and Unlocked filters.";
            }

            this._AchievementList.SetItems(visible);
        }

        private void GetStatistics()
        {
            this._Statistics.Clear();
            foreach (var stat in this._StatDefinitions)
            {
                if (string.IsNullOrEmpty(stat.Id) == true)
                {
                    continue;
                }

                if (stat is Stats.IntegerStatDefinition intStat)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(intStat.Id, out int value) == false)
                    {
                        continue;
                    }
                    this._Statistics.Add(new Stats.IntStatInfo()
                    {
                        Id = intStat.Id,
                        DisplayName = intStat.DisplayName,
                        IntValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = intStat.IncrementOnly,
                        Permission = intStat.Permission,
                        MinValue = intStat.MinValue,
                        MaxValue = intStat.MaxValue,
                        MaxChange = intStat.MaxChange,
                    });
                }
                else if (stat is Stats.FloatStatDefinition floatStat)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(floatStat.Id, out float value) == false)
                    {
                        continue;
                    }
                    this._Statistics.Add(new Stats.FloatStatInfo()
                    {
                        Id = floatStat.Id,
                        DisplayName = floatStat.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatStat.IncrementOnly,
                        Permission = floatStat.Permission,
                        MinValue = floatStat.MinValue,
                        MaxValue = floatStat.MaxValue,
                        MaxChange = floatStat.MaxChange,
                    });
                }
            }
        }

        private void AddAchievementToIconQueue(Stats.AchievementInfo info)
        {
            var iconName = GetIconName(info);
            if (string.IsNullOrEmpty(iconName) == true)
            {
                return;
            }

            if (this._Icons.TryGetValue(iconName, out var icon) == true)
            {
                info.Icon = icon;
            }
            else
            {
                this._IconQueue.Add(info);
            }
        }

        private List<Stats.AchievementInfo> GetChangedAchievements()
        {
            return this._Achievements.Where(info => info.IsChanged == true).ToList();
        }

        private int StoreAchievements(List<Stats.AchievementInfo> achievements, List<string> failed)
        {
            int count = 0;
            foreach (var info in achievements)
            {
                if (this._SteamClient.SteamUserStats.SetAchievement(info.Id, info.IsChecked) == false)
                {
                    failed.Add(info.Id);
                    continue;
                }

                info.IsAchieved = info.IsChecked;
                count++;
            }
            return count;
        }

        private int StoreStatistics(List<Stats.StatInfo> statistics, List<string> failed)
        {
            int count = 0;
            foreach (var stat in statistics)
            {
                bool stored = stat switch
                {
                    Stats.IntStatInfo intStat => this._SteamClient.SteamUserStats.SetStatValue(
                        intStat.Id,
                        intStat.IntValue),
                    Stats.FloatStatInfo floatStat => this._SteamClient.SteamUserStats.SetStatValue(
                        floatStat.Id,
                        floatStat.FloatValue),
                    _ => throw new InvalidOperationException("unsupported stat type"),
                };

                if (stored == false)
                {
                    failed.Add(stat.Id);
                    continue;
                }

                count++;
            }
            return count;
        }

        private static string DescribeFailures(List<string> failed)
        {
            const int limit = 10;
            var names = string.Join("\n", failed.Take(limit));
            var more = failed.Count > limit ? $"\n...and {failed.Count - limit} more" : "";
            var subject = failed.Count == 1 ? "1 change, so it was" : $"{failed.Count} changes, so they were";
            return $"Steam refused {subject} skipped:\n\n{names}{more}\n\n" +
                   "These are usually protected, or part of DLC you don't own.";
        }

        private static string Counted(int count, string noun)
        {
            return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
        }

        private void DisableInput()
        {
            this._ReloadButton.Enabled = false;
            this._StoreButton.Enabled = false;
        }

        private void EnableInput()
        {
            this._ReloadButton.Enabled = true;
            this._StoreButton.Enabled = true;
            this._AchievementList.IsLoading = false;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._CallbackTimer.Enabled = false;
            this._SteamClient.RunCallbacks(false);

            if (this._StoreDeadline.HasValue == true &&
                DateTime.UtcNow >= this._StoreDeadline.Value)
            {
                this._StoreDeadline = null;
                ThemedDialog.Show(
                    this,
                    "Steam hasn't confirmed the changes yet. Please check that the refreshed list looks right.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                this.RefreshStats();
            }

            this._CallbackTimer.Enabled = true;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this.RefreshStats();
        }

        private void SetVisibleAchievements(Func<Stats.AchievementInfo, bool> state)
        {
            foreach (var info in this._AchievementList.Items)
            {
                if (info.IsProtected == false)
                {
                    info.IsChecked = state(info);
                }
            }

            this._AchievementList.Invalidate();
            this.UpdatePending();
        }

        private void OnLockAll(object sender, EventArgs e)
        {
            this.SetVisibleAchievements(info => false);
        }

        private void OnInvertAll(object sender, EventArgs e)
        {
            this.SetVisibleAchievements(info => info.IsChecked == false);
        }

        private void OnUnlockAll(object sender, EventArgs e)
        {
            this.SetVisibleAchievements(info => true);
        }

        private bool Store()
        {
            if (this._SteamClient.SteamUserStats.StoreStats() == false)
            {
                ThemedDialog.Show(
                    this,
                    "An error occurred while storing, aborting.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool ConfirmStore(List<Stats.AchievementInfo> achievements, List<Stats.StatInfo> statistics)
        {
            int unlocks = achievements.Count(info => info.IsChecked == true);
            int locks = achievements.Count - unlocks;

            return ThemedDialog.Show(
                this,
                $"This will unlock {Counted(unlocks, "achievement")}, lock {Counted(locks, "achievement")} and change " +
                $"{Counted(statistics.Count, "statistic")} on your Steam account.\n\nContinue?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void OnStore(object sender, EventArgs e)
        {
            // Clicking a toolbar button doesn't end an edit that is still in progress.
            if (this._StatisticsDataGridView.EndEdit() == false)
            {
                ThemedDialog.Show(
                    this,
                    "Please fix the statistic marked with an error, then try again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var changedAchievements = this.GetChangedAchievements();
            var changedStatistics = this._Statistics.Where(stat => stat.IsModified == true).ToList();
            if (changedAchievements.Count == 0 && changedStatistics.Count == 0)
            {
                ThemedDialog.Show(
                    this,
                    "There are no changes to store.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (this.ConfirmStore(changedAchievements, changedStatistics) == false)
            {
                return;
            }

            List<string> failed = new();
            int achievements = this.StoreAchievements(changedAchievements, failed);
            int stats = this.StoreStatistics(changedStatistics, failed);

            if (achievements == 0 && stats == 0)
            {
                ThemedDialog.Show(
                    this,
                    DescribeFailures(failed),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.RefreshStats();
                return;
            }

            if (this.Store() == false)
            {
                this.RefreshStats();
                return;
            }

            this._StoreSummary = $"Stored {Counted(achievements, "achievement")} and {Counted(stats, "statistic")}.";
            if (failed.Count > 0)
            {
                this._StoreSummary += "\n\n" + DescribeFailures(failed);
            }

            this._StoreHadFailures = failed.Count > 0;
            this._StoreDeadline = DateTime.UtcNow.AddSeconds(StoreConfirmationSeconds);

            this._GameStatusLabel.Text = "Waiting for Steam to confirm...";
            this.DisableInput();
        }

        private void OnUserStatsStored(APITypes.UserStatsStored param)
        {
            if (this._StoreDeadline.HasValue == false)
            {
                return;
            }

            this._StoreDeadline = null;

            // Finish outside the callback, a dialog here would keep it open until dismissed.
            this.BeginInvoke(new Action<int>(this.FinishStore), param.Result);
        }

        private void FinishStore(int result)
        {
            if (result == 1)
            {
                ThemedDialog.Show(
                    this,
                    this._StoreSummary,
                    "Information",
                    MessageBoxButtons.OK,
                    this._StoreHadFailures == true ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            else
            {
                ThemedDialog.Show(
                    this,
                    $"Steam did not accept the changes: {TranslateError(result)}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            this.RefreshStats();
        }

        private void OnStatDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Context != DataGridViewDataErrorContexts.Commit)
            {
                return;
            }

            var view = (DataGridView)sender;
            if (e.Exception is Stats.StatIsProtectedException)
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = "Stat is protected! -- you can't modify it";
            }
            else if (e.Exception is Stats.StatConstraintException)
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = e.Exception.Message;
            }
            else
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = "Invalid value";
            }

            this._GameStatusLabel.Text = view.Rows[e.RowIndex].ErrorText;
        }

        private void OnStatAgreementChecked(object sender, EventArgs e)
        {
            this._StatisticsDataGridView.Columns[1].ReadOnly = this._EnableStatsEditingCheckBox.Checked == false;
        }

        private void OnStatCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var view = (DataGridView)sender;
            view.Rows[e.RowIndex].ErrorText = "";
            this.UpdatePending();
        }

        private void OnResetAllStats(object sender, EventArgs e)
        {
            if (ThemedDialog.Show(
                "Are you absolutely sure you want to reset stats?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            bool achievementsToo = DialogResult.Yes == ThemedDialog.Show(
                "Do you want to reset achievements too?",
                "Question",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ThemedDialog.Show(
                "Really really sure?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error) == DialogResult.No)
            {
                return;
            }

            if (this._SteamClient.SteamUserStats.ResetAllStats(achievementsToo) == false)
            {
                ThemedDialog.Show(this, "Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.RefreshStats();
        }

        private void OnAchievementToggled(Stats.AchievementInfo info)
        {
            this.UpdatePending();
        }

        private void OnProtectedToggled(Stats.AchievementInfo info)
        {
            this._GameStatusLabel.Text = $"{info.Name} is protected and can't be changed with this tool.";
        }

        private void OnShowLockedOnly(object sender, EventArgs e)
        {
            if (this._ShowLockedChip.Checked == true)
            {
                this._ShowUnlockedChip.Checked = false;
            }

            this.ApplyAchievementFilter();
        }

        private void OnShowUnlockedOnly(object sender, EventArgs e)
        {
            if (this._ShowUnlockedChip.Checked == true)
            {
                this._ShowLockedChip.Checked = false;
            }

            this.ApplyAchievementFilter();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            this.ApplyAchievementFilter();
        }

        private void OnShowAchievements(object sender, EventArgs e)
        {
            this.ShowTab(true);
        }

        private void OnShowStatistics(object sender, EventArgs e)
        {
            this.ShowTab(false);
        }

        private void ShowTab(bool showAchievements)
        {
            this._AchievementsTabChip.Checked = showAchievements;
            this._StatisticsTabChip.Checked = showAchievements == false;
            this._AchievementsPanel.Visible = showAchievements;
            this._StatisticsPanel.Visible = showAchievements == false;
        }

        private void UpdateProgress()
        {
            int unlocked = this._Achievements.Count(info => info.IsAchieved == true);
            int total = this._Achievements.Count;
            this._ProgressLabel.Text = $"{unlocked} of {Counted(total, "achievement")} unlocked";
            this._ProgressBar.Value = total > 0 ? (float)unlocked / total : 0;
        }

        private void UpdateProtectedNotice()
        {
            int total = this._Achievements.Count;
            int guarded = this._Achievements.Count(info => info.IsProtected == true);
            bool allGuarded = total > 0 && guarded == total;

            // Bulk changes skip protected rows, so with nothing else left they would do nothing.
            foreach (var button in new[] { this._LockAllButton, this._InvertAllButton, this._UnlockAllButton })
            {
                button.Enabled = allGuarded == false;
            }

            if (guarded == 0)
            {
                this._ProtectedNotice.Visible = false;
                return;
            }

            if (allGuarded == true)
            {
                this._ProtectedNotice.Title = "This game's achievements are controlled by its servers";
                this._ProtectedNotice.Body =
                    "Steam only accepts them from the game's own servers, so SAM Modern can't change them. " +
                    "Many online games work this way, including most Ubisoft titles.";
            }
            else
            {
                this._ProtectedNotice.Title =
                    $"{guarded} of {Counted(total, "achievement")} {(guarded == 1 ? "is" : "are")} controlled by the game's servers";
                this._ProtectedNotice.Body =
                    "They're marked Protected. Steam only accepts them from the game's own servers, so SAM Modern can't change them.";
            }

            this._ProtectedNotice.Visible = true;
        }

        private void UpdatePending()
        {
            int pending = this.GetChangedAchievements().Count + this._Statistics.Count(stat => stat.IsModified == true);
            this._StoreButton.Kind = pending > 0 ? ThemedButtonKind.Primary : ThemedButtonKind.Secondary;
            this._StoreButton.Invalidate();
            this._PendingLabel.Text = pending switch
            {
                0 => "",
                1 => "1 unsaved change",
                _ => $"{pending} unsaved changes",
            };
            this._HeaderPanel.PerformLayout();
        }

        private void OnHeaderLayout(object sender, LayoutEventArgs e)
        {
            int margin = Theme.Scale(20);
            int gap = Theme.Scale(8);
            int width = this._HeaderPanel.ClientSize.Width;
            int top = Theme.Scale(16);
            int buttonHeight = Theme.Scale(34);
            int artHeight = Theme.Scale(72);

            this._GameArt.SetBounds(margin, top, artHeight * 231 / 87, artHeight);

            int right = width - margin;
            foreach (var button in new[] { this._StoreButton, this._ResetButton, this._ReloadButton })
            {
                int buttonWidth = button.FitWidth();
                button.SetBounds(right - buttonWidth, top, buttonWidth, buttonHeight);
                right = button.Left - gap;
            }

            int textLeft = this._HasArt == true ? this._GameArt.Right + Theme.Scale(16) : margin;
            this._GameNameLabel.SetBounds(
                textLeft - Theme.Scale(4),
                top - Theme.Scale(2),
                Math.Max(0, right - textLeft),
                Theme.Scale(34));
            this._ProgressLabel.Location = new(textLeft - Theme.Scale(3), top + Theme.Scale(36));
            this._ProgressBar.SetBounds(textLeft, top + Theme.Scale(62), Theme.Scale(240), Theme.Scale(6));

            this._PendingLabel.Location = new(
                width - margin - this._PendingLabel.Width,
                top + buttonHeight + Theme.Scale(9));

            int tabTop = top + artHeight + Theme.Scale(12);
            int tabLeft = margin;
            foreach (var tab in new[] { this._AchievementsTabChip, this._StatisticsTabChip })
            {
                int tabWidth = tab.FitWidth();
                tab.SetBounds(tabLeft, tabTop, tabWidth, Theme.Scale(32));
                tabLeft += tabWidth + gap;
            }
        }

        private void OnToolbarLayout(object sender, LayoutEventArgs e)
        {
            int margin = Theme.Scale(20);
            int gap = Theme.Scale(8);
            int top = Theme.Scale(14);
            int height = Theme.Scale(34);

            int right = this._AchievementToolbar.ClientSize.Width - margin;
            foreach (var button in new[] { this._UnlockAllButton, this._InvertAllButton, this._LockAllButton })
            {
                int buttonWidth = button.FitWidth();
                button.SetBounds(right - buttonWidth, top, buttonWidth, height);
                right = button.Left - Theme.Scale(2);
            }

            int chipsWidth = this._ShowLockedChip.FitWidth() + this._ShowUnlockedChip.FitWidth() + (gap * 2);
            int searchWidth = Math.Max(Theme.Scale(140), Math.Min(Theme.Scale(300), right - margin - chipsWidth - gap));
            this._MatchingStringTextBox.SetBounds(margin, top, searchWidth, height);

            int left = this._MatchingStringTextBox.Right + gap;
            foreach (var chip in new[] { this._ShowLockedChip, this._ShowUnlockedChip })
            {
                int chipWidth = chip.FitWidth();
                chip.SetBounds(left, top, chipWidth, height);
                left += chipWidth + gap;
            }
        }

        private void OnStatisticsLayout(object sender, LayoutEventArgs e)
        {
            int margin = Theme.Scale(20);
            var size = this._StatisticsPanel.ClientSize;
            int width = Math.Max(0, size.Width - (margin * 2));

            this._EnableStatsEditingCheckBox.SetBounds(
                margin,
                Theme.Scale(14),
                Math.Min(width, this._EnableStatsEditingCheckBox.FitWidth()),
                Theme.Scale(34));

            int top = this._EnableStatsEditingCheckBox.Bottom + Theme.Scale(12);
            this._StatisticsDataGridView.SetBounds(margin, top, width, Math.Max(0, size.Height - top - margin));
        }

        private void OnFooterLayout(object sender, LayoutEventArgs e)
        {
            int margin = Theme.Scale(20);
            var size = this._FooterPanel.ClientSize;
            int downloadWidth = Theme.Scale(220);
            this._DownloadStatusLabel.SetBounds(size.Width - margin - downloadWidth, 0, downloadWidth, size.Height);
            this._GameStatusLabel.SetBounds(margin, 0, this._DownloadStatusLabel.Left - margin - Theme.Scale(8), size.Height);
        }
    }
}

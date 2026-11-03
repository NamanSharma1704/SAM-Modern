# SAM Modern FAQ

SAM Modern is a modified version of Gibbed's Steam Achievement Manager (SAM), so most of these answers apply to both. Numbered issue links go to the original SAM's issue tracker.

## Where should I download SAM Modern?

Only from the [SAM Modern releases page](https://github.com/NamanSharma1704/SAM-Modern/releases/latest), or build it yourself as the README describes. Other sites host copies of SAM, and there is no way to check those copies against this source code. The original SAM is on [Gibbed's releases page](https://github.com/gibbed/SteamAchievementManager/releases/latest).

## Is SAM a virus?

Antivirus products sometimes flag it ([#422](https://github.com/gibbed/SteamAchievementManager/issues/422), [#336](https://github.com/gibbed/SteamAchievementManager/issues/336), [#478](https://github.com/gibbed/SteamAchievementManager/issues/478)). A likely reason is that SAM loads Steam's own client library and tells Steam it is a game, which is what a lot of unwanted software does too. The source is all in this repository, so you can read it or build it yourself if you would rather not trust a download.

SAM Modern connects to two places: `gib.me`, Gibbed's site, for the game list, and `shared.steamstatic.com` for images. Everything else goes through your running Steam client.

## Will I get banned?

Nobody can promise you won't, so here is what is known.

From Valve: we are not aware of an official statement about SAM, or of a documented VAC ban caused by it. SAM does not touch game processes. Close a game before you open it in SAM anyway, and don't use SAM while connected to a VAC-protected server.

From game developers: they make their own decisions. Steam forum threads name Payday 2 and Dead by Daylight as games where achievement manipulation has led to action, usually because achievements unlocked in-game rewards. Treat multiplayer games with extra care.

From achievement trackers: yes, this one is real. The [Unified Achievement Hunting Rules](https://achievementhunting.com/rules) name SAM as a critical offense. Sites that follow them or share data, including Steam Hunters, Exophase and completionist.me, can ban you from their leaderboards and mark you publicly as a cheater. Those rules cover relocking too. If you have been flagged, or want to come clean, talk to their moderators before you relock anything.

## Why is my game list empty, or showing only Spacewar?

SAM could not download the game list. SAM Modern saves the last list it received, so this should only happen if the download has never worked on your PC. To add a game by hand, find its App ID on [SteamDB](https://steamdb.info/), type it into the App ID box at the top of the window, and click Add game.

The saved list is at `%LOCALAPPDATA%\SAM\games.xml`.

## Why does SAM list every game on Steam?

Something on your PC is making Steam believe you own everything. Turn off any tool that fakes game ownership, then restart Steam.

## Why does a game show no achievements, or "Failed to load schema"?

First check that you have the latest version, since old versions of SAM cannot read Steam's newer schema format. If that doesn't help, close SAM and Steam, delete the `appcache\stats` folder inside your Steam folder, and start Steam again. Steam rebuilds that folder by itself.

## What does "Error while retrieving stats" mean?

SAM Modern describes the common codes in plain words. Code 2 usually means you don't own the game. Code 3 means no connection to Steam.

## Why do some rows say Protected, or why can't I edit them?

Those achievements and stats are protected, which means only a game server can set them. SAM cannot change them from your PC.

Stats can also have limits set by the game: a highest and lowest value, a largest change per save, or a rule that the value can only go up. SAM Modern tells you which limit applies when you edit the value.

## Why can't SAM change achievements in some Ubisoft games?

In many Ubisoft games, such as The Division 2, For Honor and Ghost Recon Wildlands, every achievement is protected. Ubisoft Connect records it and passes it on to Steam, and Steam won't accept it from anywhere else. SAM Modern shows a notice when you open one of these games. Some Ubisoft games, like Assassin's Creed Odyssey, don't use protection, and SAM works normally for those.

Tools that edit Ubisoft Connect's own files exist, but they break Ubisoft's terms and put your whole Ubisoft account at risk, not just one game.

## Why did Steam refuse a change?

Steam can refuse an achievement or a stat without saying why. SAM Modern skips those rows, saves the rest, and lists what it skipped. Since the Achievement Sets update, people also report DLC achievements being refused when they don't own the DLC ([#651](https://github.com/gibbed/SteamAchievementManager/issues/651), [#639](https://github.com/gibbed/SteamAchievementManager/issues/639)).

## Why does an achievement come back after I lock it?

There are two usual causes, and the [AchievementHunting relock guide](https://achievementhunting.com/guides/relock) covers both in detail.

Your save file is the first. Many games award achievements again when they load a save. Turn off Steam Cloud for the game, delete the save, and start a new one.

Tracking stats are the second. Some achievements unlock when a stat reaches a target, so the achievement returns while the stat stays high. Often that stat can only go up, and then the only fix is resetting the whole game with Reset in SAM or `reset_all_stats <appid>` in the Steam console.

## Steam says a game is installed when it isn't, or won't let me install it

Restart Steam. This also clears up games that seem to have been added to your library.

## Steam logged me out, shut down, or says I'm logged in elsewhere

Close any other tool using the same account, ArchiSteamFarm in particular, and try again ([#439](https://github.com/gibbed/SteamAchievementManager/issues/439)).

## SAM.Game.exe won't start

Keep every file from the release zip together in one folder, including the `.dll` and `.config` files, and check that your antivirus hasn't quarantined one of them.

## Can I change an achievement's unlock date, or restore the original one?

No. Steam records the time you unlock it, and there is no way to set a date.

## Can I use SAM on games I don't own?

No. SAM checks ownership through Steam before it lists a game.

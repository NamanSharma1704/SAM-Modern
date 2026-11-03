# SAM Modern

SAM Modern is a modified version of [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager) (SAM) by Gibbed, with a redesigned interface and fixes for common problems. It unlocks and locks achievements and edits stats for games you own on Steam. Neither Gibbed nor Valve made or endorses it.

It needs Windows, the [Steam client](https://store.steampowered.com/about/) running and logged in, and network access.

[Download the latest release](https://github.com/NamanSharma1704/SAM-Modern/releases/latest), or build it yourself as described below.

Having trouble, or wondering whether it is safe? Read the [FAQ](FAQ.md).

## Changes from SAM 7.0

- A dark interface for both windows. Games show as a grid of artwork with search and type filters. Achievements show their icons, a progress bar, and which ones will change when you save.
- Saving waits for Steam to confirm. If Steam refuses some achievements or stats, the rest are still saved and the refused ones are listed.
- Protected achievements and stats are marked, and a notice explains when a game's servers control all of them. Stat limits are checked before saving.
- The game list is kept after each download, so the picker still works when the download fails.
- Errors are described in plain words.
- Achievement icons load from Steam's current image server, several at a time, starting with the ones on screen.

## Building

You need the [.NET SDK](https://dotnet.microsoft.com/download). The .NET Framework 4.8 it targets comes with Windows 10 and 11.

```bash
dotnet build SAM.sln -c Release
```

The program ends up in the `upload` folder. Keep its files together and run `SAM.Picker.exe`.

## Licence and credits

SAM Modern keeps the original's zlib licence ([LICENSE.txt](LICENSE.txt)). Files from the original keep Rick's (Gibbed's) copyright notice, and those changed for SAM Modern say so at the top. Files written for SAM Modern carry their own notice.

The original SAM was first released, closed-source, in 2008, and Gibbed later published its code. SAM Modern still downloads its game list from Gibbed's site, `gib.me`.

Interface icons come from the Segoe Fluent Icons font that ships with Windows 11, or Segoe MDL2 Assets on Windows 10. The program icon is the original SAM's, which credits the [Fugue Icons](https://p.yusukekamiyamane.com/) set for its icons.

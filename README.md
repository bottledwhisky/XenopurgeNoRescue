# XenopurgeCheat

A MelonLoader mod for Xenopurge.

![screenshot](screenshots/screenshot.png)

## Features

- +100 Speed
- A settings menu (requires MelonPreferencesManager) to:
    - Toggle "+100 Speed"
    - Unlock all squads
    - Unlock all variants
    - Unlock all maps (a.k.a difficulties)
    - Complete all challenges

## Requirements

- [MelonLoader](https://melonloader.co/)
- [MelonPreferencesManager](https://github.com/Bluscream/MelonPreferencesManager/releases) (MelonPrefManager.Mono.dll)

## Installation

Skip to step 3 if you already have MelonLoader and MelonPreferencesManager installed.

1. Install MelonLoader
    a. `<game_directory>` is the directory where the game executable is located. For example, `C:\Program Files (x86)\Steam\steamapps\common\Xenopurge`. If you still cannot find it, right-click the game in your Steam library, select "Manage", then "Browse local files".
2. Install MelonPreferencesManager (choose Mono version)
    a. Place MelonPrefManager.Mono.dll in `<game_directory>/Mods/`
3. Place the mod DLL in `<game_directory>/Mods/`
4. Restart the game

## Usage

Press **F5** in-game to open the settings menu.

You can change the F5 keybinds in the MelonPreferencesManager's settings menu as well.

## Notes

- Mac users: MelonLoader only supports Windows and Linux. Wait for Steam Workshop support.
- Xenopurge uses Mono, not IL2CPP.

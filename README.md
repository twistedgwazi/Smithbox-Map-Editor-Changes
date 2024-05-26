# Smithbox
A personal version of DSMapStudio, which is a standalone integrated modding tool for modern FromSoft games, which include Demon's Souls (PS3), the Dark Souls series, Bloodborne, Sekiro, Elden Ring and Armored Core VI. It currently includes a map editor, a game param editor, and a text editor for editing in game text.

## Key Features
- Gparam Editor: a editor for quickly creating and editing GPARAM files.
- Map Asset Browser: view a list of all assets available for the current project type. Double-click to change current selection to chosen asset.
- Viewport Grid: support for a wireframe grid for use within the Map and Model Editor.
- Action Toolbar: support for quickly configuring various Map Editor actions.
- Replicate: powerful new Map Editor action for replicating map objects in a patterned way.
- Scramble: powerful new Map Editor action for randomising map object position, rotation and scale.
- Prefabs: support for saving groups of map objects. Supports all game types and all map object types.
- Model Editor: now supports saving edited models.
- Param Editor: improvements to the actions of renaming and duplicating rows.
- Text Editor: improvements to the action of duplicating rows.

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* [Visual C++ Redistributable x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * [Microsoft .NET Core 7.0 Desktop Runtime](https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * [Microsoft .NET Core 7.0 ASP.NET Core Runtime](https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers: NVIDIA Maxwell (900 series) and newer or AMD Polaris (Radeon 400 series) and newer
* Intel GPUs currently don't seem to be working properly. At the moment you will need a dedicated NVIDIA or AMD GPU
* A 4GB (8GB recommended) of VRAM if modding DS3/BB/Sekiro/ER maps due to huge map sizes

## Usage Instructions
#### Dark Souls: Prepare to Die Edition
* Game must be unpacked with [UDSFM](https://www.nexusmods.com/darksouls/mods/1304) before usage with Smithbox.

#### Dark Souls: Remastered
* Game is unpacked by default and requires no other tools.

#### Dark Souls II: Scholar of the First Sin
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game. Vanilla Dark Souls 2 is not supported.

#### Dark Souls III
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game.

#### Sekiro: Shadows Die Twice
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game.

#### Demon's Souls
* Make sure to disable the RPCS3 file cache to test changes if using an emulator.

#### Bloodborne
* Any valid full game dump should work out of the box. 
* Note that some dumps will have the base game (1.0) and the patch as separate, so the patch should be merged on top of the base game before use with map studio.

#### Elden Ring
* Use [UXM Selective Unpack](https://github.com/Nordgaren/UXM-Selective-Unpack) to extract the game files.

# Links
[Smithbox Discord](https://discord.gg/5p9bRKkK4J)
[DSMapStudio repository](https://github.com/soulsmods/DSMapStudio)

## Credits (DSMapStudio)
* Katalash
* philiquaz
* george
* thefifthmatt
* TKGP
* Nordgaren
* [Pav](https://github.com/JohrnaJohrna)
* [Meowmaritus](https://github.com/meowmaritus)
* [PredatorCZ](https://github.com/PredatorCZ)
* [Horkrux](https://github.com/horkrux)

## Credits (Smithbox)
* Vawser
* ivi


  

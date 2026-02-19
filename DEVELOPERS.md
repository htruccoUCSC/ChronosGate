
# Developer Guidelines

This document should be read and understood by all contributors to the project. It outlines the coding standards, project structure, and best practices to ensure consistency and quality across the codebase.

Each section contains the major concepts as well as some important DO / DO NOT examples.

## Git

We use Git LFS to track large assets (images/audio/video) efficiently.  
To install:

- Visit [this link](https://git-lfs.com/) to download git lfs
- Run the installer on your local machine
- Inside the repository run `git lfs install`
- That's it

We use Unity Smart Merge to merge scenes and prefabs.  
To setup:

- Ensure a Unity editor is installed on your local machine
- Locate `UnityYAMLMerge.exe` on your machine likely located at:
  - Windows: `C:\Program Files\Unity\Editor\Data\Tools\UnityYAMLMerge.exe`
  - Mac: `/Applications/Unity/Unity.app/Contents/Helpers/UnityYAMLMerge`
- Run the following command inside of the repository on your local machine with your UnityYAMLMerge path:

```(bash)
git config --local merge.tool unityyamlmerge
git config --local mergetool.unityyamlmerge.cmd '<UnityYAMLMerge path here>' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
git config --local mergetool.unityyamlmerge.trustExitCode false
```

## Code Style

Our team's expected coding style is based on [this official C# Code Style Guide from Unity.4f1](https://unity.com/resources/c-sharp-style-guide-unity-6)

Architecture: KISS, YAGNI, DRY, Single-Responsibility Principle

Team-specific additions:

- Prefab everything.

### Key Rules

- Indentation: 4 spaces
- Braces: K&R style (brace on the same line)
- Naming Conventions:
  - Public fields and methods in PascalCase
  - Private fields `m_camelCase`
  - Local variables `camelCase`
  - Booleans: camelCase with verb prefix: `isDirty`, `canExecute`
  - Classes: PascalCase
  - Intefaces: PascalCase with "I" prefix: `IComparable`
- Comments:
  - Don't comment bad code - rewrite it instead.
  - Use tooltips [Tooltip("...")] for Inspector fields
  - Use `//` with one space after: `// Comment text`
  - Remove commented-out code - rely on source control
  - Begin with uppercase, end with period
  - Keep TODO comments updated or delete them

## Project Layout

The `_DEV` folder is ignored (using .gitignore), and it should be used to store any developer-specific data that should not ever be included in builds. For example, developers may put questionably sourced testing assets here, temporary scaffolding code, etc.

## Commit Messages

We are *not* going to use a [conventional commit](https://www.conventionalcommits.org/en/v1.0.0/) style for commit messages. Instead, we only require that the very first line be a meaningful summary of the *impact* of the change. The summary line does not need to mention file names or code structures. Messages should start with an action verb in the imperative mood.

Good examples:

- Fix spelling in lore bible
- Improve asset loading performance with concurrency

Bad examples:

- Added Pathfinding.cs
- Fixed bug in PlayerController.Update()

## Pull Requests

Developers should avoid making large pull requests that change many subsystems at once. Instead, break up changes into smaller, focused PRs that are easier to review and test. Developer should try and be very descriptive of all changes made in PRs.

## Branch Names

Branch names should always start with the name of the developer who began the branch, followed by a short description of the feature or bug being worked on. Use hyphens to separate words.

Good examples:

- adam-dev-docs
- sonny-ci-scripts

Bad examples:

- test-branch
- playtest-1
- pathfinding

## Asset Standards

### Locations

Image assets should be located in `Assets/Images`  
Audio assets should be located in `Assets/Audio`

### Naming

Asset names should be in snake case

Good examples:

- flying_dutchman.png
- vine_boom_sound_effect.wav

Bad examples:

- IndianElephant.png
- Iaahjkfsd821dfsgdfg4.wave

### Texture Sizes

We are aiming for a 100 pixel per unity unit standard for textures.
We are also using power of 2 texture exports to support more rendering options

Example texture sizes for textures we will need:

| Asset Category | World Units (m) | Texture Size (100 PPU) | Recommended Export Size
| ----- | ----- | ----- | -----
| Small Projectiles | 0.25 x 0.25 | 25 x 25 px | 32 x 32 px
| Standard Units | 1.0 x 1.0 | 100 x 100 px | 128 x 128 px
| Large Units | 2.0 x 2.0 | 200 x 200 px | 256 x 256 px
| Environment Tiles | 1.0 x 1.0 | 100 x 100 px | 128 x 128 px
| Background (Full) | 19.2 x 10.8 | 1920 x 1080 px | 2048 x 1024 px
| UI Icons | N/A | N/A | 256 x 256 px

### Audio Formats

We are going to be using .wav files for good compression and compatibility with Unity.

Example audio specifications for sounds we will need:

| Audio Type | Format (Source) | Load Type (Unity) | Compression | Sample Rate
| ----- | ----- | ----- | ----- | -----
| Short SFX (< 1s) | .wav | Decompress on Load | PCM / ADPCM | 44,100 Hz
| Common SFX (1-5s) | .wav | Compressed In Memory | Vorbis (Qual: 50) | 44,100 Hz
| BGM / Ambience | .wav | Streaming | Vorbis (Qual: 70) | 44,100 Hz
| UI Clicks | .wav | Decompress On Load | PCM | 22,050 Hz

## UI Style

Our UI should be usable on mobile using a finger or on a computer using a mouse.

Button expectations:

- When a player hovers over a button (PC only) it should slightly grow in size
- When a player presses a button it should slightly shrink in size and appear slightly brighter
- When a button is not available to be pressed it should be greyed out
- Draggables should have a placement preview

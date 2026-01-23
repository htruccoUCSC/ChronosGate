
# Developer Guidelines

This document should be read and understood by all contributors to the project. It outlines the coding standards, project structure, and best practices to ensure consistency and quality across the codebase.

Each section contains the major concepts as well as some important DO / DO NOT examples.

## Git

We use Git LFS to track large assets (images/audio/video) efficiencly. To install: visit [this link](https://git-lfs.com/).

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

Developers should avoid making large pull requests that change many subsystems at once. Instead, break up changes into smaller, focused PRs that are easier to review and test. Developer should try and be very descriptave of all changes made in PRs.

## Branch Names

Branch names should always start with the name of the developer who began the branch, followed by a short description of the feature or bug being worked on. Use hyphens to separate words.

Good examples:

- adam-dev-docs
- sonny-ci-scripts

Bad examples:

- test-branch
- playtest-1
- pathfinding

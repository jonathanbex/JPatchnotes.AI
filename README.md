# JPatchnotes.AI
A funky patchnotes generator analyzing release code-diff and prs to generate a whacky patch notes.


Example
```
[Total Patch Notes]
# Release Notes - master

*Changes since `20241130-f35d82d5`*

## Features
- **Introducing `BotApplication` Solution**
  We've added a brand new `BotApplication.sln` to better organize our bot projects. Say hello to a more streamlined development experience!

## Improvements
- **Updated CI/CD Pipeline**
  Our GitHub Actions workflow (`.github/workflows/dotnet-desktop.yml`) now restores and builds the `BotApplication` project instead of the old `DiscordBot`. Faster, cleaner, better.

## Fixes
- **Goodbye, DiscordBot**
  Removed outdated `DiscordBot` components including controllers, middleware, and project files. It's not you, it's us. We're moving forward with `BotApplication`!

## Internal
- **Repository Cleanup**
  Tidied up the codebase by deleting deprecated `DiscordBot` solution files, middleware, and launch settings. Less clutter, more clarity.

---

Keep coding, keep smiling! ??
```

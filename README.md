# Window Maximizer

A lightweight Windows utility for resizing, repositioning, and maximizing application windows while preserving their normal appearance.

The project is currently in **v0.1** and starts as a simple command-line tool focused on reliable window manipulation.

## Features

- List open application windows
- Inspect detailed window information
- Manually set window position and size
- Maximize a window to the monitor's usable work area
- Preserve the window's normal appearance and decorations
- Support multiple monitors
- DPI-aware window manipulation
- No executable modification
- No changes to the target application

## Current Commands

### List windows

```bash
WindowMaximizer.exe --list
```

Lists visible windows with a title, including their:
- Executable name
- Process ID
- Position
- Size

### Inspect a window

```bash
WindowMaximizer.exe --info <index>
```

Displays detailed information about the selected window.
Example:
```bash
WindowMaximizer.exe --info 3
```

### Set window position and size
```bash
WindowMaximizer.exe --set <index> <x> <y> <width> <height>
```

Example:
```bash
WindowMaximizer.exe --set 3 100 100 1280 720
```

### Maximize a window
```bash
WindowMaximizer.exe --maximize <index>
```

The window is resized to the usable area of the monitor it belongs to.
The taskbar remains visible and the window keeps its normal title bar, borders, buttons, and other visual decorations.

## Building

Clone the repository and build with:
```bash
dotnet build
```

Run with:
```bash
dotnet run -- --help
```
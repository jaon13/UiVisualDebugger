# UiVisualDebugger 🚀

[![Release](https://img.shields.io/github/v/release/jaon13/UiVisualDebugger?color=blue&style=flat-square)](https://github.com/jaon13/UiVisualDebugger/releases)
[![Build Status](https://img.shields.io/github/actions/workflow/status/jaon13/UiVisualDebugger/release.yml?style=flat-square)](https://github.com/jaon13/UiVisualDebugger/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.style=flat-square)](LICENSE)

Standalone Real-Time UI Visual Debugger, Inspector & System Tray Daemon for WPF (.NET 10/Core) & WinForms (.NET 4.8/Core) applications.

---

## 📦 Downloads & Installation

- **MSI Installer / ZIP Package**: Download latest release from [GitHub Releases](https://github.com/jaon13/UiVisualDebugger/releases)

---

## 🌟 Key Features

1. **Zero Target Code Modification**: Inspects any running Windows app externally without modifying its source code.
2. **System Tray Daemon Mode**: Sits in system tray with Left-Click quick menu & Right-Click context menu.
3. **Windows Startup Registration**: Option to auto-run on Windows boot via Registry (`HKCU\..\Run`).
4. **Global Hotkey (`F12`)**: Pressing `F12` anywhere dumps active window UI tree & annotated screenshot instantly.
5. **Auto Process Watcher**: Automatically detects target app (`PhMeter.WpfApp`, etc.) launch and captures snapshot.
6. **Annotated Visual Screenshots (`annotated_ui.png`)**: Draws red bounding boxes and `[1]`, `[2]` ID badges over controls.
7. **Structured Visual Tree JSON (`antigravity_ui.json`)**: Exports Element Type, AutomationId, Name, Parent, Bounds (`X, Y, Width, Height`), and visibility.

---

## 💻 Quick Start

### 1. System Tray Resident Mode (Default)
Double-click `UiVisualDebugger.exe` or run:
```powershell
UiVisualDebugger.exe
```
- **Left-Click Tray Icon**: ⚡ Quick Menu (Snapshot, Auto-Detect toggle, Startup toggle, Open Folder)
- **Right-Click Tray Icon**: 🛠️ Context Menu (Snapshot, Settings, Exit)
- **Press `F12`**: Triggers instant snapshot capture.

### 2. One-Shot CLI Mode
```powershell
UiVisualDebugger.exe attach <ProcessName_or_PID> [OutputDir]
```

---

## 🤖 AI Agent Integration (`SKILL.md`)

This repository contains `.agents/skills/ui-visual-debugger/SKILL.md` for seamless integration with AI agent coding assistants (Antigravity Agent).

---

## 📜 License
MIT License.

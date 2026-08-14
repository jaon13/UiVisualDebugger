---
name: ui-visual-debugger
description: Performs real-time visual UI debugging for WPF (.NET 10/Core) and WinForms (.NET 4.8/Core) apps using annotated_ui.png screenshots and antigravity_ui.json Visual Tree dumps.
---

# UI Visual Debugger Agent Skill

This skill provides guidelines and instructions for AI agents to perform real-time visual UI debugging, layout analysis, alignment checking, and source code modifications for WPF and Windows Forms (.NET 10 / .NET Core / .NET Framework) applications using visual screenshots and structured Visual Tree data dumps.

## External Tool Information & Releases
- **GitHub Release (MSI Installer)**: [https://github.com/jaon13/UiVisualDebugger/releases](https://github.com/jaon13/UiVisualDebugger/releases)
- **GitHub Repository**: [https://github.com/jaon13/UiVisualDebugger.git](https://github.com/jaon13/UiVisualDebugger.git)
- **Execution Command**:
  ```powershell
  UiVisualDebugger.exe attach <ProcessName_or_PID> [OutputDir]
  ```

## 🚀 Key Features
1. **Zero Target Code Modification**: Inspects running Windows applications externally without modifying target project source code.
2. **System Tray Resident Daemon**: Auto-runs on Windows startup with Tray Left-Click quick menu & Right-Click context menu.
3. **Global Hotkey (`F12`)**: Pressing `F12` instantly dumps active window UI tree & annotated screenshot.
4. **Auto Process Watcher**: Automatically detects target process startup (`PhMeter.WpfApp`, etc.) and captures UI snapshot.

## Agent Role & Workflow
- **Target Environments**: WPF (.NET 10 / .NET Core) & WinForms (.NET Framework 4.8 / .NET Core)
- **Goal**: Match defective UI controls from `annotated_ui.png` with `antigravity_ui.json` automation IDs/names and edit XAML or C# designer source code directly.

## Input Artifact Specifications
1. `annotated_ui.png`: Visual screenshot with red bounding box borders and ID badges `[1]`, `[2]`, `[3]`.
2. `antigravity_ui.json`: Hierarchical Visual Tree JSON containing `Id`, `ElementType`, `Name`, `AutomationId`, `ParentType`, `ParentName`, `BoundsInWindow` (`X, Y, Width, Height`), `Margin`, `Padding`, `Visibility`, `DataContextType`, and `HasBindingError`.

## Execution Steps for AI Agent
1. Execute `UiVisualDebugger.exe attach <ProcessName_or_PID>` to collect UI dumps externally.
2. Inspect `annotated_ui.png` and `antigravity_ui.json` to identify defective control ID badges.
3. **WPF Projects**: Modify XAML markup (`Grid.Row`, `Margin`, `Width`, `Height`, `Alignment`) and save to trigger Hot Reload.
4. **WinForms Projects**: Edit `.Designer.cs` or `.cs` code (`Location`, `Size`, `Padding`, `Anchor`, `Dock`) and save.
5. Provide a summary of modified files, line numbers, updated properties, and technical rationale.

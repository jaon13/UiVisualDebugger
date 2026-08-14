# UiVisualDebugger

Standalone Real-Time UI Visual Debugger and Inspector for WPF (.NET 10/Core) & WinForms (.NET 4.8/Core) applications.

## Features
- **Zero Target Code Modification**: Inspects any running Windows app externally without modifying its source code.
- **Annotated Visual Screenshots (`annotated_ui.png`)**: Automatically draws red bounding boxes and `[1]`, `[2]` ID badges over controls.
- **Structured Visual Tree JSON (`antigravity_ui.json`)**: Exports Element Type, AutomationId, Name, Parent, Bounds (`X, Y, Width, Height`), and visibility.
- **AI Agent Integration**: Works seamlessly with AI agent coding assistants for automated visual UI debugging and code fixes.

## Quick Start

### Build & Run
```powershell
dotnet build UiVisualDebugger.csproj
dotnet run -- attach <ProcessName_or_PID> [OutputDir]
```

### Examples
```powershell
# Attach to running process PhMeter.WpfApp
UiVisualDebugger.exe attach PhMeter.WpfApp

# Attach by PID
UiVisualDebugger.exe attach 12345 ./artifacts
```

## Outputs
- `antigravity_ui.json`: Complete Visual Tree structure with coordinates
- `annotated_ui.png`: Rendered window screenshot with red bounding box ID badges

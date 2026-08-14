using System.Collections.Generic;

namespace UiVisualDebugger.Models;

public class UiElementSnapshot
{
    public int Id { get; set; }
    public string ControlType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AutomationId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ParentType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsOffscreen { get; set; }
    public RectSnapshot? Bounds { get; set; }
    public List<UiElementSnapshot> Children { get; set; } = new();
}

public class RectSnapshot
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

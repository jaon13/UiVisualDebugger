using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.UIA3;
using UiVisualDebugger.Models;

namespace UiVisualDebugger;

public class ExternalUiInspector
{
    public static (string jsonPath, string imagePath) AttachAndInspect(
        string processNameOrPid,
        string outputDirectory = ".",
        string jsonFileName = "antigravity_ui.json",
        string imageFileName = "annotated_ui.png")
    {
        Process? process = ResolveProcess(processNameOrPid);
        if (process == null)
        {
            throw new ArgumentException($"Could not find running process matching '{processNameOrPid}'");
        }

        using var automation = new UIA3Automation();
        var app = FlaUI.Core.Application.Attach(process.Id);

        Window? mainWindow = null;
        if (process.MainWindowHandle != IntPtr.Zero)
        {
            try
            {
                var elem = automation.FromHandle(process.MainWindowHandle);
                mainWindow = elem?.AsWindow();
            }
            catch { }
        }

        if (mainWindow == null)
        {
            try
            {
                var desktop = automation.GetDesktop();
                var processElements = desktop.FindAllChildren(cf => cf.ByProcessId(process.Id));
                var winElem = processElements.FirstOrDefault(c =>
                    c.Properties.ControlType.ValueOrDefault == FlaUI.Core.Definitions.ControlType.Window &&
                    !c.Properties.IsOffscreen.ValueOrDefault);

                winElem ??= processElements.FirstOrDefault(c =>
                    c.Properties.ControlType.ValueOrDefault == FlaUI.Core.Definitions.ControlType.Window);

                mainWindow = winElem?.AsWindow();
            }
            catch { }
        }

        if (mainWindow == null)
        {
            try
            {
                mainWindow = app.GetMainWindow(automation, TimeSpan.FromSeconds(3));
            }
            catch { }
        }

        if (mainWindow == null)
        {
            throw new InvalidOperationException($"Could not find main window for process '{process.ProcessName}' (PID {process.Id})");
        }

        Rectangle windowBounds = Rectangle.Empty;
        try { windowBounds = mainWindow.BoundingRectangle; } catch { }

        // 1. Traverse UI Tree & build flat list
        int idCounter = 1;
        var flatList = new List<UiElementSnapshot>();
        var rootSnapshot = BuildSnapshot(mainWindow, null, ref idCounter, flatList, windowBounds);

        Directory.CreateDirectory(outputDirectory);
        string jsonFullPath = Path.Combine(outputDirectory, jsonFileName);
        string imgFullPath = Path.Combine(outputDirectory, imageFileName);

        // 2. Export JSON
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        string jsonText = JsonSerializer.Serialize(rootSnapshot, options);
        File.WriteAllText(jsonFullPath, jsonText);

        // 3. Capture & Annotate Window Bitmap
        CaptureAndAnnotateImage(mainWindow, flatList, imgFullPath, windowBounds);

        return (jsonFullPath, imgFullPath);
    }

    private static Process? ResolveProcess(string query)
    {
        if (int.TryParse(query, out int pid))
        {
            try { return Process.GetProcessById(pid); } catch { }
        }

        string cleanName = query.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileNameWithoutExtension(query)
            : query;

        var procs = Process.GetProcessesByName(cleanName);
        if (procs.Length > 0) return procs[0];

        var all = Process.GetProcesses();
        return all.FirstOrDefault(p => p.ProcessName.Contains(cleanName, StringComparison.OrdinalIgnoreCase));
    }

    private static UiElementSnapshot BuildSnapshot(
        AutomationElement element,
        UiElementSnapshot? parent,
        ref int idCounter,
        List<UiElementSnapshot> flatList,
        Rectangle windowBounds)
    {
        string controlType = "Unknown";
        string name = "";
        string autoId = "";
        string className = "";
        bool isEnabled = false;
        bool isOffscreen = false;
        Rectangle bounds = Rectangle.Empty;

        try { controlType = element.Properties.ControlType.ValueOrDefault.ToString(); } catch { }
        try { name = element.Properties.Name.ValueOrDefault ?? ""; } catch { }
        try { autoId = element.Properties.AutomationId.ValueOrDefault ?? ""; } catch { }
        try { className = element.Properties.ClassName.ValueOrDefault ?? ""; } catch { }
        try { isEnabled = element.Properties.IsEnabled.ValueOrDefault; } catch { }
        try { isOffscreen = element.Properties.IsOffscreen.ValueOrDefault; } catch { }
        try { bounds = element.BoundingRectangle; } catch { }

        var snapshot = new UiElementSnapshot
        {
            Id = idCounter++,
            ControlType = controlType,
            Name = name,
            AutomationId = autoId,
            ClassName = className,
            ParentType = parent?.ControlType ?? "",
            ParentName = parent?.Name ?? "",
            IsEnabled = isEnabled,
            IsOffscreen = isOffscreen
        };

        if (bounds != Rectangle.Empty && windowBounds != Rectangle.Empty)
        {
            int relX = bounds.X - windowBounds.X;
            int relY = bounds.Y - windowBounds.Y;
            snapshot.Bounds = new RectSnapshot
            {
                X = relX,
                Y = relY,
                Width = bounds.Width,
                Height = bounds.Height
            };
        }

        flatList.Add(snapshot);

        try
        {
            var children = element.FindAllChildren();
            foreach (var child in children)
            {
                snapshot.Children.Add(BuildSnapshot(child, snapshot, ref idCounter, flatList, windowBounds));
            }
        }
        catch
        {
            // Ignore subtree access exceptions
        }

        return snapshot;
    }

    private static void CaptureAndAnnotateImage(Window mainWindow, List<UiElementSnapshot> elements, string outputPath, Rectangle windowBounds)
    {
        try
        {
            if (windowBounds.Width <= 0 || windowBounds.Height <= 0)
            {
                try { windowBounds = mainWindow.BoundingRectangle; } catch { }
            }

            if (windowBounds.Width <= 0 || windowBounds.Height <= 0)
            {
                Console.WriteLine("[UiVisualDebugger] Window is minimized or has zero bounds. Skipping capture.");
                return;
            }

            using Bitmap bitmap = CaptureWindowBitmap(mainWindow, windowBounds);
            using Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var redPen = new Pen(Color.Red, 2);
            using var font = new Font("Arial", 9, FontStyle.Bold);
            using var textBrush = Brushes.White;
            using var bgBrush = new SolidBrush(Color.FromArgb(210, 220, 20, 20));

            foreach (var el in elements)
            {
                if (el.Bounds == null) continue;
                var b = el.Bounds;
                if (b.Width <= 0 || b.Height <= 0) continue;

                var rect = new Rectangle(b.X, b.Y, b.Width, b.Height);
                g.DrawRectangle(redPen, rect);

                string badge = $"[{el.Id}] {(string.IsNullOrEmpty(el.AutomationId) ? el.Name : el.AutomationId)}".Trim();
                if (string.IsNullOrWhiteSpace(badge) || badge == $"[{el.Id}]")
                {
                    badge = $"[{el.Id}]";
                }

                SizeF sz = g.MeasureString(badge, font);
                float badgeY = Math.Max(0, b.Y - sz.Height);
                var badgeRect = new RectangleF(b.X, badgeY, sz.Width + 4, sz.Height);

                g.FillRectangle(bgBrush, badgeRect);
                g.DrawString(badge, font, textBrush, badgeRect.X + 2, badgeRect.Y);
            }

            bitmap.Save(outputPath, ImageFormat.Png);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UiVisualDebugger] Annotation error: {ex.Message}");
        }
    }

    private static Bitmap CaptureWindowBitmap(Window mainWindow, Rectangle windowBounds)
    {
        // Primary: FlaUI Capture
        try
        {
            using var captured = Capture.Rectangle(windowBounds);
            if (captured?.Bitmap != null)
            {
                return new Bitmap(captured.Bitmap);
            }
        }
        catch { }

        // Fallback: Pure Win32 GDI CopyFromScreen (100% fail-safe against COM 0x80040201 errors)
        Bitmap bmp = new Bitmap(windowBounds.Width, windowBounds.Height, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(windowBounds.Location, Point.Empty, windowBounds.Size);
        }
        return bmp;
    }
}

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
        var app = FlaUI.Core.Application.Attach(process);
        var mainWindow = app.GetMainWindow(automation);

        if (mainWindow == null)
        {
            throw new InvalidOperationException($"Could not find main window for process '{process.ProcessName}' (PID {process.Id})");
        }

        // 1. Traverse UI Tree & build flat list
        int idCounter = 1;
        var flatList = new List<UiElementSnapshot>();
        var rootSnapshot = BuildSnapshot(mainWindow, null, ref idCounter, flatList, mainWindow.BoundingRectangle);

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
        CaptureAndAnnotateImage(mainWindow, flatList, imgFullPath);

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
        var bounds = element.BoundingRectangle;
        var snapshot = new UiElementSnapshot
        {
            Id = idCounter++,
            ControlType = element.Properties.ControlType.Value.ToString(),
            Name = element.Properties.Name.ValueOrDefault ?? "",
            AutomationId = element.Properties.AutomationId.ValueOrDefault ?? "",
            ClassName = element.Properties.ClassName.ValueOrDefault ?? "",
            ParentType = parent?.ControlType ?? "",
            ParentName = parent?.Name ?? "",
            IsEnabled = element.Properties.IsEnabled.ValueOrDefault,
            IsOffscreen = element.Properties.IsOffscreen.ValueOrDefault
        };

        if (bounds != Rectangle.Empty)
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

    private static void CaptureAndAnnotateImage(Window mainWindow, List<UiElementSnapshot> elements, string outputPath)
    {
        try
        {
            using var image = Capture.Rectangle(mainWindow.BoundingRectangle);
            using var bitmap = new Bitmap(image.Bitmap);
            using var g = Graphics.FromImage(bitmap);
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
}

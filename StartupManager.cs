using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace UiVisualDebugger;

public static class StartupManager
{
    private const string RUN_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string APP_NAME = "UiVisualDebugger";

    public static bool IsStartupEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, false);
            string? value = key?.GetValue(APP_NAME) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StartupManager] Check error: {ex.Message}");
            return false;
        }
    }

    public static bool SetStartup(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, true);
            if (key == null) return false;

            if (enable)
            {
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                if (string.IsNullOrEmpty(exePath))
                {
                    exePath = Path.Combine(AppContext.BaseDirectory, "UiVisualDebugger.exe");
                }
                key.SetValue(APP_NAME, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(APP_NAME) != null)
                {
                    key.DeleteValue(APP_NAME, false);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StartupManager] Set error: {ex.Message}");
            return false;
        }
    }
}

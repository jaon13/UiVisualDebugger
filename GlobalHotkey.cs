using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UiVisualDebugger;

public class GlobalHotkey : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly HotkeyWindow _window = new();
    private readonly int _id;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;

    public const uint MOD_NONE = 0x0000;
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;

    public GlobalHotkey(uint modifiers, Keys key)
    {
        _id = GetHashCode();
        _window.HotkeyPressed += (s, e) => HotkeyPressed?.Invoke(this, EventArgs.Empty);

        if (!RegisterHotKey(_window.Handle, _id, modifiers, (uint)key))
        {
            System.Diagnostics.Debug.WriteLine($"[GlobalHotkey] Failed to register hotkey {modifiers} + {key}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            UnregisterHotKey(_window.Handle, _id);
            _window.Dispose();
            _disposed = true;
        }
    }

    private class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        public event EventHandler? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}

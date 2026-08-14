using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace UiVisualDebugger;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _rightClickMenu;
    private readonly ContextMenuStrip _leftClickMenu;
    private readonly GlobalHotkey _hotkey;
    private readonly ProcessWatcher _watcher;
    private readonly ToolStripMenuItem _autoDetectRightItem;
    private readonly ToolStripMenuItem _autoDetectLeftItem;

    public TrayApplicationContext()
    {
        _rightClickMenu = new ContextMenuStrip();
        _leftClickMenu = new ContextMenuStrip();

        _autoDetectRightItem = new ToolStripMenuItem("🔍 타겟 자동 감지 (ON)", null, ToggleAutoDetect) { Checked = true };
        _autoDetectLeftItem = new ToolStripMenuItem("🔍 타겟 자동 감지 (ON)", null, ToggleAutoDetect) { Checked = true };

        _rightClickMenu.Items.Add(new ToolStripMenuItem("📸 즉시 스냅샷 덤프 (F12)", null, OnCaptureClicked));
        _rightClickMenu.Items.Add(_autoDetectRightItem);
        _rightClickMenu.Items.Add(new ToolStripSeparator());
        _rightClickMenu.Items.Add(new ToolStripMenuItem("📁 결과 저장 폴더 열기", null, OnOpenFolderClicked));
        _rightClickMenu.Items.Add(new ToolStripSeparator());
        _rightClickMenu.Items.Add(new ToolStripMenuItem("❌ 종료", null, OnExitClicked));

        _leftClickMenu.Items.Add(new ToolStripMenuItem("⚡ [퀵 메뉴] UiVisualDebugger") { Enabled = false, Font = new Font(Control.DefaultFont, FontStyle.Bold) });
        _leftClickMenu.Items.Add(new ToolStripSeparator());
        _leftClickMenu.Items.Add(new ToolStripMenuItem("📸 스냅샷 덤프 생성 (F12)", null, OnCaptureClicked));
        _leftClickMenu.Items.Add(_autoDetectLeftItem);
        _leftClickMenu.Items.Add(new ToolStripMenuItem("📁 덤프 저장 폴더 열기", null, OnOpenFolderClicked));

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "UiVisualDebugger - Real-Time UI Inspector (F12)",
            Visible = true
        };

        _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        _hotkey = new GlobalHotkey(GlobalHotkey.MOD_NONE, Keys.F12);
        _hotkey.HotkeyPressed += (s, e) => CaptureCurrentSnapshot("Global Hotkey F12");

        _watcher = new ProcessWatcher("PhMeter.WpfApp");
        _watcher.ProcessStarted += proc =>
        {
            _notifyIcon.ShowBalloonTip(3000, "UiVisualDebugger", $"타겟 앱 '{proc.ProcessName}' 감지됨! 스냅샷 자동 생성 중...", ToolTipIcon.Info);
            CaptureCurrentSnapshot("Auto Process Watcher", proc);
        };
        _watcher.Start();

        _notifyIcon.ShowBalloonTip(3000, "UiVisualDebugger 상주 구동 완료", "시스템 트레이에서 대기 중입니다.\n[F12] 키를 누르면 스냅샷을 덤프합니다.", ToolTipIcon.Info);
    }

    private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi != null)
            {
                _notifyIcon.ContextMenuStrip = _leftClickMenu;
                mi.Invoke(_notifyIcon, null);
            }
            else
            {
                _leftClickMenu.Show(Cursor.Position);
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            _notifyIcon.ContextMenuStrip = _rightClickMenu;
        }
    }

    private void OnCaptureClicked(object? sender, EventArgs e)
    {
        CaptureCurrentSnapshot("Tray Menu Click");
    }

    private void ToggleAutoDetect(object? sender, EventArgs e)
    {
        _watcher.IsEnabled = !_watcher.IsEnabled;
        bool isEnabled = _watcher.IsEnabled;
        _autoDetectRightItem.Checked = isEnabled;
        _autoDetectLeftItem.Checked = isEnabled;
        _autoDetectRightItem.Text = $"🔍 타겟 자동 감지 ({(isEnabled ? "ON" : "OFF")})";
        _autoDetectLeftItem.Text = $"🔍 타겟 자동 감지 ({(isEnabled ? "ON" : "OFF")})";

        _notifyIcon.ShowBalloonTip(2000, "UiVisualDebugger", $"자동 프로세스 감지 기능이 {(isEnabled ? "활성화" : "비활성화")}되었습니다.", ToolTipIcon.Info);
    }

    private void OnOpenFolderClicked(object? sender, EventArgs e)
    {
        string dir = Path.GetFullPath(".");
        Process.Start("explorer.exe", dir);
    }

    private void CaptureCurrentSnapshot(string sourceTrigger, Process? targetProc = null)
    {
        try
        {
            string procName = targetProc?.ProcessName ?? "PhMeter.WpfApp";
            var (jsonFile, imgFile) = ExternalUiInspector.AttachAndInspect(procName, ".");

            _notifyIcon.ShowBalloonTip(2500, "📸 스냅샷 생성 완료", $"[Trigger: {sourceTrigger}]\n- JSON: {Path.GetFileName(jsonFile)}\n- Image: {Path.GetFileName(imgFile)}", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _notifyIcon.ShowBalloonTip(3000, "❌ 스냅샷 오류", ex.Message, ToolTipIcon.Warning);
        }
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        _watcher.Dispose();
        _hotkey.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Application.Exit();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UiVisualDebugger;

public class ProcessWatcher : IDisposable
{
    private readonly HashSet<int> _knownPids = new();
    private CancellationTokenSource? _cts;
    private Task? _watcherTask;
    private readonly string _targetProcessName;

    public event Action<Process>? ProcessStarted;

    public bool IsEnabled { get; set; } = true;

    public ProcessWatcher(string targetProcessName = "PhMeter.WpfApp")
    {
        _targetProcessName = targetProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? System.IO.Path.GetFileNameWithoutExtension(targetProcessName)
            : targetProcessName;
    }

    public void Start()
    {
        Stop();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        foreach (var p in Process.GetProcessesByName(_targetProcessName))
        {
            _knownPids.Add(p.Id);
        }

        _watcherTask = Task.Run(() => WatchLoop(token), token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _watcherTask = null;
    }

    private async Task WatchLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (IsEnabled)
                {
                    var currentProcs = Process.GetProcessesByName(_targetProcessName);
                    foreach (var proc in currentProcs)
                    {
                        if (!_knownPids.Contains(proc.Id))
                        {
                            _knownPids.Add(proc.Id);
                            ProcessStarted?.Invoke(proc);
                        }
                    }

                    _knownPids.RemoveWhere(pid =>
                    {
                        try { return Process.GetProcessById(pid).HasExited; }
                        catch { return true; }
                    });
                }
            }
            catch { }

            await Task.Delay(1000, token);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

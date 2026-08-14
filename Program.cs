using System;
using System.IO;
using System.Windows.Forms;

namespace UiVisualDebugger;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        Console.WriteLine("=== Antigravity UI Visual Debugger ===");

        if (args.Length == 0)
        {
            Console.WriteLine("Starting System Tray Resident Mode...");
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
            return 0;
        }

        string command = args[0].ToLowerInvariant();
        string targetProcess = args.Length > 1 ? args[1] : "PhMeter.WpfApp";
        string outputDir = args.Length > 2 ? args[2] : ".";

        try
        {
            switch (command)
            {
                case "attach":
                case "dump":
                    Console.WriteLine($"[1/2] Attaching to process '{targetProcess}'...");
                    var (jsonFile, imgFile) = ExternalUiInspector.AttachAndInspect(targetProcess, outputDir);
                    Console.WriteLine($"[2/2] Success!");
                    Console.WriteLine($"      JSON Dump:  {Path.GetFullPath(jsonFile)}");
                    Console.WriteLine($"      Annotated:  {Path.GetFullPath(imgFile)}");
                    return 0;

                case "tray":
                    Console.WriteLine("Starting System Tray Resident Mode...");
                    ApplicationConfiguration.Initialize();
                    Application.Run(new TrayApplicationContext());
                    return 0;

                case "help":
                default:
                    PrintUsage();
                    return 0;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine(@"
Usage:
  UiVisualDebugger.exe                      (Run as System Tray Resident App)
  UiVisualDebugger.exe attach <Process_or_PID> [OutputDir]
  UiVisualDebugger.exe dump   <Process_or_PID> [OutputDir]

Examples:
  UiVisualDebugger.exe                      -> Sits in System Tray with F12 hotkey & auto-watcher
  UiVisualDebugger.exe attach PhMeter.WpfApp -> One-shot CLI snapshot dump
");
    }
}

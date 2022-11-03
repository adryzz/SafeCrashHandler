using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Runtime;
using InvalidOperationException = System.InvalidOperationException;

namespace SafeCrashHandler;


/// <summary>
/// This class allows an app to safely handle crashes, along with a memory dump of the crashed application.
/// </summary>
public static class SafeCrashHandler
{
    internal static Mutex Mutex = null!;

    private static readonly string ModuleName = Assembly.GetEntryAssembly()!.EntryPoint!.DeclaringType!.FullName!;

    /// <summary>
    /// This event gets fired whenever the application is about to crash.
    /// </summary>
    public static event EventHandler<SafeCrashHandlerEventArgs>? OnCrash;
    
    /// <summary>
    /// This event gets fired whenever an unhandled exception occurs, and is fired before <see cref="OnCrash"/> fires.
    /// </summary>
    public static event EventHandler<UnhandledExceptionEventArgs>? OnUnhandledException;
    
    /// <summary>
    /// This event gets fired right before the application exits.
    /// </summary>
    public static event EventHandler? OnExit;

    /// <summary>
    /// This keeps count of how many times the application has crashed before.
    /// </summary>
    public static uint CrashCounter { get; private set; }
    
    /// <summary>
    /// If set to true, the crash handler will restart the application when it crashes.
    /// </summary>
    public static bool RestartAppOnCrash { get; set; }
    
    // Not really the intended purpose of a module initializer, but it does what i want
    //[ModuleInitializer]
    
    /// <summary>
    /// Sets up the crash handler. Should be called at the start of your entrypoint, after subscribing to the <see cref="SafeCrashHandler"/> events.
    /// </summary>
    public static void Start()
    {
        Mutex = new Mutex(true, ModuleName, out bool created);
        if (created)
        {
            do
            {
                if (CrashCounter > 0)
                {
                    Console.WriteLine($"[crash-handler] App restarted after {CrashCounter} crash(es)");
                }
                StartDaemon();
            } while (RestartAppOnCrash);
            
            Mutex.Dispose();
            Environment.Exit(0);
        }
        else
        {
            Mutex m = new Mutex(true, $"{ModuleName}1", out bool created1);
            if (!created1)
                throw new InvalidOperationException("[crash-handler] The mutex already exists");
            
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                OnUnhandledException?.Invoke(sender, eventArgs);
                m.ReleaseMutex();
                Debug.WriteLine("[crash-handler] Unhandled exception signaled to crash handler daemon!");
                m.WaitOne();
                Console.WriteLine("[crash-handler] Process about to crash...");
                m.Dispose();
                Mutex.Dispose();
            };
        }
    }

    private static void StartDaemon()
    {
        if (Environment.ProcessPath is { } path)
        {
            Process app = Process.Start(path, Environment.GetCommandLineArgs());
            Debug.WriteLine("[crash-handler] Crash handler started!");
            
            // This makes sure that the target app has created the mutex
            Thread.Sleep(200);
            
            Mutex m = Mutex.OpenExisting($"{ModuleName}1");

            while (!app.HasExited)
            {
                if (m.WaitOne(200))
                {
                    Console.WriteLine("[crash-handler] Crash detected!");
                    using DataTarget target = DataTarget.CreateSnapshotAndAttach(app.Id);
                    Console.WriteLine("[crash-handler] Memory dumped!");
                    OnCrash?.Invoke(null, new SafeCrashHandlerEventArgs(target));
                    // Use the memory dump, copy it or something
                    // You can also do stuff on the running (but paused) live process
                    
                    target.Dispose();
                    CrashCounter++;
                    m.ReleaseMutex();
                    Debug.WriteLine("[crash-handler] Exit signal sent!");
                    break;
                }
            }
                
            app.WaitForExit();
            
            m.Dispose();
            Debug.WriteLine("[crash-handler] App Exited!");
            if (app.ExitCode != 0)
            {
                Console.WriteLine("[crash-handler] Non-zero exit code!");
            }
            OnExit?.Invoke(null, EventArgs.Empty);
        }
        else
        {
            Console.WriteLine("[crash-handler] Error while starting the crash handler.");
        }
    }
}
# SafeCrashHandler
A .NET crash handler

`SafeCrashHandler` allows you to handle almost all .NET unhandled exceptions and crashes effortlessly.

## Getting Started

```csharp
static void Main(string[] args)
{
    // Choose if you want your app to restart when it crashes
    CrashHandler.RestartAppOnCrash = true;
    CrashHandler.Start();
    
    // Your code here
}
```

It also allows you to subscribe to the events that fire during a crash.

### Events

- `CrashHandler.UnhandledException` is the first event to fire, and as such, is limited in what you can do with it.

  It will report to you the unhandled exception, and that's it.

- `CrashHandler.Crash` is the second event, and is fired from a separate process.

  The main process is paused, a memory dump has been created and you are able to interact with the .NET CLR in many ways, from reading arbitrary memory   to managing threads and a whole lot more. For more info on what can be done, visit the [clrmd repository](https://github.com/microsoft/clrmd).

Remember to do everything you need to here, because the memory dump will be deleted afterwards if not copied elsewhere.

- `CrashHandler.Exit` is the last event to fire, and is also fired from a separate process.

  This event is fired ***after*** the application has crashed, but ***before*** a new instance starts up if you have `RestartAppOnCrash` set to `true`.

### Properties

- `RestartAppOnCrash` makes sure that the crash handler restarts the application whenever a crash occurs.

- `CrashCounter` counts how many times the application has crashed and has subsequently been restarted.
